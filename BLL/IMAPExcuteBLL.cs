using System;
using System.Collections.Generic;
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
        private string _currentUser;
        private bool _isAuthenticated;
        private readonly string _baseDir="";
        private readonly string _defaultDomain="";

        public IMAPExecuteBLL(TcpClient client, Action<string> logAction, string baseDir, string defaultDomain)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
            _mailDAL = new MailDAL();
            _accountDAL = new AccountDAL();
            _currentUser = string.Empty;
            _isAuthenticated = false;
            _baseDir = baseDir ?? throw new ArgumentNullException(nameof(baseDir));
            _defaultDomain = defaultDomain ?? throw new ArgumentNullException(nameof(defaultDomain));
        }

        public void Start()
        {
            try
            {
                using NetworkStream stream = _client.GetStream();
                using StreamReader reader = new(stream, Encoding.UTF8); // Đọc với UTF-8
                using StreamWriter writer = new(stream, Encoding.UTF8) { AutoFlush = true }; // Ghi với UTF-8


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
                _client.Close();
                Log("Connection closed.");
            }
        }

        private string ProcessCommand(string command)
        {
            try
            {
                // Kiểm tra nếu không phải JSON
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

            string username = jsonCommand["Username"];
            username = username + _defaultDomain;
            string password = jsonCommand["Password"];
            password = SecurityBLL.Sha256(password);       // Hash password

            var account = _accountDAL.GetAccount(username, password);

            if (account != null)
            {
                _currentUser = username;
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
            string fullname = jsonCommand["Fullname"];
            string password = jsonCommand["Password"];
            password = SecurityBLL.Sha256(password);       // Hash password
            string email = username+_defaultDomain;

            var account = new Account
            {
                EmailAddress = email,
                Fullname = fullname,
                Password = password
            };

            if (_accountDAL.InsertAccount(account))
            {
                string userDir = Path.Combine(_baseDir, username);

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

            if (!jsonCommand.ContainsKey("NewFullname"))
                return "BAD CHNAME requires NewFullname";

            string newName = jsonCommand["NewFullname"];

            if (_accountDAL.UpdateFullName(_currentUser, newName))
                return "OK CHNAME completed";

            return "NO CHNAME failed";
        }

        private string HandleChangePassword(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            if (!jsonCommand.ContainsKey("OldPassword") || !jsonCommand.ContainsKey("NewPassword"))
                return "BAD CHPASS requires OldPassword and NewPassword";

            string oldPassword = jsonCommand["OldPassword"];
            oldPassword = SecurityBLL.Sha256(oldPassword);       // Hash password
            string newPassword = jsonCommand["NewPassword"];
            newPassword = SecurityBLL.Sha256(newPassword);       // Hash password

            if (_accountDAL.UpdatePassword(_currentUser, oldPassword, newPassword))
                return "OK CHPASS completed";

            return "NO CHPASS failed";
        }

        private string HandleSelect(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            if (!jsonCommand.ContainsKey("Folder"))
                return "BAD SELECT requires Folder";

            string folder = jsonCommand["Folder"].ToLower();

            // Implement logic to fetch mails from the selected folder
            return "OK SELECT completed";
        }

        private string HandleFetch(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            if (!jsonCommand.ContainsKey("MailId") || !int.TryParse(jsonCommand["MailId"], out int mailId))
                return "BAD FETCH requires a valid MailId";

            var mail = _mailDAL.GetMailById(mailId, _currentUser);

            if (mail != null)
            {
                return $"OK FETCH completed\n{mail.Content}";
            }

            return "NO FETCH failed";
        }

        private string HandleDelete(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            if (!jsonCommand.ContainsKey("MailId") || !int.TryParse(jsonCommand["MailId"], out int mailId))
                return "BAD DELETE requires a valid MailId";

            if (_mailDAL.MarkMailAsDeleted(mailId, _currentUser))
                return "OK DELETE completed";

            return "NO DELETE failed";
        }

        private string HandleRestore(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            // Implement logic to restore deleted mail
            return "OK RESTORE completed";
        }

        private string HandleLogout(Dictionary<string, string> jsonCommand)
        {
            if (!_isAuthenticated)
                return "NO Not authenticated";

            _currentUser = string.Empty;
            _isAuthenticated = false;
            return "OK LOGOUT completed";
        }

        private void Log(string message)
        {
            _logAction?.Invoke($"{DateTime.Now:HH:mm:ss} - {message}");
        }
    }
}
