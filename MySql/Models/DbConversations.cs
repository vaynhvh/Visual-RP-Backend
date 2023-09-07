using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("phone_conversations_messages")]
    public class DbPhoneConversationMessage
    {
        [Key]
        public uint Id { get; set; }
        public uint ConversationId { get; set; }
        public string Message { get; set; }
        public DateTime TimeStamp { get; set; } = DateTime.Now;
        public uint SenderId { get; set; }
    }

    [Table("phone_conversations")]
    public class DbPhoneConversation
    {
        [Key]
        public uint Id { get; set; }
        public uint Player1 { get; set; }
        public uint Player2 { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.Now;
    }
}
