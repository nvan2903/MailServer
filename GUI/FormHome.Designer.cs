namespace GUI
{
    partial class FormHome
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnStartServer = new Button();
            lblSMTP = new Label();
            lblIMAP = new Label();
            lblFTP = new Label();
            txtScreenSMTP = new TextBox();
            txtScreenIMAP = new TextBox();
            txtScreenFTP = new TextBox();
            lblSMTPStatus = new Label();
            lblIMAPStatus = new Label();
            lblFTPStatus = new Label();
            btnStopServer = new Button();
            btnSaveLog = new Button();
            SuspendLayout();
            // 
            // btnStartServer
            // 
            btnStartServer.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            btnStartServer.Location = new Point(21, 23);
            btnStartServer.Name = "btnStartServer";
            btnStartServer.Size = new Size(258, 72);
            btnStartServer.TabIndex = 0;
            btnStartServer.Text = "START SERVER";
            btnStartServer.UseVisualStyleBackColor = true;
            btnStartServer.Click += btnStartServer_Click;
            // 
            // lblSMTP
            // 
            lblSMTP.AutoSize = true;
            lblSMTP.Font = new Font("Segoe UI Semibold", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblSMTP.Location = new Point(250, 161);
            lblSMTP.Name = "lblSMTP";
            lblSMTP.Size = new Size(74, 31);
            lblSMTP.TabIndex = 1;
            lblSMTP.Text = "SMTP";
            // 
            // lblIMAP
            // 
            lblIMAP.AutoSize = true;
            lblIMAP.Font = new Font("Segoe UI Semibold", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblIMAP.Location = new Point(922, 161);
            lblIMAP.Name = "lblIMAP";
            lblIMAP.Size = new Size(70, 31);
            lblIMAP.TabIndex = 2;
            lblIMAP.Text = "IMAP";
            // 
            // lblFTP
            // 
            lblFTP.AutoSize = true;
            lblFTP.Font = new Font("Segoe UI Semibold", 13.8F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lblFTP.Location = new Point(1576, 161);
            lblFTP.Name = "lblFTP";
            lblFTP.Size = new Size(52, 31);
            lblFTP.TabIndex = 3;
            lblFTP.Text = "FTP";
            // 
            // txtScreenSMTP
            // 
            txtScreenSMTP.Location = new Point(21, 233);
            txtScreenSMTP.Multiline = true;
            txtScreenSMTP.Name = "txtScreenSMTP";
            txtScreenSMTP.ReadOnly = true;
            txtScreenSMTP.Size = new Size(611, 589);
            txtScreenSMTP.TabIndex = 4;
            // 
            // txtScreenIMAP
            // 
            txtScreenIMAP.Location = new Point(656, 233);
            txtScreenIMAP.Multiline = true;
            txtScreenIMAP.Name = "txtScreenIMAP";
            txtScreenIMAP.ReadOnly = true;
            txtScreenIMAP.Size = new Size(611, 589);
            txtScreenIMAP.TabIndex = 5;
            // 
            // txtScreenFTP
            // 
            txtScreenFTP.Location = new Point(1290, 233);
            txtScreenFTP.Multiline = true;
            txtScreenFTP.Name = "txtScreenFTP";
            txtScreenFTP.ReadOnly = true;
            txtScreenFTP.Size = new Size(611, 589);
            txtScreenFTP.TabIndex = 7;
            // 
            // lblSMTPStatus
            // 
            lblSMTPStatus.AutoSize = true;
            lblSMTPStatus.Location = new Point(260, 207);
            lblSMTPStatus.Name = "lblSMTPStatus";
            lblSMTPStatus.Size = new Size(55, 23);
            lblSMTPStatus.TabIndex = 8;
            lblSMTPStatus.Text = "label1";
            // 
            // lblIMAPStatus
            // 
            lblIMAPStatus.AutoSize = true;
            lblIMAPStatus.Location = new Point(937, 207);
            lblIMAPStatus.Name = "lblIMAPStatus";
            lblIMAPStatus.Size = new Size(55, 23);
            lblIMAPStatus.TabIndex = 9;
            lblIMAPStatus.Text = "label2";
            // 
            // lblFTPStatus
            // 
            lblFTPStatus.AutoSize = true;
            lblFTPStatus.Location = new Point(1573, 207);
            lblFTPStatus.Name = "lblFTPStatus";
            lblFTPStatus.Size = new Size(55, 23);
            lblFTPStatus.TabIndex = 10;
            lblFTPStatus.Text = "label3";
            // 
            // btnStopServer
            // 
            btnStopServer.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            btnStopServer.Location = new Point(301, 23);
            btnStopServer.Name = "btnStopServer";
            btnStopServer.Size = new Size(258, 72);
            btnStopServer.TabIndex = 11;
            btnStopServer.Text = "STOP SERVER";
            btnStopServer.UseVisualStyleBackColor = true;
            btnStopServer.Click += btnStopServer_Click;
            // 
            // btnSaveLog
            // 
            btnSaveLog.Font = new Font("Segoe UI Semibold", 14F, FontStyle.Bold);
            btnSaveLog.Location = new Point(586, 23);
            btnSaveLog.Name = "btnSaveLog";
            btnSaveLog.Size = new Size(258, 72);
            btnSaveLog.TabIndex = 12;
            btnSaveLog.Text = "SAVE LOG";
            btnSaveLog.UseVisualStyleBackColor = true;
            btnSaveLog.Click += btnSaveLog_Click;
            // 
            // FormHome
            // 
            AutoScaleDimensions = new SizeF(9F, 23F);
            AutoScaleMode = AutoScaleMode.Font;
            AutoScroll = true;
            BackColor = SystemColors.ButtonHighlight;
            ClientSize = new Size(1924, 1055);
            Controls.Add(btnSaveLog);
            Controls.Add(btnStopServer);
            Controls.Add(lblFTPStatus);
            Controls.Add(lblIMAPStatus);
            Controls.Add(lblSMTPStatus);
            Controls.Add(txtScreenFTP);
            Controls.Add(txtScreenIMAP);
            Controls.Add(txtScreenSMTP);
            Controls.Add(lblFTP);
            Controls.Add(lblIMAP);
            Controls.Add(lblSMTP);
            Controls.Add(btnStartServer);
            Font = new Font("Segoe UI", 10.2F, FontStyle.Regular, GraphicsUnit.Point, 0);
            Name = "FormHome";
            Text = "Mail Server";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button btnStartServer;
        private Label lblSMTP;
        private Label lblIMAP;
        private Label lblFTP;
        private TextBox txtScreenSMTP;
        private TextBox txtScreenIMAP;
        private TextBox txtScreenFTP;
        private Label lblSMTPStatus;
        private Label lblIMAPStatus;
        private Label lblFTPStatus;
        private Button btnStopServer;
        private Button btnSaveLog;
    }
}