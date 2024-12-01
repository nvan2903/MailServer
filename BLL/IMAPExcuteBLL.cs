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
        private Account _currentUser;
        private bool _isAuthenticated;
        private readonly string _baseDir;
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
                    writer.WriteLine("* OK IMAP Server Ready");
                    Log("* OK IMAP Server Ready");

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Log($"Client: {line}");
                        string response = ProcessCommand(line);
                        writer.WriteLine(response);
                        Log($"Server: {response}");

                        if (line.ToUpper().Contains("LOGOUT"))
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
                return "BAD Empty command received";

            try
            {
                if (!command.TrimStart().StartsWith("{"))
                    return "BAD Invalid command format: Must be JSON";

                var jsonCommand = JsonConvert.DeserializeObject<Dictionary<string, string>>(command);
                if (jsonCommand == null || !jsonCommand.ContainsKey("Command"))
                    return "BAD Invalid JSON format";

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
                    _ => "BAD Command unrecognized"
                };
            }
            catch (JsonException ex)
            {
                return $"BAD Error processing command: {ex.Message}";
            }
            catch (Exception ex)
            {
                return $"BAD Internal server error: {ex.Message}";
            }
        }

        private string HandleCapability(Dictionary<string, string> jsonCommand)
        {
            return "OK CAPABILITY completed";
        }

        private string HandleLogin(Dictionary<string, string> jsonCommand)
        {
            if (_isAuthenticated)
                return "NO LOGIN failed - Already authenticated";

            if (!jsonCommand.ContainsKey("Username") || !jsonCommand.ContainsKey("Password"))
                return "BAD LOGIN requires Username and Password";

            string username = jsonCommand["Username"] + _defaultDomain;
            string password = SecurityBLL.Sha256(jsonCommand["Password"]);

            var account = _accountDAL.GetAccount(username, password);

            if (account != null)
            {
                _currentUser = account;
                _isAuthenticated = true;
                return "OK LOGIN completed";
            }

            return "NO LOGIN failed - Invalid credentials";
        }

        private string HandleRegister(Dictionary<string, string> jsonCommand)
        {
            if (!jsonCommand.ContainsKey("Username") ||
                !jsonCommand.ContainsKey("Fullname") ||
                !jsonCommand.ContainsKey("Password"))
            {
                return "BAD REGISTER requires Username, Fullname, and Password";
            }

            string username = jsonCommand["Username"];
            if (username.Length < 5 || username.Length > 50)
                return "BAD Username length must be between 5 and 50 characters";

            string fullname = jsonCommand["Fullname"];
            string password = SecurityBLL.Sha256(jsonCommand["Password"]);
            string email = username + _defaultDomain;

            var account = new Account
            {
                EmailAddress = email,
                Fullname = fullname,
                Password = password
            };

            Log($"Registering user: {fullname}");

            if (_accountDAL.InsertAccount(account))
            {
                string userDir = Path.GetFullPath(Path.Combine(_baseDir, username));

                try
                {
                    if (!Directory.Exists(userDir))
                        Directory.CreateDirectory(userDir);

                    Log($"Directory created: {userDir}");
                    return "OK REGISTER completed";
                }
                catch (Exception ex)
                {
                    Log($"Error creating directory: {ex.Message}");
                    return "NO REGISTER failed - Unable to create directory";
                }
            }

            return "NO REGISTER failed - Account already exists";
        }

        private string HandleChangeName(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            if (!jsonCommand.ContainsKey("Newfullname"))
                return "BAD CHNAME requires Newfullname";

            string newName = jsonCommand["Newfullname"];

            if (_accountDAL.UpdateFullName(_currentUser.EmailAddress, newName))
                return "OK CHNAME completed";

            return "NO CHNAME failed";
        }

        private string HandleChangePassword(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            if (!jsonCommand.ContainsKey("Oldpassword") || !jsonCommand.ContainsKey("Newpassword"))
                return "BAD CHPASS requires Oldpassword and Newpassword";

            string oldPassword = SecurityBLL.Sha256(jsonCommand["Oldpassword"]);
            string newPassword = SecurityBLL.Sha256(jsonCommand["Newpassword"]);

            if (_accountDAL.UpdatePassword(_currentUser.EmailAddress, oldPassword, newPassword))
                return "OK CHPASS completed";

            return "NO CHPASS failed";
        }

        private string HandleSelect(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            if (!jsonCommand.ContainsKey("Mailbox"))
                return "BAD SELECT requires Mailbox";

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

                foreach (DataRow row in mailsTable.Rows)
                {
                    string mailId = row["id"]?.ToString() ?? "N/A";
                    string sender = row["sender"]?.ToString() ?? "N/A";
                    string receiver = row["receiver"]?.ToString() ?? "N/A";
                    string subject = row["subject"]?.ToString() ?? "N/A";
                    Log($"MailId: {mailId}, From: {sender}, To: {receiver}, Subject: {subject}");
                }

                return "OK SELECT completed";
            }
            catch (Exception ex)
            {
                return $"NO SELECT failed - {ex.Message}";
            }
        }

        private string HandleFetch(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            if (!jsonCommand.ContainsKey("Mailid") || !int.TryParse(jsonCommand["Mailid"], out int mailId))
                return "BAD FETCH requires a valid Mailid";

            Log($"Fetching mail with ID: {mailId} for user: {_currentUser.EmailAddress}");

            var mail = _mailDAL.GetMailById(mailId, _currentUser.EmailAddress);

            if (mail != null)
            {
                Log($"Mail found: {mail.Subject}");
                string content = string.Empty;
                if (!string.IsNullOrEmpty(mail.Content) && File.Exists(mail.Content))
                {
                    content = File.ReadAllText(mail.Content);
                }

                // Tạo đối tượng JSON
                var emailData = new
                {
                    mail.Id,
                    mail.Sender,
                    mail.Receiver,
                    mail.Subject,
                    mail.CreatedAt,
                    mail.IsRead,
                    mail.Attachment,
                    Content = content
                };
                // Chuyển thành JSON
                string jsonResponse = JsonConvert.SerializeObject(emailData, Formatting.Indented);
                // Gửi trả JSON
                return $"200 OK {jsonResponse}";

            }

            Log("Mail not found or access denied.");
            return "NO FETCH failed";
        }


        private string HandleDelete(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            if (!jsonCommand.ContainsKey("Mailid") || !int.TryParse(jsonCommand["Mailid"], out int mailId))
                return "BAD DELETE requires a valid Mailid";

            if (_mailDAL.MarkMailAsDeleted(mailId, _currentUser.EmailAddress))
                return "OK DELETE completed";

            return "NO DELETE failed";
        }

        private string HandleRestore(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";
            if (!jsonCommand.ContainsKey("Mailid") || !int.TryParse(jsonCommand["Mailid"], out int mailId))
                return "BAD RESTORE requires a valid Mailid";

            if (_mailDAL.RestoreMail(mailId, _currentUser.EmailAddress))
                return "OK RESTORE completed";

            return "NO RESTORE failed";
        }

        private string HandleLogout(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            _currentUser = null;
            _isAuthenticated = false;
            return "OK LOGOUT completed";
        }

        private void Log(string message)
        {
            _logAction?.Invoke(message);
        }
    }
}
