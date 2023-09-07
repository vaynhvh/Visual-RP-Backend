using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Backend.MySql.Models
{
    public class DbPlayer
    {
        [Key]
        public uint Id { get; set; }
        public string Username { get; set; }
        public string DiscordID { get; set; }
        public string password { get; set; }
        public string DateOfEntry { get; set; } = DateTime.Now.ToString("dd.MM.yyyy");
        public DateTime LastSeen { get; set; } = DateTime.Now;
        public int Cash { get; set; } = 0;
        public int Blackmoney { get; set; } = 0;
        public int ForumId { get; set; }
        public uint RankId { get; set; } = 0;
        public uint TeamId { get; set; } = 0;
        public string WalletAdress { get; set; } = "";
        public double WalletValue { get; set; } = 0;
        public uint TeamrankId { get; set; } = 0;
        public uint InventoryId { get; set; } = 0;
        public uint GangwarContainerId { get; set; } = 0;
        public uint WorkstationId { get; set; } = 0;
        public uint WorkstationInputContainerId { get; set; } = 0;
        public uint WorkstationOutputContainerId { get; set; } = 0;
        public uint LabOutputContainerId { get; set; } = 0;
        public uint LabInputContainerId { get; set; } = 0;
        public uint BankAccountId { get; set; } = 0;
        public uint Jailtime { get; set; } = 0;
        public int Level { get; set; } = 0;
        public int Warns { get; set; } = 0;
        public string Weapons { get; set; } = "[]";
        public string Position { get; set; } = "0,0,0";
        public string Storages { get; set; } = "[]";
        public string FunkFav { get; set; } = "[]";
        public string SocialClubId { get; set; }
        public string SocialClubNumber { get; set; }
        public string HWID { get; set; }
        public string IP { get; set; }
        public uint Phone { get; set; } = 0;
        public string ClientHash { get; set; }
        public bool DeathStatus { get; set; } = false;
        public DateTime DeathTime { get; set; } = DateTime.Now;
        public DateTime LuckyWheel { get; set; } = DateTime.Now.AddDays(-1);
        public bool Coma { get; set; } = false;
        public bool InDuty { get; set; } = false;
        public int HP { get; set; } = 200;
        public int Armor { get; set; } = 0;
        public int IsMale { get; set; } = 1;
        public int Paytime { get; set; } = 180;
        public int PTA { get; set; } = 0;
        public int PTAPoints { get; set; } = 0;
        public int PTAWarns { get; set; } = 0;
        public int Stress { get; set; } = 0;
        public int Sport { get; set; } = 0;
        public int Hunger { get; set; } = 50;
        public int Thirst { get; set; } = 50;

        public uint HouseId { get; set; }
        public DateTime BanExpires { get; set; } = new DateTime(0);
        public string AnimationShortcuts { get; set; } = "{}";

    }
}
