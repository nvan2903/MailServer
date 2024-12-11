using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using DTO;
using DAL;

namespace BLL
{
    public class IMAPExecuteBLL
    {
        private readonly TcpClient _client;
        private readonly Action<string> _logAction;
        private readonly MailDAL _mailDAL;
        private readonly AccountDAL _accountDAL;
        private Account _currentUser;                 // người dùng hiện tại đang thực hiện request
        private bool _isAuthenticated;               // trạng thái xác thực của người dùng
        private readonly string _baseDir;             // thư mục mailBox 
        private readonly string _defaultDomain;

        public IMAPExecuteBLL(TcpClient client, Action<string> logAction, string baseDir, string defaultDomain)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
            _mailDAL = new MailDAL();
            _accountDAL = new AccountDAL();
            _currentUser = null;
            _isAuthenticated = false;
            _baseDir = baseDir ?? throw new ArgumentNullException(nameof(baseDir));
            _defaultDomain = defaultDomain ?? throw new ArgumentNullException(nameof(defaultDomain));
        }

        // phuơng thức nhận request từ client đầu vào và trả về response đầu cuối
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
                    Log("* OK IMAP Server Ready");
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        line = RemoveBOM(line); // Loại bỏ BOM trước khi xử lý
                        Log($"Client: {line}");
                        string response = ProcessCommand(line);
                        writer.WriteLine(response);
                        Log($"Server: {response}");

                        if (line.ToUpper().Contains("LOGOUT"))
                        { break; }
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

        private string RemoveBOM(string input)
        {
            return input.TrimStart('\uFEFF'); // Loại bỏ BOM từ đầu chuỗi
        }


        // phương thức xử lý các trường hợp command từ client
        private string ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return CreateJsonResponse("NO", "Syntax error, command unrecognized");

            try
            {
                if (!command.TrimStart().StartsWith("{"))
                    return CreateJsonResponse("NO", "Invalid JSON format");

                var jsonCommand = JsonConvert.DeserializeObject<Dictionary<string, string>>(command);
                if (jsonCommand == null || !jsonCommand.ContainsKey("Command"))
                    return CreateJsonResponse("NO", "Invalid JSON format: Missing 'Command' key");

                string cmd = jsonCommand["Command"].ToUpper();

                return cmd switch
                {
                    "CAPABILITY" => HandleCapability(jsonCommand),
                    "LOGIN" => HandleLogin(jsonCommand),
                    "REGISTER" => HandleRegister(jsonCommand),
                    "CHNAME" => HandleChangeName(jsonCommand),
                    "CHPASS" => HandleChangePassword(jsonCommand),
                    "SELECT" => HandleSelect(jsonCommand),
                    "FETCH" => HandleFetch(jsonCommand),
                    "DELETE" => HandleDelete(jsonCommand),
                    "RESTORE" => HandleRestore(jsonCommand),
                    "LOGOUT" => HandleLogout(jsonCommand),
                    _ => CreateJsonResponse("NO", "Command unrecognized")
                };
            }
            catch (JsonException ex)
            {
                return CreateJsonResponse("NO", $"Error processing command: {ex.Message}");
            }
            catch (Exception ex)
            {
                return CreateJsonResponse("NO", $"Internal server error: {ex.Message}");
            }
        }

        private string HandleCapability(Dictionary<string, string> jsonCommand)
        {
            return CreateJsonResponse("OK", "CAPABILITY completed");
        }

        private string HandleLogin(Dictionary<string, string> jsonCommand)
        {
            if (_isAuthenticated)
                return CreateJsonResponse("NO", "Already authenticated");

            if (!jsonCommand.ContainsKey("Username") || !jsonCommand.ContainsKey("Password"))
                return CreateJsonResponse("NO", "LOGIN requires Username and Password");

            string username = jsonCommand["Username"] + _defaultDomain;                             // thêm domain vào username "vantn.21it" + "@vku.udn.vn"
            string password = SecurityBLL.Sha256(jsonCommand["Password"]);

            var account = _accountDAL.GetAccount(username, password);

            if (account != null)
            {
                _currentUser = account;
                _isAuthenticated = true;
                return CreateJsonResponse("OK", "LOGIN completed");
            }

            return CreateJsonResponse("NO", "LOGIN failed - Invalid credentials");
        }

        private string HandleRegister(Dictionary<string, string> jsonCommand)
        {
            if (!jsonCommand.ContainsKey("Username") ||
                !jsonCommand.ContainsKey("Fullname") ||
                !jsonCommand.ContainsKey("Password"))
            {
                return CreateJsonResponse("NO", "REGISTER requires Username, Fullname, and Password");
            }

            string username = jsonCommand["Username"];
            if (username.Length < 5 || username.Length > 50)
                return CreateJsonResponse("NO", "Username length must be between 5 and 50 characters");

            string fullname = jsonCommand["Fullname"];
            string password = SecurityBLL.Sha256(jsonCommand["Password"]);
            string email = username + _defaultDomain;                                                                    // thêm domain vào username "vantn.21it" + "@vku.udn.vn"                                  

            var account = new Account
            {
                EmailAddress = email,
                Fullname = fullname,
                Password = password
            };

            Log($"Registering user: {fullname}");

            if (_accountDAL.InsertAccount(account))
            {
                string userDir = Path.GetFullPath(Path.Combine(_baseDir, username));                                       // tạo thư mục cho user trong thư mục mailBoxD:\MailBox\vantn.21it

                try
                {
                    if (!Directory.Exists(userDir))
                        Directory.CreateDirectory(userDir);

                    Log($"Directory created: {userDir}");
                    return CreateJsonResponse("OK", "REGISTER completed");
                }
                catch (Exception ex)
                {
                    Log($"Error creating directory: {ex.Message}");
                    return CreateJsonResponse("NO", "REGISTER failed - Unable to create directory");
                }
            }

            return CreateJsonResponse("NO", "REGISTER failed - Account already exists");
        }

        private string HandleChangeName(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return CreateJsonResponse("NO", "Not authenticated");

            if (!jsonCommand.ContainsKey("Newfullname"))
                return CreateJsonResponse("NO", "CHNAME requires Newfullname");

            string newName = jsonCommand["Newfullname"];

            if (_accountDAL.UpdateFullName(_currentUser.EmailAddress, newName))
                return CreateJsonResponse("OK", "CHNAME completed");

            return CreateJsonResponse("NO", "CHNAME failed");
        }

        private string HandleChangePassword(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return CreateJsonResponse("NO", "Not authenticated");

            if (!jsonCommand.ContainsKey("Oldpassword") || !jsonCommand.ContainsKey("Newpassword"))
                return CreateJsonResponse("NO", "CHPASS requires Oldpassword and Newpassword");

            string oldPassword = SecurityBLL.Sha256(jsonCommand["Oldpassword"]);
            string newPassword = SecurityBLL.Sha256(jsonCommand["Newpassword"]);

            if (_accountDAL.UpdatePassword(_currentUser.EmailAddress, oldPassword, newPassword))
                return CreateJsonResponse("OK", "CHPASS completed");

            return CreateJsonResponse("NO", "CHPASS failed");
        }

        private string HandleSelect(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return CreateJsonResponse("NO", "Not authenticated");

            if (!jsonCommand.ContainsKey("Mailbox"))
                return CreateJsonResponse("NO", "SELECT requires Mailbox");

            string mailBox = jsonCommand["Mailbox"].ToUpper();
            try
            {
                DataTable mailsTable = mailBox switch
                {
                    "INBOX" => _mailDAL.GetInboxMails(_currentUser.EmailAddress),
                    "SENT" => _mailDAL.GetSentMails(_currentUser.EmailAddress),
                    "TRASH" => _mailDAL.GetDeletedMails(_currentUser.EmailAddress),
                    "ALL" => _mailDAL.GetAllMails(_currentUser.EmailAddress),
                    _ => throw new ArgumentException("Invalid folder")
                };

                // change DataTable mailsTable to List<Mail> mails
                List<Mail> mails = new List<Mail>();
                foreach (DataRow row in mailsTable.Rows)
                {
                    Mail mail = new Mail
                    {
                        //Id = int.Parse(row["id"].ToString()),
                        //CreatedAt = DateTime.Parse(row["created_at"].ToString()),
                        Sender = row["sender"].ToString(),
                        Receiver = row["receiver"].ToString(),
                        IsRead = bool.Parse(row["is_read"].ToString()),
                        //Attachment = row["attachment"] != DBNull.Value ? row["attachment"].ToString() : null,
                        Subject = row["subject"].ToString(),
                        CreatedAt = DateTime.Parse(row["created_at"].ToString()),
                        //Content = row["content"].ToString(),
                        //Reply = row["reply"] != DBNull.Value ? int.Parse(row["reply"].ToString()) : (int?)null

                    };
                    mails.Add(mail);

                }





                //foreach (DataRow row in mailsTable.Rows)
                //{
                //    string mailId = row["id"]?.ToString() ?? "N/A";
                //    string sender = row["sender"]?.ToString() ?? "N/A";
                //    string receiver = row["receiver"]?.ToString() ?? "N/A";
                //    string subject = row["subject"]?.ToString() ?? "N/A";
                //    Log($"MailId: {mailId}, From: {sender}, To: {receiver}, Subject: {subject}");
                //}

                return CreateJsonResponse("OK", "SELECT completed", mails);
            }
            catch (Exception ex)
            {
                return CreateJsonResponse("NO", $"SELECT failed: {ex.Message}");
            }
        }

        private string HandleFetch(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return CreateJsonResponse("NO", "Not authenticated");

            if (!jsonCommand.ContainsKey("Mailid") || !int.TryParse(jsonCommand["Mailid"], out int mailId))
                return CreateJsonResponse("NO", "FETCH requires a valid Mailid");

            Log($"Fetching mail with ID: {mailId} for user: {_currentUser.EmailAddress}");

            Mail mail = _mailDAL.GetMailById(mailId, _currentUser.EmailAddress);

            if (mail != null)
            {
                Log($"Mail found: {mail.Subject}");
                string content = string.Empty;
                if (!string.IsNullOrEmpty(mail.Content) && File.Exists(mail.Content))             // kiểm tra file content và trường content của mail (trường này chứa đường đãn đên file content)
                {
                    content = File.ReadAllText(mail.Content);                                     // đọc nội dung file content
                    mail.Content = content;                                                      // gán nội dung file content vào trường content của mail
                    List<Mail> mails = new List<Mail> { mail };                                  // tạo danh sách mail để trả về
                    return CreateJsonResponse("OK", "FETCH completed", mails);                  // trả về danh sách mail
                }
            }

            Log("Mail not found or access denied.");
            return CreateJsonResponse("NO", "FETCH failed");
        }


        private string HandleDelete(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return CreateJsonResponse("NO", "Not authenticated");

            if (!jsonCommand.ContainsKey("Mailid") || !int.TryParse(jsonCommand["Mailid"], out int mailId))
                return CreateJsonResponse("NO", "DELETE requires a valid Mailid");
            if (_mailDAL.MarkMailAsDeleted(mailId, _currentUser.EmailAddress))
                return CreateJsonResponse("OK", "DELETE completed");

            return CreateJsonResponse("NO", "DELETE failed");
        }

        private string HandleRestore(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return CreateJsonResponse("NO", "Not authenticated");
            if (!jsonCommand.ContainsKey("Mailid") || !int.TryParse(jsonCommand["Mailid"], out int mailId))
                return CreateJsonResponse("NO", "RESTORE requires a valid Mailid");

            if (_mailDAL.RestoreMail(mailId, _currentUser.EmailAddress))
                return CreateJsonResponse("OK", "RESTORE completed");

            return CreateJsonResponse("NO", "RESTORE failed");
        }

        private string HandleLogout(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return CreateJsonResponse("NO", "Not authenticated");

            _currentUser = null;
            _isAuthenticated = false;
            return CreateJsonResponse("OK", "LOGOUT completed");
        }

        // phương thức để tạo json response
        private string CreateJsonResponse(string status, string messagel)
        {
            ServerResponse serverResponse = new ServerResponse(status, messagel);
            return serverResponse.ToJson();

        }

        // phương thức để tạo json response trường hợp có danh sách mail cần trả về
        private string CreateJsonResponse(string status, string message, List<Mail> Mails)
        {
            ServerResponse serverResponse = new ServerResponse(status, message, Mails);
            return serverResponse.ToJson();

        }

        // phương thức để viết log server
        private void Log(string message)
        {
            _logAction?.Invoke(message);
        }
    }
}
