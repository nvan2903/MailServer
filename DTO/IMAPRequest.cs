using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class IMAPRequest
    {
        public string Command { get; set; }
        public string Username { get; set; }
        public string Fullname { get; set; }
        public string Password { get; set; }
        public string NewFullname { get; set; }
        public string NewPassword { get; set; }
        public string Oldpassword { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public string Attachment { get; set; }
        public string Mailbox { get; set; }
        public int Mailid { get; set; }


        //  Contructor for normal imap request
        public IMAPRequest(string command)
        {
            Command = command;
        }

        //  Contructor for register request
        public IMAPRequest(string command, string username, string fullname, string password)
        {
            Command = command;
            Username = username;
            Fullname = fullname;
            Password = password;
        }

        //  Contructor for login request
        public IMAPRequest(string command, string username, string password)
        {
            Command = command;
            Username = username;
            Password = password;
        }

        ////  Contructor for change name request
        //public IMAPRequest(string command, string newFullname)
        //{
        //    Command = command;
        //    NewFullname = newFullname;
        //}

        ////  Contructor for change password request
        //public IMAPRequest(string command, string oldpassword, string newPassword)
        //{
        //    Command = command;
        //    Oldpassword = oldpassword;
        //    NewPassword = newPassword;
        //}

        //  Contructor for select request
        public IMAPRequest(string command, string mailbox)
        {
            Command = command;
            Mailbox = mailbox;

        }

        //  Contructor for fetch request
        public IMAPRequest(string command, int mailid)
        {
            Command = command;
            Mailid = mailid;
        }

    }
}
