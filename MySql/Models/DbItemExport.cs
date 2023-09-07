using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("itemexport_items")]
    public class DbItemExportItem
    {

        [Key]
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonIgnore]
        public uint ExportId { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "png")]
        public string ItemImage { get; set; }

        [JsonProperty(PropertyName = "p")]
        public uint Price { get; set; }

    }
    
    [Table("itemexport")]
    public class DbItemExport
    {

        [Key]
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "desc")]
        public string Description { get; set; }

        [JsonIgnore]
        public string Position { get; set; }

        [JsonIgnore]
        public string PedHash { get; set; }

        [JsonIgnore]
        public float PedHeading { get; set; }

        [NotMapped]
        [JsonProperty(PropertyName = "items")]
        public List<DbItemExportItem> items = new List<DbItemExportItem>();
    }
}
