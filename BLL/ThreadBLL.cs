using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using BLL;

namespace BLL
{
    public class ThreadBLL
    {
        private TcpListener smtpServer, imapServer, ftpServer;
        private Thread smtpThread, imapThread, ftpThread;
        private bool isRunning = false;

        private int smtpConnectionCount = 0, imapConnectionCount = 0, ftpConnectionCount = 0;
        private readonly string baseDir;
        private readonly string defaultDomain;

        // Delegate for logging
        public Action<string> LogSMTP { get; set; }
        public Action<string> LogIMAP { get; set; }
        public Action<string> LogFTP { get; set; }

        public ThreadBLL(string baseDir, string defaultDomain)
        {
            this.baseDir = baseDir;
            this.defaultDomain = defaultDomain;
        }

        public void StartServers()
        {
            smtpServer = new TcpListener(IPAddress.Any, 25);
            imapServer = new TcpListener(IPAddress.Any, 143);
            ftpServer = new TcpListener(IPAddress.Any, 21);

            smtpServer.Start();
            imapServer.Start();
            ftpServer.Start();

            isRunning = true;

            StartListeningThreads();
        }

        public void StopServers()
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
                throw new Exception($"Error stopping servers: {ex.Message}");
            }

            smtpThread?.Join();
            imapThread?.Join();
            ftpThread?.Join();

            smtpConnectionCount = 0;
            imapConnectionCount = 0;
            ftpConnectionCount = 0;
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
    }
}
