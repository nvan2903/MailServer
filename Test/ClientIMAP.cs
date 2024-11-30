using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Test
{
    class ClientIMAP
    {
        static void Main(string[] args)
        {
            string server = "localhost"; // Đổi thành địa chỉ server của bạn
            int port = 143; // Đổi thành cổng server của bạn

            try
            {
                using TcpClient client = new TcpClient(server, port);
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

                // Đọc lời chào từ server
                string response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");

                // Gửi lệnh CAPABILITY
                var capabilityCommand = new { Command = "CAPABILITY" };
                writer.WriteLine(JsonConvert.SerializeObject(capabilityCommand));
                response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");

                // Gửi lệnh REGISTER
                var registerCommand = new
                {
                    Command = "REGISTER",
                    Username = "vantn.21it",
                    Fullname = "Tào Nguyên Văn",
                    Password = "12345678"
                };
                writer.WriteLine(JsonConvert.SerializeObject(registerCommand));
                response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");

                // Gửi lệnh LOGIN
                var loginCommand = new
                {
                    Command = "LOGIN",
                    Username = "vantn.21it",
                    Password = "12345678"
                };
                writer.WriteLine(JsonConvert.SerializeObject(loginCommand));
                response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");

                // Gửi lệnh CHNAME để đổi tên
                var chnameCommand = new
                {
                    Command = "CHNAME",
                    Newfullname = "Văn Tào Nguyên"
                };
                writer.WriteLine(JsonConvert.SerializeObject(chnameCommand));
                response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");

                // Gửi lệnh CHPASS để đổi mật khẩu
                var chpassCommand = new
                {
                    Command = "CHPASS",
                    Oldpassword = "12345678",
                    Newpassword = "87654321"
                };
                writer.WriteLine(JsonConvert.SerializeObject(chpassCommand));
                response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");

                // Gửi lệnh SELECT để lấy danh sách email trong INBOX
                var selectCommand = new
                {
                    Command = "SELECT",
                    Mailbox = "INBOX"
                };
                writer.WriteLine(JsonConvert.SerializeObject(selectCommand));
                response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");


                //// Send SELECT INBOX command
                //var selectInboxCommand = new
                //{
                //    Command = "SELECT",
                //    Mailbox = "INBOX"
                //};
                //string jsonSelectInboxCommand = JsonConvert.SerializeObject(selectInboxCommand);
                //writer.WriteLine(jsonSelectInboxCommand);
                //response = reader.ReadLine();
                //Console.WriteLine($"Server: {response}");

                //// Send SELECT SENT command
                //var selectSentCommand = new
                //{
                //    Command = "SELECT",
                //    Mailbox = "SENT"
                //};
                //string jsonSelectSentCommand = JsonConvert.SerializeObject(selectSentCommand);
                //writer.WriteLine(jsonSelectSentCommand);
                //response = reader.ReadLine();
                //Console.WriteLine($"Server: {response}");

                //// Send SELECT TRASH command
                //var selectTrashCommand = new
                //{
                //    Command = "SELECT",
                //    Mailbox = "TRASH"
                //};
                //string jsonSelectTrashCommand = JsonConvert.SerializeObject(selectTrashCommand);
                //writer.WriteLine(jsonSelectTrashCommand);
                //response = reader.ReadLine();
                //Console.WriteLine($"Server: {response}");

                //// Send SELECT ALL command
                //var selectAllCommand = new
                //{
                //    Command = "SELECT",
                //    Mailbox = "ALL"
                //};
                //string jsonSelectAllCommand = JsonConvert.SerializeObject(selectAllCommand);
                //writer.WriteLine(jsonSelectAllCommand);
                //response = reader.ReadLine();
                //Console.WriteLine($"Server: {response}");

                // Gửi lệnh LOGOUT
                var logoutCommand = new { Command = "LOGOUT" };
                writer.WriteLine(JsonConvert.SerializeObject(logoutCommand));
                response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }
}
