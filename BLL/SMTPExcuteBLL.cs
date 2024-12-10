using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using DAL;
using DTO;
using System.Reflection;
using System.Xml.Linq;

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
                    var welcomeMessage = CreateJsonResponse("220", "Welcome to SMTP Server");
                    writer.WriteLine(welcomeMessage);


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
                return CreateJsonResponse("BAD", "Syntax error, command unrecognized");

            try
            {
                if (!command.TrimStart().StartsWith("{"))
                    return CreateJsonResponse("BAD", "Invalid JSON format");

                var jsonCommand = JsonConvert.DeserializeObject<Dictionary<string, string>>(command);
                if (jsonCommand == null || !jsonCommand.ContainsKey("Command"))
                    return CreateJsonResponse("BAD", "Invalid JSON format: Missing 'Command' key");

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
                    _ => CreateJsonResponse("BAD", "Command unrecognized")
                };
            }
            catch (JsonException ex)
            {
                return CreateJsonResponse("BAD", $"Error processing command: {ex.Message}");
            }
        }

        // Handle HELO Command
        private string HandleHelo(Dictionary<string, string> command)
        {
            return CreateJsonResponse("OK", "Hello, this is SMTP Server");
        }

        // Handle MAIL FROM Command
        private string HandleMailFrom(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Email"))
                return CreateJsonResponse("BAD", "Missing 'Email' in MAIL FROM command");

            _senderEmail = command["Email"];
            if (!IsValidEmail(_senderEmail))
                return CreateJsonResponse("BAD", "Invalid sender email address");

            return CreateJsonResponse("OK", "Sender email address accepted");
        }

        // Handle RCPT TO Command
        private string HandleRcptTo(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Email"))
                return CreateJsonResponse("BAD", "Missing 'Email' in RCPT TO command");

            _receiverEmail = command["Email"];
            if (!IsValidEmail(_receiverEmail))
                return CreateJsonResponse("BAD", "Invalid receiver email address");

            return CreateJsonResponse("OK", "Receiver email address accepted");
        }

        // Handle FORWARD FROM Command
        private string HandleForwardFrom(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Email"))
                return CreateJsonResponse("BAD", "Missing 'Email' in FORWARD FROM command");

            _forwardFrom = command["Email"];
            if (!IsValidEmail(_forwardFrom))
                return CreateJsonResponse("BAD", "Invalid forward from email address");

            return CreateJsonResponse("OK", "Forward from email address accepted");
        }

        // Handle FORWARD TO Command
        private string HandleForwardTo(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Email"))
                return CreateJsonResponse("BAD", "Missing 'Email' in FORWARD TO command");

            _forwardTo = command["Email"];
            if (!IsValidEmail(_forwardTo))
                return CreateJsonResponse("BAD", "Invalid forward to email address");

            return CreateJsonResponse("OK", "Forward to email address accepted");
        }

        // Handle MAIL FORWARD Command
        private string HandleMailForward(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Mailid"))
                return CreateJsonResponse("BAD", "Missing 'Mailid' in MAIL FORWARD command");

            _forwardMailId = int.Parse(command["Mailid"]);
            _isForwardMail = true;

            string identify = DateTime.Now.ToString("yyyyMMdd_HHmmss");  // Unique identifier for each email
            try
            {
                SaveForwardMail(_forwardMailId, _forwardFrom, _forwardTo, identify);
                Log($"{identify}: Mail forwarded successfully");
                return CreateJsonResponse("OK", "Forward mail successfully", identify);
            }
            catch (Exception ex)
            {
                Log($"Error in HandleMailForward: {ex.Message}");
                return CreateJsonResponse("BAD", $"Error forwarding mail: {ex.Message}");
            }
        }

        // Handle REPLY Command
        private string HandleReply(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Mailid"))
                return CreateJsonResponse("BAD", "Missing 'Mailid' in REPLY command");

            _replyMailId = int.Parse(command["Mailid"]);

            return CreateJsonResponse("OK", "Reply mail initialized");
        }

        // Handle DATA Command
        private string HandleDataCommand(Dictionary<string, string> command)
        {
            try
            {
                if (!command.ContainsKey("Subject") || !command.ContainsKey("Content"))
                {
                    return CreateJsonResponse("BAD", "Missing 'Subject' or 'Content' in DATA command");
                }

                _emailSubject = command["Subject"];
                _emailContent = command["Content"];
                data = true;

                Log($"Data command processed successfully. Subject: {_emailSubject}, Content Length: {_emailContent.Length}");

                string identify = DateTime.Now.ToString("yyyyMMdd_HHmmss");  // Unique identifier for each email

             
                    if (_replyMailId == 0) // If reply mail id is 0, then it is a normal mail
                    {
                        // save to sender mail box and database
                        SaveContentMail(_senderEmail, _emailContent, identify);
                        SaveEmailToDatabase(_senderEmail, _receiverEmail, _senderEmail, _attachmentPath, _emailSubject, _emailContentPath);
                        //save to receiver mail box and database
                        SaveContentMail(_receiverEmail, _emailContent, identify);
                        SaveEmailToDatabase(_senderEmail, _receiverEmail, _receiverEmail, _attachmentPath, _emailSubject, _emailContentPath);
                        return CreateJsonResponse("OK", "Send mail successfully", identify);
                    }
                    else // If reply mail id is not 0, then it is a reply mail
                    {

                        SaveContentMail(_senderEmail, _emailContent, identify);
                        SaveReplyEmailToDatabase(_senderEmail, _receiverEmail, _senderEmail, _attachmentPath, _emailSubject, _emailContentPath, _replyMailId);



                        SaveContentMail(_receiverEmail, _emailContent, identify);
                        // no need to set the 'Reply' field because this is a received message, not a reply
                        SaveEmailToDatabase(_senderEmail, _receiverEmail, _receiverEmail, _attachmentPath, _emailSubject, _emailContentPath);
                        return CreateJsonResponse("OK", "Reply mail successfully", identify);
                    }
                

            }
            catch (Exception ex)
            {
                Log($"Error in HandleDataCommand: {ex.Message}");
                return CreateJsonResponse("BAD", $"Error processing DATA command: {ex.Message}");
            }
        }


        // Handle ATTACH Command
        private string HandleAttach(Dictionary<string, string> command)
        {
            if (!command.ContainsKey("Filename"))
                return CreateJsonResponse("BAD", "Missing 'Filename' in ATTACH command");

            _attachmentPath = command["Filename"];
           
                return CreateJsonResponse("OK", "Attachment accepted");
        }

        // Handle QUIT Command
        private string HandleQuit()
        {

            return CreateJsonResponse("221", "Bye");
        }

        private void SaveEmailToDatabase(string senderEmail, string receiverEmail, string owner, string attachmentPath, string emailSubject, string emailContentPath)
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
                        Reply = null                  // So reply is null
                    };
                    bool isInserted = _mailDAL.InsertMail(mail);

                if (isInserted)
                {
                    Log($"Email saved to database successfully with subject: {mail.Subject}");
                }
                else
                {
                    Log("Failed to save email to database.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error saving email to database: {ex.Message}");
            }
        }



        private void SaveReplyEmailToDatabase(string senderEmail, string receiverEmail, string owner, string attachmentPath, string emailSubject, string emailContentPath, int reply)
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
                        Reply = reply                   // Set the reply mail id here
                    };

                bool isInserted = _mailDAL.InsertReplyMail(mail);

                if (isInserted)
                {
                    Log($"Reply Email saved to database successfully with subject: {mail.Subject}");
                }
                else
                {
                    Log("Failed to save email to database.");
                }
            }
            catch (Exception ex)
            {
                Log($"Error saving email to database: {ex.Message}");
            }
        }






        private void SaveForwardMail(int mailId, string forwardFrom, string forwardTo, string identify)
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
                SaveContentMail(forwardFrom, forwardContent, identify);
                SaveEmailToDatabase(forwardFrom, forwardTo, forwardFrom, forwardAttachmentPath, originalMail.Subject, _emailContentPath);

                // Save forwarded content and email for 'forwardTo'
                SaveContentMail(forwardTo, forwardContent, identify);
                SaveEmailToDatabase(forwardFrom, forwardTo, forwardTo, forwardAttachmentPath, originalMail.Subject, _emailContentPath);

                Log($"{identify}: Forwarded mail saved successfully for mail ID: {mailId}");
            }
            catch (Exception ex)
            {
                Log($"Error saving forward mail: {ex.Message}");
            }
        }

        


        //Save content mail 
        private void SaveContentMail(string email, string content, string identify)
        {
            try
            {
               
                string accountDir = _baseDir + email.Replace(_defaultDomain, "") + "/" + identify + "/";   // such as D:\MailBox\trungnd.21it\20241202_230409

                // string file name = parent folder name
                string fileName = identify + ".txt";

                if (!Directory.Exists(accountDir))
                    Directory.CreateDirectory(accountDir);

                File.WriteAllText(accountDir + fileName, content);
                _emailContentPath = accountDir + fileName;
                Log($"{identify}: Content mail saved successfully");
            }
            catch (Exception ex)
            {
                Log($"Error saving content mail: {ex.Message}");
            }
        }

        private string CreateJsonResponse(string status, string messagel)
        {
            ServerResponse serverResponse = new ServerResponse(status, messagel);
            return serverResponse.ToJson();

        }

        private string CreateJsonResponse(string status, string message, string identify)
        {
            ServerResponse serverResponse = new ServerResponse(status, message, identify);
            return serverResponse.ToJson();

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
