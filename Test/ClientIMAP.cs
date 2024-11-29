using System;
using System.Net.Sockets;
using System.Text;
using Newtonsoft.Json;

namespace Test
{
    class ClientIMAP
    {
        static void Main(string[] args)
        {
            string server = "localhost"; // Change to your server address
            int port = 143; // Change to your server port

            try
            {
                using TcpClient client = new TcpClient(server, port);
                using NetworkStream stream = client.GetStream();
                using StreamReader reader = new StreamReader(stream, Encoding.ASCII);
                using StreamWriter writer = new StreamWriter(stream, Encoding.ASCII) { AutoFlush = true };

                // Read server greeting
                string response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");

                // Send CAPABILITY command
                var capabilityCommand = new
                {
                    Command = "CAPABILITY"
                };
                string jsonCapabilityCommand = JsonConvert.SerializeObject(capabilityCommand);
                writer.WriteLine(jsonCapabilityCommand);
                response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");

                // Send REGISTER command
                var registerCommand = new
                {
                    Command = "REGISTER",
                    Username = "tuanvd.21it",
                    Fullname = "Võ Đức Tuân",
                    Password = "12345678"
                };
                string jsonRegisterCommand = JsonConvert.SerializeObject(registerCommand);
                writer.WriteLine(jsonRegisterCommand);
                response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");

                // Send LOGIN command
                var loginCommand = new
                {
                    Command = "LOGIN",
                    Username = "trungnd.21it",
                    Password = "12345678"
                };
                string jsonLoginCommand = JsonConvert.SerializeObject(loginCommand);
                writer.WriteLine(jsonLoginCommand);
                response = reader.ReadLine();
                Console.WriteLine($"Server: {response}");

                // Send LOGOUT command
                var logoutCommand = new
                {
                    Command = "LOGOUT"
                };
                string jsonLogoutCommand = JsonConvert.SerializeObject(logoutCommand);
                writer.WriteLine(jsonLogoutCommand);
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
