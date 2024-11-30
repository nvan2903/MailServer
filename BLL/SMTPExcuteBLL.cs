using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using DAL;
using DTO;

namespace BLL
{
    public class SMTPExecuteBLL
    {
        private readonly TcpClient _client;
        private readonly Action<string> _logAction;
        private readonly AccountDAL _accountDAL;
        private readonly MailDAL _mailDAL;
        private readonly string _baseDir;
        private readonly string _defaultDomain;
        private Account _currentUser;
        private bool _isAuthenticated;
        private string _senderEmail;
        private string _receiverEmail;
        private StringBuilder _emailData;

        public SMTPExecuteBLL(TcpClient client, Action<string> logAction, string baseDir, string defaultDomain)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
            _accountDAL = new AccountDAL();
            _mailDAL = new MailDAL();
            _baseDir = baseDir ?? throw new ArgumentNullException(nameof(baseDir));
            _defaultDomain = defaultDomain ?? throw new ArgumentNullException(nameof(defaultDomain));
            _emailData = new StringBuilder();
        }

        public void Start()
        {
            using (_client)
            {
                try
                {
                    using NetworkStream stream = _client.GetStream();
                    using StreamReader reader = new(stream, Encoding.UTF8);
                    using StreamWriter writer = new(stream, Encoding.UTF8) { AutoFlush = true };

                    Log("Connection established.");
                    writer.WriteLine("220 SMTP Server Ready");

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Log($"Client: {line}");
                        string response = ProcessCommand(line);
                        writer.WriteLine(response);
                        Log($"Server: {response}");

                        if (response.StartsWith("221"))
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Log($"Error: {ex.Message}");
                }
                finally
                {
                    Log("Connection closed.");
                }
            }
        }

        private string ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return "500 Syntax error, command unrecognized";

            try
            {
                if (!command.TrimStart().StartsWith("{"))
                    return "500 Invalid command format: Must be JSON";

                var jsonCommand = JsonConvert.DeserializeObject<Dictionary<string, string>>(command);
                if (jsonCommand == null || !jsonCommand.ContainsKey("Command"))
                    return "500 Invalid JSON format";

                string cmd = jsonCommand["Command"].ToUpper();

                return cmd switch
                {
                    "HELO" => HandleHelo(jsonCommand),
                    "LOGIN" => HandleLogin(jsonCommand),
                    "DATA" => HandleDataCommand(jsonCommand),
                    "MAIL FROM" => HandleMailFrom(jsonCommand),
                    "RCPT TO" => HandleRcptTo(jsonCommand),
                    "FORWARD FROM" => HandleForwardFrom(jsonCommand),
                    "FORWARD TO" => HandleForwardTo(jsonCommand),
                    "MAIL FORWARD" => HandleMailForward(jsonCommand),
                    "REPLY" => HandleReply(jsonCommand),
                    "SUBJECT" => HandleSubject(jsonCommand),
                    "CONTENT" => HandleContent(jsonCommand),
                    "ATTACH" => HandleAttch(jsonCommand),
                    "QUIT" => HandleQuit(),
                    _ => "500 Command unrecognized"
                };
            }
            catch (JsonException ex)
            {
                return $"500 Error processing command: {ex.Message}";
            }
        }

        private string HandleHelo(Dictionary<string, string> jsonCommand)
        {
            if (!jsonCommand.ContainsKey("Domain"))
                return "500 HELO requires Domain";

            return $"250 Hello {jsonCommand["Domain"]}";
        }

        private string HandleLogin(Dictionary<string, string> jsonCommand)
        {
            if (_isAuthenticated)
                return "503 Already authenticated";

            if (!jsonCommand.ContainsKey("Username") || !jsonCommand.ContainsKey("Password"))
                return "500 LOGIN requires Username and Password";

            string username = jsonCommand["Username"] + _defaultDomain;
            string password = SecurityBLL.Sha256(jsonCommand["Password"]);

            var account = _accountDAL.GetAccount(username, password);

            if (account != null)
            {
                _currentUser = account;
                _isAuthenticated = true;
                return "250 LOGIN successful";
            }

            return "535 Authentication failed";
        }

        private string HandleDataCommand(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "530 Authentication required";

            return "250 Message accepted for start send mail process. continue ";
        }

        private string HandleMailFrom(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "530 Authentication required";

            if (!jsonCommand.ContainsKey("Sender") || !IsValidEmail(jsonCommand["Sender"]))
                return "500 MAIL FROM requires a valid Sender email";

            _senderEmail = jsonCommand["Sender"];
            return "250 Sender OK";
        }

        private string HandleRcptTo(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "530 Authentication required";

            if (!jsonCommand.ContainsKey("Recipient") || !IsValidEmail(jsonCommand["Recipient"]))
                return "500 RCPT TO requires a valid Recipient email";

            _receiverEmail = jsonCommand["Recipient"];
            return "250 Recipient OK";
        }

        private string HandleForwardFrom(Dictionary<string, string> jsonCommand)
        {

        }

        private string HandleForwardTo(Dictionary<string, string> jsonCommand)
        {

        }

        private string HandleMailForward(Dictionary<string, string> jsonCommand)
        {
           
        }


        private string HandleReply(Dictionary<string, string> jsonCommand)
        {
            
        }

        private string HandleSubject(Dictionary<string, string> jsonCommand)
        {
          
        }

        private string HandleContent(Dictionary<string, string> jsonCommand)
        {
         
        }

        private string HandleAttch(Dictionary<string, string> jsonCommand)
        {
          
        }

        private string HandleQuit()
        {
            return "221 Bye";
        }

        private void SaveEmail()
        {
            try
            {
                // 1. Tạo đường dẫn thư mục lưu trữ email
                string receiverDirectory = Path.Combine(_baseDir, _receiverEmail.Replace(_defaultDomain, ""));
                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string emailDirectory = Path.Combine(receiverDirectory, timestamp);
                Directory.CreateDirectory(emailDirectory);

                // 2. Tạo file nội dung email
                string contentFilePath = Path.Combine(emailDirectory, $"{timestamp}.txt");
                File.WriteAllText(contentFilePath, _emailData.ToString());

                // 3. Tạo DTO để lưu vào cơ sở dữ liệu
                var mail = new Mail
                {
                    CreatedAt = DateTime.Now,
                    Sender = _senderEmail,
                    Receiver = _receiverEmail,
                    Owner = _currentUser?.EmailAddress, // Nếu cần lưu thông tin người gửi
                    IsRead = false,
                    Attachment = null, // Bạn có thể gán tên file đính kèm (nếu có)
                    Subject =  , // Có thể lấy từ lệnh "SUBJECT"
                    Content = contentFilePath.Replace("\\", "/"), // Đường dẫn file
                    DeletedAt = null,
                    Reply = null
                };

                // 4. Lưu vào cơ sở dữ liệu
                _mailDAL.InsertMail(mail);

                Log($"Email saved successfully with content path: {contentFilePath}");
            }
            catch (Exception ex)
            {
                Log($"Error saving email: {ex.Message}");
            }
        }


        private bool IsValidEmail(string email)
        {
            //example:  vantn.21it@vku.udn.vn  with @vku.udn.vn is _defaultDomain 
            return email.Contains("@") && email.EndsWith(_defaultDomain);
        }

        private void Log(string message)
        {
            _logAction?.Invoke(message);
        }
    }
}
