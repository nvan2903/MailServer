using BLL;
using DotNetEnv;
using System.Diagnostics;

namespace GUI
{
    public partial class FormHome : Form
    {
        private ThreadBLL threadBLL;

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
                //string baseDir = Env.GetString("BASE_DIRECTORY") ?? "C:\\ServerBaseDir";
                //string defaultDomain = Env.GetString("DEFAULT_DOMAIN") ?? "example.com";


                string baseDir = Env.GetString("BASE_DIRECTORY");
                string defaultDomain = Env.GetString("DEFAULT_DOMAIN");

                if (baseDir == "")
                {
                    throw new Exception("BASE_DIRECTORY is required in .env file");
                }

             
                Debug.WriteLine($"BASE_DIRECTORY: {baseDir}");

                threadBLL = new ThreadBLL(baseDir, defaultDomain)
                {
                    LogSMTP = LogSMTP,
                    LogIMAP = LogIMAP,
                    LogFTP = LogFTP
                };
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
                threadBLL.StartServers();
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

        private void btnStopServer_Click(object sender, EventArgs e)
        {
            try
            {
                threadBLL.StopServers();
                UpdateServerStatus(lblSMTPStatus, false);
                UpdateServerStatus(lblIMAPStatus, false);
                UpdateServerStatus(lblFTPStatus, false);
                btnStartServer.Enabled = true;
                btnStopServer.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error stopping servers: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void FormHome_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                threadBLL.StopServers();
            }
            catch (Exception) { }
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

        private void LogSMTP(string message) => UpdateTextBox(txtScreenSMTP, message);
        private void LogIMAP(string message) => UpdateTextBox(txtScreenIMAP, message);
        private void LogFTP(string message) => UpdateTextBox(txtScreenFTP, message);
    }
}
