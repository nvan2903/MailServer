
using System.Net;
using System.Net.Sockets;
using BLL;
using DotNetEnv;

namespace GUI
{
    public partial class FormHome : Form
    {
        private TcpListener smtpServer, imapServer, ftpServer;
        private Thread smtpThread, imapThread, ftpThread;
        private bool isRunning = false; // Để kiểm soát việc dừng luồng
        string baseDir;
        string defaultDomain;

        public FormHome()
        {
            InitializeComponent();
            // Provide the additional parameters
           
        }

        private void btnStartServer_Click(object sender, EventArgs e)
        {
            try
            {           
                Env.TraversePath().Load();          // Tải các biến môi trường từ file .env
                baseDir = Env.GetString("BASE_DIRECTORY");
                defaultDomain = Env.GetString("DEFAULT_DOMAIN");
                StartServers();
                StartListeningThreads();
                this.FormClosing += FormHome_FormClosing;
                btnStartServer.Enabled = false;
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

            isRunning = true; // Đánh dấu server đã khởi chạy
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

                        // Instantiate SMTPExecuteBLL with the required parameters
                        var smtpExecuteBLL = new SMTPExecuteBLL(client, LogSMTP, baseDir, defaultDomain);

                        // Start handling SMTP commands for the client
                        smtpExecuteBLL.Start();
                    }
                    catch (SocketException) { break; } // Kết thúc khi server dừng
                }
            });

            imapThread = new Thread(() =>
            {
                while (isRunning)
                {
                    try
                    {
                        TcpClient client = imapServer.AcceptTcpClient();
                        var imapExecuteBLL = new IMAPExecuteBLL(client, LogIMAP, baseDir,defaultDomain);
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
                        var ftpExecuteBLL = new FTPExcuteBLL(client, LogFTP,baseDir,defaultDomain);
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

        private void LogSMTP(string message)
        {
            UpdateTextBox(txtScreenSMTP, message);
        }

        private void LogIMAP(string message)
        {
            UpdateTextBox(txtScreenIMAP, message);
        }

        private void LogFTP(string message)
        {
            UpdateTextBox(txtScreenFTP, message);
        }

        private void UpdateTextBox(TextBox textBox, string message)
        {
            if (textBox.InvokeRequired)
            {
                textBox.Invoke(new Action(() => textBox.AppendText($"{message}{Environment.NewLine}")));
            }
            else
            {
                textBox.AppendText($"{message}{Environment.NewLine}");
            }
        }


        private void FormHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            StopServers();
        }

        private void StopServers()
        {
            isRunning = false; // Ngừng các vòng lặp trong các luồng

            try
            {
                smtpServer?.Stop();
                imapServer?.Stop();
                ftpServer?.Stop();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error closing servers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            // Đợi các luồng kết thúc
            smtpThread?.Join();
            imapThread?.Join();
            ftpThread?.Join();
        }

        private void AppendServerStartMessages()
        {
            txtScreenSMTP.AppendText("SMTP Server is starting to listen...\n");
            txtScreenIMAP.AppendText("IMAP Server is starting to listen...\n");
            txtScreenFTP.AppendText("FTP Server is starting to listen...\n");
        }
    }
}