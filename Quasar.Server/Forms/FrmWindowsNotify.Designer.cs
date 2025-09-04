namespace Quasar.Server.Forms
{
    partial class FrmWindowsNotify
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListBox lstKeywords;
        private System.Windows.Forms.TextBox txtKeyword;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.ListView lstEvents;
        private System.Windows.Forms.ColumnHeader hUser;
        private System.Windows.Forms.ColumnHeader hTime;
        private System.Windows.Forms.ColumnHeader hKeyword;
        private System.Windows.Forms.ColumnHeader hWindow;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.lstKeywords = new System.Windows.Forms.ListBox();
            this.txtKeyword = new System.Windows.Forms.TextBox();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnRemove = new System.Windows.Forms.Button();
            this.lstEvents = new System.Windows.Forms.ListView();
            this.hUser = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hKeyword = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.hWindow = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.SuspendLayout();
            // 
            // lstKeywords
            // 
            this.lstKeywords.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstKeywords.FormattingEnabled = true;
            this.lstKeywords.IntegralHeight = false;
            this.lstKeywords.Location = new System.Drawing.Point(12, 12);
            this.lstKeywords.Name = "lstKeywords";
            this.lstKeywords.Size = new System.Drawing.Size(220, 386);
            // 
            // txtKeyword
            // 
            this.txtKeyword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.txtKeyword.Location = new System.Drawing.Point(12, 404);
            this.txtKeyword.Name = "txtKeyword";
            this.txtKeyword.Size = new System.Drawing.Size(220, 22);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Location = new System.Drawing.Point(238, 404);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 23);
            this.btnAdd.Text = "Add";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemove.Location = new System.Drawing.Point(319, 404);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 23);
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // lstEvents
            // 
            this.lstEvents.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstEvents.FullRowSelect = true;
            this.lstEvents.Location = new System.Drawing.Point(238, 12);
            this.lstEvents.Name = "lstEvents";
            this.lstEvents.Size = new System.Drawing.Size(730, 386);
            this.lstEvents.View = System.Windows.Forms.View.Details;
            this.lstEvents.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] { this.hUser, this.hTime, this.hKeyword, this.hWindow });
            // 
            // Column headers
            // 
            this.hUser.Text = "User"; this.hUser.Width = 160;
            this.hTime.Text = "Time"; this.hTime.Width = 160;
            this.hKeyword.Text = "Keyword"; this.hKeyword.Width = 160;
            this.hWindow.Text = "Active Window"; this.hWindow.Width = 240;
            // 
            // FrmWindowsNotify
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(980, 440);
            this.Controls.Add(this.lstEvents);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.txtKeyword);
            this.Controls.Add(this.lstKeywords);
            this.Font = new System.Drawing.Font("Segoe UI", 8.25F);
            this.Name = "FrmWindowsNotify";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Windows Notify";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}


