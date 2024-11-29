using System;
using System.Security.Cryptography;
using System.Text;

namespace BLL
{
    public class SecurityBLL
    {
        public static string Sha256(string input)
        {
            try
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                    byte[] hashBytes = sha256.ComputeHash(inputBytes);
                    StringBuilder hexString = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        hexString.AppendFormat("{0:x2}", b);
                    }
                    return hexString.ToString();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return string.Empty;
            }
        }
    }
}