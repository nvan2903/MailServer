using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class SMTPRequest
    {
        public string Command { get; set; }
        public string Email { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Filename { get; set; }

        //  Contructor for normal smtp request
        public SMTPRequest(string command, string email, string subject, string content, string filename)
        {
            Command = command;
        }

        //  Contructor for sender and receiver request
        public SMTPRequest(string command, string email)
        {
            Command = command;
            Email = email;
        }

        //  Contructor for start data request
        public SMTPRequest(string command, string subject, string content)
        {
            Command = command;
            Subject = subject;
            Content = content;
        }

        //  Contructor for attach request
        //public SMTPRequest(string command, string filename)
        //{
        //    Command = command;
        //    Filename = filename;
        //}



    }
}
