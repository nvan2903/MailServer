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
            string sql = "UPDATE mail SET deleted_at = CURRENT_TIMESTAMP WHERE id = @MailId AND owner = @Owner";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MailId", mailId);
                    cmd.Parameters.AddWithValue("@Owner", owner);
                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    CloseConnection();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in MarkMailAsDeleted: " + ex.Message);
                return false;
            }
        }

        public bool DeleteMailPermanently(int mailId, string owner)
        {
            string sql = "DELETE FROM mail WHERE id = @MailId AND owner = @Owner";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@MailId", mailId);
                    cmd.Parameters.AddWithValue("@Owner", owner);
                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    CloseConnection();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in DeleteMailPermanently: " + ex.Message);
                return false;
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
                    CloseConnection();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in UpdateMailReadStatus: " + ex.Message);
                return false;
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
                            Mail mail = new Mail
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
                            CloseConnection();
                            return mail;
                        }
                    }
                    CloseConnection();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in GetMailById: " + ex.Message);
            }
            return new Mail(); // Return a new Mail object instead of null
        }

        public bool InsertMail(Mail mail)
        {
            string sql = "INSERT INTO mail (created_at, sender, receiver, owner, is_read, attachment, subject, content, reply, deleted_at) " +
                         "VALUES (@CreatedAt, @Sender, @Receiver, @Owner, @IsRead, @Attachment, @Subject, @Content, @Reply, NULL)";
            try
            {
                using (SqlCommand cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CreatedAt", mail.CreatedAt);
                    cmd.Parameters.AddWithValue("@Sender", mail.Sender);
                    cmd.Parameters.AddWithValue("@Receiver", mail.Receiver);
                    cmd.Parameters.AddWithValue("@Owner", mail.Owner);
                    cmd.Parameters.AddWithValue("@IsRead", mail.IsRead ? 1 : 0);

                    // Handle attachment
                    if (string.IsNullOrEmpty(mail.Attachment))
                        cmd.Parameters.AddWithValue("@Attachment", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@Attachment", mail.Attachment);

                    // Handle subject
                    if (string.IsNullOrEmpty(mail.Subject))
                        cmd.Parameters.AddWithValue("@Subject", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@Subject", mail.Subject);

                    // Set the content path
                    cmd.Parameters.AddWithValue("@Content", mail.Content);

                    // Handle reply ID (foreign key)
                    if (mail.Reply == 0)
                        cmd.Parameters.AddWithValue("@Reply", DBNull.Value);
                    else
                        cmd.Parameters.AddWithValue("@Reply", mail.Reply);

                    OpenConnection();
                    int rowsAffected = cmd.ExecuteNonQuery();
                    CloseConnection();
                    return rowsAffected > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error in InsertMail: " + ex.Message);
                return false;
            }
        }
    }
}
