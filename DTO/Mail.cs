using System;

namespace DTO
{
    public class Mail
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Sender { get; set; }
        public string SenderName { get; set; }
        public string Receiver { get; set; }
        public string ReceiverName { get; set; }
        public int? Reply { get; set; }
        public string Owner { get; set; }
        public bool IsRead { get; set; }
        public string Attachment { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Default Constructor
        public Mail()
        {
            Id = 0;
            CreatedAt = DateTime.Now;
            Sender = string.Empty;
            SenderName = string.Empty;
            Receiver = string.Empty;
            ReceiverName = string.Empty;
            Reply = 0;
            Owner = string.Empty;
            IsRead = false;
            Attachment = string.Empty;
            Subject = string.Empty;
            Content = string.Empty;
            DeletedAt = null;
        }

        // Parameterized Constructor
        public Mail(int id, DateTime createdAt, string sender, string receiver, int reply, string owner, bool isRead, string attachment, string subject, string content, DateTime? deletedAt)
        {
            Id = id;
            CreatedAt = createdAt;
            Sender = sender;
            Receiver = receiver;
            Reply = reply;
            Owner = owner;
            IsRead = isRead;
            Attachment = attachment;
            Subject = subject;
            Content = content;
            DeletedAt = deletedAt;
        }
    }
}
