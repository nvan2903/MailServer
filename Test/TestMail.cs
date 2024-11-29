//using System;
//using DTO;
//using DAL;

//namespace Test
//{
//    class MailDALTest
//    {
//        static void Main(string[] args)
//        {
//            MailDAL mailDAL = new MailDAL();

//            Console.WriteLine("Testing InsertMail:");
//            TestInsertMail(mailDAL);

//            Console.WriteLine("Testing GetMailById:");
//            TestGetMailById(mailDAL);

//            Console.WriteLine("Testing MarkMailAsDeleted:");
//            TestMarkMailAsDeleted(mailDAL);

//            Console.WriteLine("Testing DeleteMailPermanently:");
//            TestDeleteMailPermanently(mailDAL);

//            Console.WriteLine("Testing UpdateMailReadStatus:");
//            TestUpdateMailReadStatus(mailDAL);
//        }

//        static void TestInsertMail(MailDAL mailDAL)
//        {
//            var mail = new Mail
//            {
//                CreatedAt = DateTime.Now,
//                Sender = "sender@example.com",
//                Receiver = "receiver@example.com",
//                Owner = "sender@example.com",
//                IsRead = false,
//                Attachment = null,
//                Subject = "Test Subject",
//                Content = "Test Content",
//                Reply = 0
//            };

//            bool result = mailDAL.InsertMail(mail);
//            Console.WriteLine("InsertMail Result: " + result);
//        }

//        static void TestGetMailById(MailDAL mailDAL)
//        {
//            int mailId = 213; // Replace with an existing mail ID in your database
//            string owner = "sender@example.com";

//            Mail mail = mailDAL.GetMailById(mailId, owner);
//            if (mail != null)
//            {
//                Console.WriteLine("Mail found:");
//                Console.WriteLine("Subject: " + mail.Subject);
//                Console.WriteLine("Content: " + mail.Content);
//            }
//            else
//            {
//                Console.WriteLine("Mail not found.");
//            }
//        }

//        static void TestMarkMailAsDeleted(MailDAL mailDAL)
//        {
//            int mailId = 1; // Replace with an existing mail ID in your database
//            string owner = "sender@example.com";

//            bool result = mailDAL.MarkMailAsDeleted(mailId, owner);
//            Console.WriteLine("MarkMailAsDeleted Result: " + result);
//        }

//        static void TestDeleteMailPermanently(MailDAL mailDAL)
//        {
//            int mailId = 1; // Replace with an existing mail ID in your database
//            string owner = "sender@example.com";

//            bool result = mailDAL.DeleteMailPermanently(mailId, owner);
//            Console.WriteLine("DeleteMailPermanently Result: " + result);
//        }

//        static void TestUpdateMailReadStatus(MailDAL mailDAL)
//        {
//            int mailId = 213; // Replace with an existing mail ID in your database
//            bool isRead = true;

//            bool result = mailDAL.UpdateMailReadStatus(mailId, isRead);
//            Console.WriteLine("UpdateMailReadStatus Result: " + result);
//        }
//    }
//}
