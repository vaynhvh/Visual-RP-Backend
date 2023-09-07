using Backend.Models;
using Backend.MySql;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Rank
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class RankModule : RXModule
    {
        public RankModule() : base("Ranks") { }

        public static List<RXRank> Ranks = new List<RXRank>();

        //[HandleExceptions]
        public override void LoadAsync()
        {
            Ranks.Add(new RXRank
            {
                Id = 1,
                ClothesId = 2,
                Permission = 100,
                Name = "Projektleiter",
                DiscordRole = 1139567789808570449,
                UprankPoints = 0
            });

            Ranks.Add(new RXRank
            {
                Id = 2,
                ClothesId = 2,
                Permission = 99,
                Name = "Serverleiter",
                DiscordRole = 1139567791448539146,
                UprankPoints = 0
            });

            Ranks.Add(new RXRank
            {
                Id = 3,
                ClothesId = 12,
                Permission = 98,
                Name = "Senior Entwickler",
                DiscordRole = 1139567794854318220,
                UprankPoints = 0
            });

            Ranks.Add(new RXRank
            {
                Id = 3,
                ClothesId = 12,
                Permission = 97,
                Name = "Headadministrator",
                DiscordRole = 1139567795932246128,
                UprankPoints = 0
            });

            Ranks.Add(new RXRank
            {
                Id = 4,
                ClothesId = 3,
                Permission = 96,
                Name = "Administrator",
                DiscordRole = 1139567799950389280,
                UprankPoints = 1000,
            });

            Ranks.Add(new RXRank
            {
                Id = 5,
                ClothesId = 4,
                Permission = 95,
                Name = "Moderator",
                DiscordRole = 1139567802852847648,
                UprankPoints = 800,
            });

            Ranks.Add(new RXRank
            {
                Id = 6,
                ClothesId = 11,
                Permission = 94,
                Name = "Entwickler",
                DiscordRole = 1139567802248867850,
                UprankPoints = 500,
            });

            Ranks.Add(new RXRank
            {
                Id = 7,
                ClothesId = 7,
                Permission = 92,
                Name = "Gamedesigner",
                DiscordRole = 1139567806464131195,
                UprankPoints = 300,

            });
            Ranks.Add(new RXRank
            {
                Id = 8,
                ClothesId = 5,
                Permission = 91,
                Name = "Supporter",
                DiscordRole = 1139567808246722621,
                UprankPoints = 200,
            });
            Ranks.Add(new RXRank
            {
                Id = 9,
                ClothesId = 9,
                Permission = 90,
                Name = "Guide",
                DiscordRole = 1139567810826219580,
                UprankPoints = 0,
            });
        }
    }
}
