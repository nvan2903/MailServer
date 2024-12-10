//using System;
//using System.IO;
//using System.Net.Sockets;
//using System.Text;
//using Newtonsoft.Json;

//namespace Test
//{
//    class TestForward
//    {
//        static void Main(string[] args)
//        {
//            string server = "localhost"; // Change to your server address
//            int port = 25; // Default SMTP port is 25

//            try
//            {
//                using TcpClient client = new TcpClient(server, port);
//                using NetworkStream stream = client.GetStream();
//                using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
//                using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

//                // Read server greeting
//                string response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Send HELO command
//                var heloCommand = new { Command = "HELO" };
//                writer.WriteLine(JsonConvert.SerializeObject(heloCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Send FORWARD FROM command
//                var forwardFromCommand = new
//                {
//                    Command = "FORWARD FROM",
//                    Email = "trungnd.21it@vku.udn.vn"
//                };
//                writer.WriteLine(JsonConvert.SerializeObject(forwardFromCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Send FORWARD TO command
//                var forwardToCommand = new
//                {
//                    Command = "FORWARD TO",
//                    Email = "hieutnn.21it@vku.udn.vn"
//                };
//                writer.WriteLine(JsonConvert.SerializeObject(forwardToCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Send MAIL FORWARD command
//                var mailForwardCommand = new
//                {
//                    Command = "MAIL FORWARD",
//                    Mailid = 307
//                };
//                writer.WriteLine(JsonConvert.SerializeObject(mailForwardCommand));
//                response = reader.ReadLine();
//                Console.WriteLine($"Server: {response}");

//                // Send QUIT command
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
