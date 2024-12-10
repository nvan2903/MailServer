using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DTO
{
    public class FTPRequest
    {
        public string Command { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public string Maildir { get; set; }
        public string Filename { get; set; }
        public int Mailid { get; set; }


        //  Contructor for normal ftp request
        public FTPRequest(string command)
        {
            Command = command;
        }

        //  Contructor for start ftp request
        public FTPRequest(string command, string sender, string recipient, string maildir, string filename, int mailid)
        {
            Command = command;
            Sender = sender;
            Recipient = recipient;
        }

        //  Contructor for put and recv request 
        public FTPRequest(string command, string maildir, string filename)
        {
            Command = command;
            Maildir = maildir;
            Filename = filename;
        }




    }
}
