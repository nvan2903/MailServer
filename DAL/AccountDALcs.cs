using System;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class AccountDAL : DatabaseConnection
    {
        // Method to update the full name of an account
        public bool UpdateFullName(string emailAddress, string fullname)
        {
            string sql = "UPDATE account SET fullname = @fullname WHERE email_address = @Email";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@fullname", fullname);
                    cmd.Parameters.AddWithValue("@Email", emailAddress);
                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateFullName: " + ex.Message);
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Method to update the password of an account
        public bool UpdatePassword(string emailAddress, string oldPassword, string newPassword)
        {
            string sql = "UPDATE account SET password = @newPassword WHERE email_address = @Email AND password = @oldPassword";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", emailAddress);
                    cmd.Parameters.AddWithValue("@oldPassword", oldPassword);
                    cmd.Parameters.AddWithValue("@newPassword", newPassword);
                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdatePassword: " + ex.Message);
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Method to get the full name of an account by sender's email
        public string GetFullNameByEmail(string email)
        {
            string sql = "SELECT fullname FROM account WHERE email_address = @Email";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", email);
                    OpenConnection();
                    object result = cmd.ExecuteScalar();
                    return result?.ToString();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetFullNameByEmail: " + ex.Message);
                return null;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Method to retrieve account details by email and password for login authentication
        public Account GetAccount(string emailAddress, string password)
        {
            string sql = "SELECT * FROM account WHERE email_address = @Email AND password = @Password";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", emailAddress);
                    cmd.Parameters.AddWithValue("@Password", password);
                    OpenConnection();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Account
                            {
                                EmailAddress = reader["email_address"].ToString(),
                                Fullname = reader["fullname"].ToString(),
                                Password = reader["password"].ToString(),
                                CreatedAt = Convert.ToDateTime(reader["created_at"])
                            };
                        }
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAccount: " + ex.Message);
                return null;
            }
            finally
            {
                CloseConnection();
            }
        }

        // Method to insert a new account into the database
        public bool InsertAccount(Account account)
        {
            string sql = "INSERT INTO account (email_address, fullname, password, created_at) VALUES (@Email, @Fullname, @Password, current_timestamp)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@Email", account.EmailAddress);
                    cmd.Parameters.AddWithValue("@Fullname", account.Fullname);
                    cmd.Parameters.AddWithValue("@Password", account.Password);
                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in InsertAccount: " + ex.Message);
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }
    }
}
