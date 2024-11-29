using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using DTO;
using DAL;

namespace BLL
{
    public class SMTPExecuteBLL
    {
        private readonly TcpClient _client;
        private readonly Action<string> _logAction; // Action để ghi log
        private readonly MailDAL _mailDAL; // DAL để lưu vào database
        private readonly string _baseDir; // Thư mục lưu tài khoản email
        private readonly string _defaultDomain; // Domain mặc định là @vku.udn.vn
        

        private string _senderEmail = string.Empty;
        private string _receiverEmail = string.Empty;
        private readonly StringBuilder _emailData = new(); // Lưu nội dung email
        private bool _isDataMode = false;
        private bool _mailFromReceived = false;
        private bool _rcptToReceived = false;

        public SMTPExecuteBLL(TcpClient client, Action<string> logAction,  string baseDir, string defaultDomain)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
             _mailDAL = new MailDAL() ;
            _baseDir = baseDir ?? throw new ArgumentNullException(nameof(baseDir));
            _defaultDomain = defaultDomain ?? throw new ArgumentNullException(nameof(defaultDomain));
           
        }

        public void Start()
        {
            try
            {
                using NetworkStream stream = _client.GetStream();
                using StreamReader reader = new(stream, Encoding.ASCII);
                using StreamWriter writer = new(stream, Encoding.ASCII) { AutoFlush = true };

                Log("Connection established with the client.");
                writer.WriteLine("220 Welcome to Simple SMTP Server");
                Log("220 Welcome to Simple SMTP Server");

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    Log($"Client: {line}");
                    string response;

                    if (_isDataMode)
                    {
                        response = HandleDataInput(line, writer);
                    }
                    else
                    {
                        response = ProcessCommand(line);
                        writer.WriteLine(response);
                        Log($"Server: {response}");
                    }

                    if (line.ToUpper() == "QUIT")
                        break;
                }
            }
            catch (IOException ex)
            {
                Log($"Connection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log($"Unexpected error: {ex.Message}");
            }
            finally
            {
                _client.Close();
                Log("Connection has been closed.");
            }
        }

        private string HandleDataInput(string line, StreamWriter writer)
        {
            if (line == ".")
            {
                _isDataMode = false;
                SaveEmail(); // Gọi hàm lưu email
                writer.WriteLine("250 OK: Message accepted for delivery");
                Log("Email has been accepted for delivery.");
                return "250 OK";
            }

            _emailData.AppendLine(line);
            return string.Empty;
        }

        private string ProcessCommand(string command)
        {
            if (command.StartsWith("HELO", StringComparison.OrdinalIgnoreCase))
            {
                return "250 Hello, pleased to meet you";
            }
            else if (command.StartsWith("MAIL FROM:", StringComparison.OrdinalIgnoreCase))
            {
                return HandleMailFrom(command);
            }
            else if (command.StartsWith("RCPT TO:", StringComparison.OrdinalIgnoreCase))
            {
                return HandleRcptTo(command);
            }
            else if (command.StartsWith("DATA", StringComparison.OrdinalIgnoreCase))
            {
                return HandleDataCommand();
            }
            else if (command.ToUpper() == "QUIT")
            {
                return "221 Bye";
            }
            else
            {
                return "502 Command not implemented";
            }
        }

        private string HandleMailFrom(string command)
        {
            _senderEmail = ExtractEmail(command, "MAIL FROM:");
            if (ValidateEmail(_senderEmail))
            {
                _mailFromReceived = true;
                _rcptToReceived = false; // Reset trạng thái RCPT TO
                return "250 OK";
            }
            return "550 Invalid sender email";
        }

        private string HandleRcptTo(string command)
        {
            if (!_mailFromReceived)
                return "503 Bad sequence of commands";

            _receiverEmail = ExtractEmail(command, "RCPT TO:");
            if (ValidateEmail(_receiverEmail))
            {
                _rcptToReceived = true;
                return "250 OK";
            }
            return "550 Invalid recipient email";
        }

        private string HandleDataCommand()
        {
            if (!_mailFromReceived || !_rcptToReceived)
                return "503 Bad sequence of commands";

            _isDataMode = true;
            _emailData.Clear();
            return "354 End data with <CR><LF>.<CR><LF>";
        }

        private string ExtractEmail(string command, string prefix)
        {
            return command.Substring(prefix.Length).Trim('<', '>', ' ').ToLower();
        }

        private void SaveEmail()
        {
            try
            {
                if (string.IsNullOrEmpty(_receiverEmail))
                    throw new InvalidOperationException("Recipient email is not set.");

                // Tạo thư mục theo email người nhận 
                // vantn.21it@vku.udn.vn
                //baseDir -> D\\Source-CSharp\\MailServer\\BaseDir
                // vantn.21it -> directory
                // @vku.udn.vn -> defaultDomain
                string directory = Path.Combine(_baseDir, _receiverEmail.Replace("@", "_"));
                Directory.CreateDirectory(directory);

                // Lưu file email
                string filePath = Path.Combine(directory, $"{DateTime.Now:yyyyMMdd_HHmmss}.eml");
                File.WriteAllText(filePath, _emailData.ToString());

                // Tạo đối tượng Mail DTO
                var mail = new Mail
                {
                    CreatedAt = DateTime.Now,
                    Sender = _senderEmail,
                    Receiver = _receiverEmail,
                    Owner = _receiverEmail,
                    IsRead = false,
                    Attachment = null,
                    Subject = ExtractSubject(), // Lấy Subject từ dữ liệu email
                    Content = filePath, // Lưu đường dẫn file vào Content
                    Reply = 0
                };

                // Gọi MailDAL để lưu vào cơ sở dữ liệu
                if (_mailDAL.InsertMail(mail))
                {
                    Log($"Email saved to {filePath} and inserted into the database.");
                }
                else
                {
                    Log("Failed to insert email into the database.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error saving email: {ex.Message}");
            }
        }

        private string ExtractSubject()
        {
            var emailLines = _emailData.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
            foreach (var line in emailLines)
            {
                if (line.StartsWith("Subject:", StringComparison.OrdinalIgnoreCase))
                {
                    return line.Substring(8).Trim();
                }
            }
            return "No Subject"; // Mặc định nếu không tìm thấy Subject
        }

        private bool ValidateEmail(string email)
        {
            return email.Contains("@") && email.Contains(".");
        }

        private void Log(string message)
        {
            _logAction?.Invoke($"{DateTime.Now:HH:mm:ss} - {message}");
        }
    }
}
