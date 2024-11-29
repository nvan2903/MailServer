using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using DTO;
using DAL;

namespace BLL
{
    public class FTPExcuteBLL
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly Action<string> _logAction; // Action để ghi log
        private readonly MailDAL _mailDAL; // DAL để lưu vào database
        private readonly string _baseDir; // Thư mục lưu tài khoản email
        private readonly string _defaultDomain; // Domain mặc định là @vku.udn.vn

        public FTPExcuteBLL(TcpClient client, Action<string> logAction, string baseDir, string defaultDomain)
        {
            
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = _client.GetStream();
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
            _mailDAL = new MailDAL();
            _baseDir = baseDir ?? throw new ArgumentNullException(nameof(baseDir));
            _defaultDomain = defaultDomain ?? throw new ArgumentNullException(nameof(defaultDomain));
        }

        public void Start()
        {
            using (var reader = new StreamReader(_stream, Encoding.UTF8))
            using (var writer = new StreamWriter(_stream, Encoding.UTF8) { AutoFlush = true })
            {
                try
                {
                    string request;
                    while ((request = reader.ReadLine()) != null)
                    {
                        var parts = request.Split(' ', 2);
                        var command = parts[0].ToUpper();
                        var argument = parts.Length > 1 ? parts[1] : null;

                        switch (command)
                        {
                            case "PUT":
                                HandlePutCommand(argument, reader, writer);
                                break;

                            case "RECV":
                                HandleRecvCommand(argument, writer);
                                break;

                            case "FORWARD":
                                HandleForwardCommand(argument, writer);
                                break;

                            case "QUIT":
                                writer.WriteLine("OK");
                                return;

                            default:
                                writer.WriteLine("INVALID COMMAND");
                                break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error in FTP session: {ex.Message}");
                }
            }
        }

        private void HandlePutCommand(string fileName, StreamReader reader, StreamWriter writer)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    writer.WriteLine("ERROR: Missing file name");
                    return;
                }

                string filePath = Path.Combine("Attachments", fileName);
                using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    char[] buffer = new char[1024];
                    int bytesRead;
                    while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        fileStream.Write(Encoding.UTF8.GetBytes(buffer), 0, bytesRead);
                    }
                }

                writer.WriteLine("OK: File uploaded successfully");
            }
            catch (Exception ex)
            {
                writer.WriteLine($"ERROR: {ex.Message}");
            }
        }

        private void HandleRecvCommand(string fileName, StreamWriter writer)
        {
            try
            {
                if (string.IsNullOrEmpty(fileName))
                {
                    writer.WriteLine("ERROR: Missing file name");
                    return;
                }

                string filePath = Path.Combine("Attachments", fileName);
                if (!File.Exists(filePath))
                {
                    writer.WriteLine("ERROR: File not found");
                    return;
                }

                using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    byte[] buffer = new byte[1024];
                    int bytesRead;
                    while ((bytesRead = fileStream.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        _stream.Write(buffer, 0, bytesRead);
                    }
                }

                writer.WriteLine("OK: File sent successfully");
            }
            catch (Exception ex)
            {
                writer.WriteLine($"ERROR: {ex.Message}");
            }
        }

        private void HandleForwardCommand(string mailId, StreamWriter writer)
        {
            try
            {
                if (string.IsNullOrEmpty(mailId))
                {
                    writer.WriteLine("ERROR: Missing mail ID");
                    return;
                }

                var mail = _mailDAL.GetMailById(int.Parse(mailId), "owner"); // Replace "owner" with actual owner
                if (mail != null)
                {
                    if (!string.IsNullOrEmpty(mail.Attachment))
                    {
                        string attachmentPath = Path.Combine("Attachments", mail.Attachment);
                        if (File.Exists(attachmentPath))
                        {
                            writer.WriteLine("OK: Forwarding mail with attachment");
                            HandleRecvCommand(mail.Attachment, writer);
                        }
                        else
                        {
                            writer.WriteLine("ERROR: Attachment not found");
                        }
                    }
                    else
                    {
                        writer.WriteLine("OK: Forwarding mail without attachment");
                    }
                }
                else
                {
                    writer.WriteLine("ERROR: Mail not found");
                }
            }
            catch (Exception ex)
            {
                writer.WriteLine($"ERROR: {ex.Message}");
            }
        }
    }
}