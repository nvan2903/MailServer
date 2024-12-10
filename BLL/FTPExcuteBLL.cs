using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;
using System.Collections.Generic;
using DTO;

namespace BLL
{
    public class FTPExcuteBLL
    {
        private readonly TcpClient _client;
        private readonly NetworkStream _stream;
        private readonly Action<string> _logAction;
        private readonly string _baseDir;
        private readonly string _defaultDomain;

        private string _currentUserEmail;
        private string _senderEmail;
        private string _recipientEmail;

        public FTPExcuteBLL(TcpClient client, Action<string> logAction, string baseDir, string defaultDomain)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _stream = _client.GetStream();
            _logAction = logAction ?? throw new ArgumentNullException(nameof(logAction));
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
                        string response = ProcessCommand(request);
                        writer.WriteLine(response);
                        if (request.ToUpper().Contains("QUIT"))
                            break;
                    }
                }
                catch (Exception ex)
                {
                   Log($"Error in FTP session: {ex.Message}");
                }
            }
        }

        private string ProcessCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                return CreateJsonResponse("NO", "Invalid command format: Must be JSON");

            try
            {
                if (!command.TrimStart().StartsWith("{"))
                    return CreateJsonResponse("NO", "Invalid command format: Must be JSON");

                var jsonCommand = JsonConvert.DeserializeObject<Dictionary<string, string>>(command);

                if (jsonCommand == null || !jsonCommand.ContainsKey("Command"))
                    return CreateJsonResponse("NO", "Invalid command format: Missing 'Command' field");

                string cmd = jsonCommand["Command"].ToUpper();

                return cmd switch
                {
                    "FTP" => HandleFtp(jsonCommand),
                    "PUT" => HandlePut(jsonCommand),
                    "RECV" => HandleRecv(jsonCommand),
                    "FORWARD" => HandleForward(jsonCommand),
                    "QUIT" => HandleQuit(jsonCommand),
                    _ => CreateJsonResponse("NO", $"Invalid command: {cmd}"),
                };
            }
            catch (JsonException ex)
            {
                return CreateJsonResponse("NO", $"Invalid JSON format: {ex.Message}");
            }
            catch (Exception ex)
            {
                return CreateJsonResponse("NO", $"Error processing command: {ex.Message}");
            }
        }

        private string HandleFtp(Dictionary<string, string> jsonCommand)
        {
            if (!jsonCommand.ContainsKey("Sender") || !jsonCommand.ContainsKey("Recipient"))
                return CreateJsonResponse("NO", "Missing 'Sender' or 'Recipient' field for FTP command");

            _senderEmail = jsonCommand["Sender"];
            _recipientEmail = jsonCommand["Recipient"];

           Log($"FTP session started. Sender: {_senderEmail}, Recipient: {_recipientEmail}");
            return CreateJsonResponse("OK", "FTP session initialized with user information");
        }

        private string HandlePut(Dictionary<string, string> jsonCommand)
        {
            if (!jsonCommand.ContainsKey("Filename"))
                return CreateJsonResponse("NO", "Missing 'Filename' field for PUT command");

            if (!jsonCommand.ContainsKey("Identify"))
                return CreateJsonResponse("NO", "Missing 'Identify' field for PUT command");

            string fileName = jsonCommand["Filename"];
            string identify = jsonCommand["Identify"];
            Log("Uploading file: " + fileName);

            try
            {
                // Construct file paths
                string senderFilePath = Path.Combine(
                    _baseDir,
                    _senderEmail.Replace(_defaultDomain, ""),
                    identify,
                    "Attachments",
                    fileName);

                string receiverFilePath = Path.Combine(
                    _baseDir,
                    _recipientEmail.Replace(_defaultDomain, ""),
                    identify,
                    "Attachments",
                    fileName);

                // Create directories for sender and receiver
                string senderDir = Path.GetDirectoryName(senderFilePath);
                string receiverDir = Path.GetDirectoryName(receiverFilePath);

                if (string.IsNullOrEmpty(senderDir) || string.IsNullOrEmpty(receiverDir))
                    return CreateJsonResponse("NO", "Error constructing file paths");

                Directory.CreateDirectory(senderDir);
                Directory.CreateDirectory(receiverDir);

                // Increase timeout for receiving file from client
                _client.ReceiveTimeout = 30000; // Increased timeout to 30 seconds

                // Use buffered stream for efficient file write
                using (var fileStream = new FileStream(senderFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 64 * 1024))
                {
                    byte[] buffer = new byte[64 * 1024]; // Larger buffer size for better performance
                    int bytesRead;

                    while (true)
                    {
                        if (_stream == null || !_stream.CanRead)
                            throw new ObjectDisposedException(nameof(NetworkStream), "NetworkStream is unavailable");

                        try
                        {
                            while (_stream.DataAvailable && (bytesRead = _stream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                fileStream.Write(buffer, 0, bytesRead);
                            }
                            break;

                        }
                        catch (IOException ex)
                        {
                            Log($"IOException during file read: {ex.Message}");
                            Thread.Sleep(100); // Sleep for a short period before retrying
                        }
                        //Log to check if the file is being uploaded
                        Log($"File uploaded: {fileName}");
                    }
                }

                // Optionally copy the file to the receiver's directory
                File.Copy(senderFilePath, receiverFilePath, overwrite: true);
                Log("File uploaded successfully}");
                // Return success response
                return CreateJsonResponse("OK", "File uploaded successfully");
            }
            catch (ObjectDisposedException ex)
            {
                Log($"Stream disposed error: {ex.Message}");
                return CreateJsonResponse("NO", "Connection closed unexpectedly");
            }
            catch (IOException ex)
            {
                Log($"IO Error uploading file: {ex.Message}");
                return CreateJsonResponse("NO", $"IO Error uploading file: {ex.Message}");
            }
            catch (SocketException ex)
            {
                Log($"Socket Error: {ex.Message}");
                return CreateJsonResponse("NO", $"Socket Error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Log($"Error uploading file: {ex.Message}");
                return CreateJsonResponse("NO", $"Error uploading file: {ex.Message}");
            }
        }




        private string HandleRecv(Dictionary<string, string> jsonCommand)
        {
            if (!jsonCommand.ContainsKey("FileName"))
                return CreateJsonResponse("NO", "Missing 'FileName' field for PUT command");

            return CreateJsonResponse("OK", "FTP session initialized with user information");
        }

        private string HandleForward(Dictionary<string, string> jsonCommand)
        {
            if (!jsonCommand.ContainsKey("FileName"))
                return CreateJsonResponse("NO", "Missing 'FileName' field for PUT command");

            return CreateJsonResponse("OK", "FTP session initialized with user information");
        }

        private string HandleQuit(Dictionary<string, string> jsonCommand)
        {

         return CreateJsonResponse("OK", "FTP session ended");
        }
        private string CreateJsonResponse(string status, string messagel)
        {
            ServerResponse serverResponse = new ServerResponse(status, messagel);
            return serverResponse.ToJson();

        }

        private void Log(string message)
        {
            _logAction?.Invoke(message);
        }
    }
}
