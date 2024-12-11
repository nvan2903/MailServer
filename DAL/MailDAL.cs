using System;
using System.Data;
using System.Data.SqlClient;
using DTO;

namespace DAL
{
    public class MailDAL : DatabaseConnection
    {
        public bool MarkMailAsDeleted(int mailId, string owner)
        {
            string sql = "UPDATE mail SET deleted_at = CURRENT_TIMESTAMP WHERE id = @MailId AND owner = N@Owner";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MailId", mailId);
                    cmd.Parameters.AddWithValue("@Owner", owner);
                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MarkMailAsDeleted: " + ex.Message);
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        // method to restore mail 
        public bool RestoreMail(int mailId, string owner)
        {
            string sql = "UPDATE mail SET deleted_at = NULL WHERE id = @MailId AND owner = @Owner";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MailId", mailId);
                    cmd.Parameters.AddWithValue("@Owner", owner);
                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in RestoreMail: " + ex.Message);
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }
        public bool UpdateMailReadStatus(int mailId, bool isRead)
        {
            string sql = "UPDATE mail SET is_read = @IsRead WHERE id = @MailId";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MailId", mailId);
                    cmd.Parameters.AddWithValue("@IsRead", isRead ? 1 : 0);
                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateMailReadStatus: " + ex.Message);
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }

        public Mail GetMailById(int mailId, string owner)
        {
            string sql = "SELECT created_at, sender, receiver, owner, is_read, attachment, subject, content, reply, deleted_at FROM mail WHERE id = @MailId AND owner = @Owner";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MailId", mailId);
                    cmd.Parameters.AddWithValue("@Owner", owner);
                    OpenConnection();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Mail
                            {
                                Id = mailId,
                                CreatedAt = Convert.ToDateTime(reader["created_at"]),
                                Sender = reader["sender"]?.ToString() ?? string.Empty,
                                Receiver = reader["receiver"]?.ToString() ?? string.Empty,
                                Owner = reader["owner"]?.ToString() ?? string.Empty,
                                IsRead = Convert.ToBoolean(reader["is_read"]),
                                Attachment = reader["attachment"] == DBNull.Value ? null : reader["attachment"]?.ToString(),
                                Subject = reader["subject"] == DBNull.Value ? null : reader["subject"]?.ToString(),
                                Content = reader["content"]?.ToString() ?? string.Empty,
                                Reply = reader["reply"] == DBNull.Value ? 0 : Convert.ToInt32(reader["reply"]),
                                DeletedAt = reader["deleted_at"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["deleted_at"])
                            };
                        }
                        else
                        {
                            Console.WriteLine($"Mail with ID {mailId} and owner {owner} not found.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetMailById: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return null;
        }


        public bool InsertMail(Mail mail)
        {
            string sql = "INSERT INTO mail (created_at, sender, receiver, owner, is_read, attachment, subject, content, reply, deleted_at) " +
                         "VALUES (@CreatedAt, @Sender, @Receiver, @Owner, @IsRead, @Attachment, @Subject, @Content, NULL, NULL)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CreatedAt", mail.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@Sender", mail.Sender);
                    cmd.Parameters.AddWithValue("@Receiver", mail.Receiver);
                    cmd.Parameters.AddWithValue("@Owner", mail.Owner);
                    cmd.Parameters.AddWithValue("@IsRead", mail.IsRead ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Attachment", mail.Attachment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subject", mail.Subject ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Content", mail.Content ?? (object)DBNull.Value);



                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in InsertMail: " + ex.Message);
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }



        public bool InsertReplyMail(Mail mail)
        {
            string sql = "INSERT INTO mail (created_at, sender, receiver, owner, is_read, attachment, subject, content, reply, deleted_at) " +
                         "VALUES (@CreatedAt, @Sender, @Receiver, @Owner, @IsRead, @Attachment, @Subject, @Content, @Reply, NULL)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CreatedAt", mail.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@Sender", mail.Sender);
                    cmd.Parameters.AddWithValue("@Receiver", mail.Receiver);
                    cmd.Parameters.AddWithValue("@Owner", mail.Owner);
                    cmd.Parameters.AddWithValue("@IsRead", mail.IsRead ? 1 : 0);
                    cmd.Parameters.AddWithValue("@Attachment", mail.Attachment ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Subject", mail.Subject ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Content", mail.Content ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Reply", mail.Reply);



                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in InsertMail: " + ex.Message);
                return false;
            }
            finally
            {
                CloseConnection();
            }
        }




        // method to get inbox mails
        public DataTable GetInboxMails(string currentUser)
        {
            string sql = "SELECT id, created_at, sender, receiver, is_read, attachment, subject, content, reply FROM mail WHERE owner = @CurrentUser AND deleted_at IS NULL";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentUser", currentUser);
                    OpenConnection();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetInboxMails: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return new DataTable();
        }

        // method to get sent mails
        public DataTable GetSentMails(string currentUser)
        {
            string sql = "SELECT id, created_at, sender, receiver, is_read, attachment, subject, content, reply FROM mail WHERE sender = @CurrentUser AND deleted_at IS NULL";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentUser", currentUser);
                    OpenConnection();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetSentMails: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return new DataTable();
        }

        // method to get deleted mails
        public DataTable GetDeletedMails(string currentUser)
        {
            string sql = "SELECT id, created_at, sender, receiver, is_read, attachment, subject, content, reply FROM mail WHERE owner = @CurrentUser AND deleted_at IS NOT NULL";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentUser", currentUser);
                    OpenConnection();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetDeletedMails: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return new DataTable();
        }

        // method to get all mails
        public DataTable GetAllMails(string currentUser)
        {
            string sql = "SELECT id, created_at, sender, receiver, is_read, attachment, subject, content, reply FROM mail WHERE sender = @CurrentUser or receiver = @CurrentUser";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CurrentUser", currentUser);
                    OpenConnection();
                    using (SqlDataAdapter adapter = new SqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        return dt;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetAllMails: " + ex.Message);
            }
            finally
            {
                CloseConnection();
            }
            return new DataTable();
        }
    }
}
