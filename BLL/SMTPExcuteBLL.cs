using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using DAL;
using DTO;
using System.Reflection;

namespace BLL
{
    public class SMTPExecuteBLL
    {
        private readonly TcpClient _client;
        private readonly Action<string> _logAction;
        private readonly MailDAL _mailDAL;
        private string _senderEmail;
        private string _receiverEmail;
        private string _emailSubject;
        private string _emailContent;
        private string _emailContentPath;
        private string _attachmentPath;
        private readonly string _baseDir;
        private readonly string _defaultDomain;

        private bool data = false;

        private bool _isForwardMail = false;
        private string _forwardFrom;
        private string _forwardTo;
        private int _forwardMailId;
        private int _replyMailId;

        public SMTPExecuteBLL(TcpClient client, Action<string> logAction, string baseDir, string defaultDomain)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
            _mailDAL = new MailDAL();
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
                    writer.WriteLine("220 SMTP Server Ready");

                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        Log($"Client: {line}");
                        string response = ProcessCommand(line);
                        writer.WriteLine(response);
                        Log($"Server: {response}");

                        if (response.StartsWith("221")) // QUIT command ends session
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
                    "MAIL FROM" => HandleMailFrom(jsonCommand),
                    "RCPT TO" => HandleRcptTo(jsonCommand),
                    "FORWARD FROM" => HandleForwardFrom(jsonCommand),
                    "FORWARD TO" => HandleForwardTo(jsonCommand),
                    "MAIL FORWARD" => HandleMailForward(jsonCommand),
                    "REPLY" => HandleReply(jsonCommand),
                    "DATA" => HandleDataCommand(jsonCommand),
                    "ATTACH" => HandleAttach(jsonCommand),
                    "QUIT" => HandleQuit(),
                    _ => "500 Command unrecognized"
                };
            }
            catch (JsonException ex)
            {
                return $"500 Error processing command: {ex.Message}";
            }
        }

        // Handle HELO Command
        private string HandleHelo(Dictionary<string, string> command)
        {
            return "250 Hello, pleased to meet you";
        }

        // Handle MAIL FROM Command
        private string HandleMailFrom(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Email"))
                return "500 Missing 'Email' in MAIL FROM command";

            _senderEmail = command["Email"];
            if (!IsValidEmail(_senderEmail))
                return "550 Invalid sender email address";

            return "250 OK";
        }

        // Handle RCPT TO Command
        private string HandleRcptTo(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Email"))
                return "500 Missing 'Email' in RCPT TO command";

            _receiverEmail = command["Email"];
            if (!IsValidEmail(_receiverEmail))
                return "550 Invalid receiver email address";

            return "250 OK";
        }

        // Handle FORWARD FROM Command
        private string HandleForwardFrom(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Email"))
                return "500 Missing 'Email' in FORWARD FROM command";

            _forwardFrom = command["Email"];
            if (!IsValidEmail(_forwardFrom))
                return "550 Invalid forward from email address";

            return "250 OK";
        }

        // Handle FORWARD TO Command
        private string HandleForwardTo(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Email"))
                return "500 Missing 'Email' in FORWARD TO command";

            _forwardTo = command["Email"];
            if (!IsValidEmail(_forwardTo))
                return "550 Invalid forward to email address";

            return "250 OK";
        }

        // Handle MAIL FORWARD Command
        private string HandleMailForward(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Mailid"))
                return "500 Missing 'Mailid' in MAIL FORWARD command";

            _forwardMailId = int.Parse(command["Mailid"]);
            _isForwardMail = true;

            return "250 OK";
        }

        // Handle REPLY Command
        private string HandleReply(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Mailid"))
                return "500 Missing 'Email' in REPLY command";

            _replyMailId = int.Parse(command["Mailid"]);

            return "250 OK";
        }

        // Handle DATA Command
        private string HandleDataCommand(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Subject") || !command.ContainsKey("Content"))
                return "500 Missing 'Subject' or 'Content' in DATA command";

            _emailSubject = command["Subject"];
            _emailContent = command["Content"];

            data = true;

            return "250 OK";
        }

        // Handle ATTACH Command
        private string HandleAttach(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("FilePath"))
                return "500 Missing 'FilePath' in ATTACH command";

              _attachmentPath = command["FilePath"];
           
                return "250 File attached successfully";
        }

        // Handle QUIT Command
        private string HandleQuit()
        {

            if (_isForwardMail)
            {

                SaveForwardMail(_forwardMailId, _forwardFrom, _forwardTo);
                Log("Mail forwarded successfully");
            }


            if (data)
            {
                SaveContentMail(_senderEmail, _emailContent);
                SaveEmailToDatabase(_senderEmail, _receiverEmail, _senderEmail, _attachmentPath, _emailSubject, _emailContentPath,_replyMailId);

                SaveContentMail(_receiverEmail, _emailContent);
                SaveEmailToDatabase(_senderEmail, _receiverEmail, _receiverEmail, _attachmentPath, _emailSubject, _emailContentPath, _replyMailId);
            }


            return "221 Goodbye";
        }

        // Save email to the database
        private void SaveEmailToDatabase(string senderEmail, string receiverEmail, string owner, string attachmentPath,string emailSubject, string emailContentPath, int reply)
        {
            try
            {
                    Mail mail = new()
                    {
                        CreatedAt = DateTime.Now,
                        Sender = senderEmail,
                        Receiver = receiverEmail,
                        Owner = owner,
                        IsRead = false,
                        Attachment = attachmentPath,
                        Subject = emailSubject,
                        Content = emailContentPath,
                        DeletedAt = null,
                        Reply = reply
                    };
                    _mailDAL.InsertMail(mail);
                    Log($"Email saved to database successfully with subject: {_emailSubject}");
                

            }
            catch (Exception ex)
            {
                Log($"Error saving email to database: {ex.Message}");
            }
        }

        private void SaveForwardMail(int mailId, string forwardFrom, string forwardTo)
        {
            try
            {
                // Retrieve the original mail from the database
                Mail originalMail = _mailDAL.GetMailById(mailId, forwardFrom);
                if (originalMail == null)
                {
                    Log($"Mail with ID {mailId} not found for forwarding.");
                    return;
                }

                // Load original email content from its file path
                string originalContent = string.Empty;
                if (!string.IsNullOrEmpty(originalMail.Content) && File.Exists(originalMail.Content))
                {
                    originalContent = File.ReadAllText(originalMail.Content);

                }
                else
                {
                    Log($"Original mail content file not found: {originalMail.Content}");
                }

                // Create the forwarded message content
                string forwardedMessage =
                    "<--------- Forwarded message --------->\n" +
                    $"<---------- From: {originalMail.Sender} ---------->\n" +
                    $"<---------- To: {originalMail.Receiver} ---------->\n" +
                    $"<---------- Date: {originalMail.CreatedAt} ---------->\n\n";

                string forwardContent = forwardedMessage + originalContent;
                Log("Forwarded message content: " + forwardContent);


                // Copy attachment if exists
                string forwardAttachmentPath = "Attachment.txt";
                //if (!string.IsNullOrEmpty(originalMail.Attachment) && File.Exists(originalMail.Attachment))
                //{
                //    string newAttachmentPath = Path.Combine(_baseDir, forwardTo.Replace(_defaultDomain, ""), "attachments");
                //    if (!Directory.Exists(newAttachmentPath))
                //        Directory.CreateDirectory(newAttachmentPath);

                //    string newAttachmentFile = Path.Combine(newAttachmentPath, Path.GetFileName(originalMail.Attachment));
                //    File.Copy(originalMail.Attachment, newAttachmentFile, true);
                //    forwardedAttachment = newAttachmentFile;
                //}

                // Save forwarded content and email for 'forwardFrom'
                SaveContentMail(forwardFrom, forwardContent);
                SaveEmailToDatabase(forwardFrom, forwardTo, forwardFrom, forwardAttachmentPath, originalMail.Subject, _emailContentPath, _replyMailId);

                // Save forwarded content and email for 'forwardTo'
                SaveContentMail(forwardTo, forwardContent);
                SaveEmailToDatabase(forwardFrom, forwardTo, forwardTo, forwardAttachmentPath, originalMail.Subject, _emailContentPath, _replyMailId);

                Log($"Forwarded mail saved successfully for mail ID: {mailId}");
            }
            catch (Exception ex)
            {
                Log($"Error saving forward mail: {ex.Message}");
            }
        }

        


        //Save content mail 
        private void SaveContentMail(string email, string content)
        {
            try
            {

                string accountDir = _baseDir + email.Replace(_defaultDomain, "") + "/";
                string fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";


                if (!Directory.Exists(accountDir))
                    Directory.CreateDirectory(accountDir);

                File.WriteAllText(accountDir + fileName, content);
                _emailContentPath = accountDir + fileName;
            }
            catch (Exception ex)
            {
                Log($"Error saving content mail: {ex.Message}");
            }
        }



        // Validate email format
        private bool IsValidEmail(string email)
        {
            return email.Contains("@") && email.EndsWith(_defaultDomain);
        }

        // Log helper
        private void Log(string message)
        {
            _logAction?.Invoke(message);
        }
    }
}
