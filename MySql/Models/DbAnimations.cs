using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("animation_categories")]
    public class DbAnimationCategory
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
    }

    [Table("animation_items")]
    public class DbAnimationItem
    {
        [Key]
        public uint Id { get; set; }

        public string Text { get; set; }

        public string Dict { get; set; }
        public string Name { get; set; }
        public int Flag { get; set; }

    }
}
