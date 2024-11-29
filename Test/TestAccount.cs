//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using DAL;
//using DTO;

//namespace Test
//{
//    public class AccountDALTest
//    {
//        public static void Main(string[] args)
//        {
//            AccountDAL accountDAL = new AccountDAL();

//            // Test InsertAccount
//            //TestInsertAccount(accountDAL);

//            // Test UpdateFullName
//            //TestUpdateFullName(accountDAL);

//            // Test UpdatePassword
//            //TestUpdatePassword(accountDAL);

//            // Test GetAccountByEmail
//            TestGetAccountByEmail(accountDAL);

//            // Test AuthenticateAccount
//            TestAuthenticateAccount(accountDAL);
//        }

//        private static void TestInsertAccount(AccountDAL accountDAL)
//        {
//            Account newAccount = new Account
//            {
//                EmailAddress = "receiver@example.com",
//                Fullname = "Test Receiver Fullname",
//                Password = "password123" // Ensure to hash the password if needed in production
//            };

//            bool isInserted = accountDAL.InsertAccount(newAccount);
//            Console.WriteLine("InsertAccount Test: " + (isInserted ? "Passed" : "Failed"));
//        }

//        private static void TestUpdateFullName(AccountDAL accountDAL)
//        {
//            bool isUpdated = accountDAL.UpdateFullName("test@example.com", "Updated Test User");
//            Console.WriteLine("UpdateFullName Test: " + (isUpdated ? "Passed" : "Failed"));
//        }

//        private static void TestUpdatePassword(AccountDAL accountDAL)
//        {
//            bool isUpdated = accountDAL.UpdatePassword("test@example.com", "password123", "newpassword123");
//            Console.WriteLine("UpdatePassword Test: " + (isUpdated ? "Passed" : "Failed"));
//        }

//        private static void TestGetAccountByEmail(AccountDAL accountDAL)
//        {
//            Account account = accountDAL.GetAccount("test@example.com", "newpassword123");
//            if (account != null)
//            {
//                Console.WriteLine("GetAccountByEmail Test: Passed");
//                Console.WriteLine($"Full Name: {account.Fullname}");
//            }
//            else
//            {
//                Console.WriteLine("GetAccountByEmail Test: Failed");
//            }
//        }

//        private static void TestAuthenticateAccount(AccountDAL accountDAL)
//        {
//            Account account = accountDAL.GetAccount("test@example.com", "newpassword123");
//            bool isAuthenticated = account != null;
//            Console.WriteLine("AuthenticateAccount Test: " + (isAuthenticated ? "Passed" : "Failed"));
//        }
//    }
//}
