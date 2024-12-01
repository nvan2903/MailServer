using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using BLL;
using DotNetEnv;

namespace GUI
{
    public partial class FormHome : Form
    {
        private TcpListener smtpServer, imapServer, ftpServer;
        private Thread smtpThread, imapThread, ftpThread;
        private bool isRunning = false;
        private int smtpConnectionCount = 0, imapConnectionCount = 0, ftpConnectionCount = 0;
        private string baseDir, defaultDomain;

        public FormHome()
        {
            InitializeComponent();
            LoadEnvConfig();
            this.FormClosing += FormHome_FormClosing;
        }

        private void LoadEnvConfig()
        {
            try
            {
                Env.TraversePath().Load();
                baseDir = Env.GetString("BASE_DIRECTORY") ?? "C:\\ServerBaseDir";
                defaultDomain = Env.GetString("DEFAULT_DOMAIN") ?? "example.com";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load configuration: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            try
            {
                StartServers();
                StartListeningThreads();
                UpdateServerStatus(lblSMTPStatus, true);
                UpdateServerStatus(lblIMAPStatus, true);
                UpdateServerStatus(lblFTPStatus, true);
                btnStartServer.Enabled = false;
                btnStopServer.Enabled = true;
                AppendServerStartMessages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Can't start server: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        private void StartServers()
        {
            smtpServer = new TcpListener(IPAddress.Any, 25);
            imapServer = new TcpListener(IPAddress.Any, 143);
            ftpServer = new TcpListener(IPAddress.Any, 21);

            smtpServer.Start();
            imapServer.Start();
            ftpServer.Start();

            isRunning = true;
        }

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            StopServers();
            UpdateServerStatus(lblSMTPStatus, false);
            UpdateServerStatus(lblIMAPStatus, false);
            UpdateServerStatus(lblFTPStatus, false);
            btnStartServer.Enabled = true;
            btnStopServer.Enabled = false;
        }

        private void StartListeningThreads()
        {
            smtpThread = new Thread(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        TcpClient client = smtpServer.AcceptTcpClient();
                        Interlocked.Increment(ref smtpConnectionCount);
                        var smtpExecuteBLL = new SMTPExecuteBLL(client, LogSMTP, baseDir, defaultDomain);
                        smtpExecuteBLL.Start();
                    }
                    catch (SocketException) { break; }
                }
            });

            imapThread = new Thread(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        TcpClient client = imapServer.AcceptTcpClient();
                        Interlocked.Increment(ref imapConnectionCount);
                        var imapExecuteBLL = new IMAPExecuteBLL(client, LogIMAP, baseDir, defaultDomain);
                        imapExecuteBLL.Start();
                    }
                    catch (SocketException) { break; }
                }
            });

            ftpThread = new Thread(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        TcpClient client = ftpServer.AcceptTcpClient();
                        Interlocked.Increment(ref ftpConnectionCount);
                        var ftpExecuteBLL = new FTPExcuteBLL(client, LogFTP, baseDir, defaultDomain);
                        ftpExecuteBLL.Start();
                    }
                    catch (SocketException) { break; }
                }
            });

            smtpThread.IsBackground = true;
            imapThread.IsBackground = true;
            ftpThread.IsBackground = true;

            smtpThread.Start();
            imapThread.Start();
            ftpThread.Start();
        }

        private void StopServers()
        {
            isRunning = false;

            try
            {
                smtpServer?.Stop();
                imapServer?.Stop();
                ftpServer?.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping servers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            smtpThread?.Join();
            imapThread?.Join();
            ftpThread?.Join();

            smtpConnectionCount = 0;
            imapConnectionCount = 0;
            ftpConnectionCount = 0;
        }

        private void LogSMTP(string message) => UpdateTextBox(txtScreenSMTP, message);
        private void LogIMAP(string message) => UpdateTextBox(txtScreenIMAP, message);
        private void LogFTP(string message) => UpdateTextBox(txtScreenFTP, message);

        private void UpdateTextBox(TextBox textBox, string message)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() =>
                {
                    textBox.AppendText($"{message}{Environment.NewLine}");
                    textBox.ScrollToCaret();
                }));
            }
            else
            {
                textBox.AppendText($"{message}{Environment.NewLine}");
                textBox.ScrollToCaret();
            }
        }

        private void UpdateServerStatus(Label label, bool isRunning)
        {
            label.Text = isRunning ? "Running" : "Stopped";
            label.ForeColor = isRunning ? Color.Green : Color.Red;
        }

        private void AppendServerStartMessages()
        {
            txtScreenSMTP.AppendText("SMTP Server is starting to listen...\n");
            txtScreenIMAP.AppendText("IMAP Server is starting to listen...\n");
            txtScreenFTP.AppendText("FTP Server is starting to listen...\n");
        }


        private void FormHome_FormClosing(object sender, FormClosingEventArgs e) => StopServers();

        private void btnSaveLog_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    File.WriteAllText(saveFileDialog.FileName, txtScreenSMTP.Text + txtScreenIMAP.Text + txtScreenFTP.Text);
                    MessageBox.Show("Logs saved successfully!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
    }
}
