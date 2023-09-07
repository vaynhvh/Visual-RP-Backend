using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("player_emails")]
    public class DbEmail
    {
        [Key]
        public uint Id { get; set; }
        public uint PlayerId { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool Readed { get; set; }
        public DateTime Date { get; set; }

        public async void UpdateReadStatus()
        {
            using var db = new RXContext();

            var email = await db.Emails.FirstOrDefaultAsync(x => x.Id == this.Id && x.PlayerId == this.PlayerId);
            if (email == null) return;

            email.Readed = true;

            await db.SaveChangesAsync();
        }
    }
}
