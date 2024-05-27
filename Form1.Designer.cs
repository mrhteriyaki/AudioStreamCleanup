using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace AudioStreamCleanup
{
    public partial class Form1 : Form
    {

        // Form overrides dispose to clean up the component list.
        [DebuggerNonUserCode()]
        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing && components is not null)
                {
                    components.Dispose();
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        // Required by the Windows Form Designer
        private System.ComponentModel.IContainer components;

        // NOTE: The following procedure is required by the Windows Form Designer
        // It can be modified using the Windows Form Designer.  
        // Do not modify it using the code editor.
        [DebuggerStepThrough()]
        private void InitializeComponent()
        {
            btnScan = new Button();
            btnScan.Click += new EventHandler(btnScan_Click);
            lblStatus = new Label();
            txtScanLocation = new TextBox();
            txtScanLocation.TextChanged += new EventHandler(txtScanLocation_TextChanged);
            btnProcess = new Button();
            btnProcess.Click += new EventHandler(btnProcess_Click);
            btnCheckAll = new Button();
            btnCheckAll.Click += new EventHandler(btnCheckAll_Click);
            btnCheckNone = new Button();
            btnCheckNone.Click += new EventHandler(btnCheckNone_Click);
            btnPreviousPage = new Button();
            btnPreviousPage.Click += new EventHandler(btnPreviousPage_Click);
            btnNextPage = new Button();
            btnNextPage.Click += new EventHandler(btnNextPage_Click);
            lblPage = new Label();
            gbxMedia = new GroupBox();
            gbxMedia.Enter += new EventHandler(gbxMedia_Enter);
            txtThreads = new TextBox();
            txtThreads.TextChanged += new EventHandler(txtThreads_TextChanged);
            Label1 = new Label();
            Label2 = new Label();
            SuspendLayout();
            // 
            // btnScan
            // 
            btnScan.Location = new Point(333, 12);
            btnScan.Name = "btnScan";
            btnScan.Size = new Size(75, 23);
            btnScan.TabIndex = 0;
            btnScan.Text = "Scan Folder";
            btnScan.UseVisualStyleBackColor = true;
            // 
            // lblStatus
            // 
            lblStatus.AutoSize = true;
            lblStatus.Location = new Point(12, 64);
            lblStatus.Name = "lblStatus";
            lblStatus.Size = new Size(44, 15);
            lblStatus.TabIndex = 1;
            lblStatus.Text = "Status:";
            // 
            // txtScanLocation
            // 
            txtScanLocation.Location = new Point(50, 13);
            txtScanLocation.Name = "txtScanLocation";
            txtScanLocation.Size = new Size(277, 20);
            txtScanLocation.TabIndex = 2;
            // 
            // btnProcess
            // 
            btnProcess.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnProcess.Location = new Point(1398, 10);
            btnProcess.Name = "btnProcess";
            btnProcess.Size = new Size(125, 23);
            btnProcess.TabIndex = 3;
            btnProcess.Text = "Process Selected";
            btnProcess.UseVisualStyleBackColor = true;
            // 
            // btnCheckAll
            // 
            btnCheckAll.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCheckAll.Location = new Point(1317, 10);
            btnCheckAll.Name = "btnCheckAll";
            btnCheckAll.Size = new Size(75, 23);
            btnCheckAll.TabIndex = 4;
            btnCheckAll.Text = "Check All";
            btnCheckAll.UseVisualStyleBackColor = true;
            // 
            // btnCheckNone
            // 
            btnCheckNone.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCheckNone.Location = new Point(1236, 10);
            btnCheckNone.Name = "btnCheckNone";
            btnCheckNone.Size = new Size(75, 23);
            btnCheckNone.TabIndex = 5;
            btnCheckNone.Text = "Check None";
            btnCheckNone.UseVisualStyleBackColor = true;
            // 
            // btnPreviousPage
            // 
            btnPreviousPage.Location = new Point(414, 11);
            btnPreviousPage.Name = "btnPreviousPage";
            btnPreviousPage.Size = new Size(89, 23);
            btnPreviousPage.TabIndex = 6;
            btnPreviousPage.Text = "Previous Page";
            btnPreviousPage.UseVisualStyleBackColor = true;
            // 
            // btnNextPage
            // 
            btnNextPage.Location = new Point(509, 11);
            btnNextPage.Name = "btnNextPage";
            btnNextPage.Size = new Size(89, 23);
            btnNextPage.TabIndex = 7;
            btnNextPage.Text = "Next Page";
            btnNextPage.UseVisualStyleBackColor = true;
            // 
            // lblPage
            // 
            lblPage.AutoSize = true;
            lblPage.Location = new Point(604, 15);
            lblPage.Name = "lblPage";
            lblPage.Size = new Size(59, 15);
            lblPage.TabIndex = 8;
            lblPage.Text = "Page: 0/0";
            // 
            // gbxMedia
            // 
            gbxMedia.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;

            gbxMedia.Location = new Point(10, 85);
            gbxMedia.Name = "gbxMedia";
            gbxMedia.Size = new Size(1513, 73);
            gbxMedia.TabIndex = 9;
            gbxMedia.TabStop = false;
            gbxMedia.Text = "Media Files";
            // 
            // txtThreads
            // 
            txtThreads.Location = new Point(99, 39);
            txtThreads.Name = "txtThreads";
            txtThreads.Size = new Size(46, 20);
            txtThreads.TabIndex = 10;
            txtThreads.Text = "4";
            // 
            // Label1
            // 
            Label1.AutoSize = true;
            Label1.Location = new Point(12, 42);
            Label1.Name = "Label1";
            Label1.Size = new Size(86, 15);
            Label1.TabIndex = 11;
            Label1.Text = "Scan Threads:";
            // 
            // Label2
            // 
            Label2.AutoSize = true;
            Label2.Location = new Point(12, 16);
            Label2.Name = "Label2";
            Label2.Size = new Size(35, 15);
            Label2.TabIndex = 12;
            Label2.Text = "Path:";
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(6.0f, 13.0f);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1537, 173);
            Controls.Add(Label2);
            Controls.Add(Label1);
            Controls.Add(txtThreads);
            Controls.Add(gbxMedia);
            Controls.Add(lblPage);
            Controls.Add(btnNextPage);
            Controls.Add(btnPreviousPage);
            Controls.Add(btnCheckNone);
            Controls.Add(btnCheckAll);
            Controls.Add(btnProcess);
            Controls.Add(txtScanLocation);
            Controls.Add(lblStatus);
            Controls.Add(btnScan);
            Name = "Form1";
            Text = "Audio Stream Cleanup Tool";
            WindowState = FormWindowState.Maximized;
            Load += new EventHandler(Form1_Load);
            FormClosing += new FormClosingEventHandler(Form1_FormClosing);
            ResumeLayout(false);
            PerformLayout();

        }

        internal Button btnScan;
        internal Label lblStatus;
        internal TextBox txtScanLocation;
        internal Button btnProcess;
        internal Button btnCheckAll;
        internal Button btnCheckNone;
        internal Button btnPreviousPage;
        internal Button btnNextPage;
        internal Label lblPage;
        internal GroupBox gbxMedia;
        internal TextBox txtThreads;
        internal Label Label1;
        internal Label Label2;
    }
}