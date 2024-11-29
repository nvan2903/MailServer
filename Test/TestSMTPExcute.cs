//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net.Sockets;
//using System.Text;
//using System.Threading.Tasks;

//namespace Test
//{
//    internal class TestSMTPExcute
//    {
//        static async Task Main(string[] args)
//        {
//            string server = "localhost";
//            int port = 25; // Use the port your SMTP server is listening on

//            try
//            {
//                using TcpClient client = new TcpClient();
//                client.ReceiveTimeout = 10000; // 10 seconds
//                client.SendTimeout = 10000; // 10 seconds
//                await client.ConnectAsync(server, port);

//                using NetworkStream stream = client.GetStream();
//                using StreamReader reader = new StreamReader(stream, Encoding.ASCII);
//                using StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

//                // Read the server's initial response
//                string response = await reader.ReadLineAsync();
//                Console.WriteLine($"Server: {response}");

//                // Send HELO command
//                await writer.WriteLineAsync("HELO localhost");
//                response = await reader.ReadLineAsync();
//                Console.WriteLine($"Server: {response}");

//                // Send MAIL FROM command
//                await writer.WriteLineAsync("MAIL FROM:<sender@example.com>");
//                response = await reader.ReadLineAsync();
//                Console.WriteLine($"Server: {response}");

//                // Send RCPT TO command
//                await writer.WriteLineAsync("RCPT TO:<recipient@example.com>");
//                response = await reader.ReadLineAsync();
//                Console.WriteLine($"Server: {response}");

//                // Send DATA command
//                await writer.WriteLineAsync("DATA");
//                response = await reader.ReadLineAsync();
//                Console.WriteLine($"Server: {response}");

//                // Send email data
//                await writer.WriteLineAsync("Subject: Test Email");
//                await writer.WriteLineAsync("This is a test email.");
//                await writer.WriteLineAsync("."); // End of data
//                response = await reader.ReadLineAsync();
//                Console.WriteLine($"Server: {response}");

//                // Send QUIT command
//                await writer.WriteLineAsync("QUIT");
//                response = await reader.ReadLineAsync();
//                Console.WriteLine($"Server: {response}");
//            }
//            catch (SocketException ex)
//            {
//                Console.WriteLine($"SocketException: {ex.Message}");
//            }
//            catch (IOException ex)
//            {
//                Console.WriteLine($"IOException: {ex.Message}");
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Exception: {ex.Message}");
//            }
//        }
//    }
//}
