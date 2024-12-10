//using System;
//using System.IO;
//using System.Net.Sockets;
//using System.Text;
//using Newtonsoft.Json;

//namespace Test
//{
//    class TestReply
//    {
//        static void Main(string[] args)
//        {
//            string server = "localhost"; // Đổi thành địa chỉ server của bạn
//            int port = 25; // Cổng SMTP mặc định là 25

//            try
//            {
//                using TcpClient client = new TcpClient(server, port);
//                using NetworkStream stream = client.GetStream();
//                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
//                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

//                // Đọc lời chào từ server
//                string response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Gửi lệnh HELO
//                var heloCommand = new { Command = "HELO" };
//                writer.WriteLine(JsonConvert.SerializeObject(heloCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Gửi lệnh MAIL FROM
//                var mailFromCommand = new
//                {
//                    Command = "MAIL FROM",
//                    Email = "trungnd.21it@vku.udn.vn"
//                };
//                writer.WriteLine(JsonConvert.SerializeObject(mailFromCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Gửi lệnh RCPT TO
//                var rcptToCommand = new
//                {
//                    Command = "RCPT TO",
//                    Email = "vantn.21it@vku.udn.vn"
//                };
//                writer.WriteLine(JsonConvert.SerializeObject(rcptToCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Gửi lệnh REPLY
//                var replyCommand = new
//                {
//                    Command = "REPLY",
//                    Mailid = 307
//                };
//                writer.WriteLine(JsonConvert.SerializeObject(replyCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Gửi lệnh DATA
//                var dataCommand = new
//                {
//                    Command = "DATA",
//                    Subject = "Trung Reply Mail To Van",
//                    Content = "Trung has reply"
//                };
//                writer.WriteLine(JsonConvert.SerializeObject(dataCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Gửi lệnh ATTACH (đính kèm tệp)
//                var attachCommand = new
//                {
//                    Command = "ATTACH",
//                    FilePath = "Attachment.txt" // Thay đường dẫn bằng file thực tế
//                };
//                writer.WriteLine(JsonConvert.SerializeObject(attachCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Gửi lệnh QUIT
//                var quitCommand = new { Command = "QUIT" };
//                writer.WriteLine(JsonConvert.SerializeObject(quitCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error: {ex.Message}");
//            }
//        }
//    }
//}
