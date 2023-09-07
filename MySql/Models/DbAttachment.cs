using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace Backend.MySql.Models
{
    [Table("attachments")]
    public class DbAttachment
    {

        [Key]
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonIgnore]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "model")]
        public int ObjectId { get; set; }

        [JsonProperty(PropertyName = "bone")]
        public int Bone { get; set; }
        public string RawPosition { get; set; } 
        public string RawRotation { get; set; }

        [JsonProperty(PropertyName = "needsAnimation")]
        public bool NeedsAnimation { get; set; }

        [JsonProperty(PropertyName = "animationDict")]
        public string AnimDic1 { get; set; }

        [JsonProperty(PropertyName = "animationName")]
        public string AnimDic2 { get; set; }

        [JsonProperty(PropertyName = "animationFlag")]
        public int AnimFlag { get; set; }

        [JsonProperty(PropertyName = "isCarrying")]
        public bool IsCarry { get; set; }

        [NotMapped]
        [JsonProperty(PropertyName = "offset")]
        public Vector3 Position { get; set; }

        [NotMapped]
        [JsonProperty(PropertyName = "rotation")]
        public Vector3 Rotation { get; set; }
    }
}
