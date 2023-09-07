using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("login_songs")]
    public class DbLoadingscreenSongs
    {

        [Key]
        public uint Id { get; set; }
        public string Text { get; set; }
        public string Url { get; set; }

    }
}
