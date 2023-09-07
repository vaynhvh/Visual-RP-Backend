using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Gangwar;
using Backend.MySql;
using Backend.MySql.Models;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.SelfMenu
{
    public class TeamMemberObject
    {
        [JsonProperty(PropertyName = "p")]
        public uint PlayerId { get; set; }

        [JsonProperty(PropertyName = "ph")]
        public uint PlayerPhone { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "r")]
        public uint PlayerRank { get; set; }

        [JsonProperty(PropertyName = "dn")]
        public uint Dienstnummer { get; set; }

        [JsonProperty(PropertyName = "d")]
        public bool InDuty { get; set; }

        [JsonProperty(PropertyName = "v")]
        public bool InviteAccess { get; set; }

        [JsonProperty(PropertyName = "b")]
        public bool BankAccess { get; set; }

        [JsonProperty(PropertyName = "inv")]
        public bool InventoryAccess { get; set; }

        [JsonProperty(PropertyName = "l")]
        public string LastOnline { get; set; }

        [JsonProperty(PropertyName = "o")]
        public bool IsOnline { get; set; }

    }

    public class TeamObject
    {
        [JsonProperty(PropertyName = "p")]
        public uint PlayerId { get; set; }
        [JsonProperty(PropertyName = "i")]
        public uint TeamId { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string TeamName { get; set; }

        [JsonProperty(PropertyName = "motd")]
        public string MOTD { get; set; }

        [JsonProperty(PropertyName = "r")]
        public uint PlayerRank { get; set; }

        [JsonProperty(PropertyName = "v")]
        public bool InviteAccess { get; set; }

        [JsonProperty(PropertyName = "b")]
        public bool BankAccess { get; set; }

        [JsonProperty(PropertyName = "t")]
        public bool IsTeam { get; set; }

        [JsonProperty(PropertyName = "d")]
        public bool Dienstnummer { get; set; }

        [JsonProperty(PropertyName = "c")]
        public string Color { get; set; }

        [JsonProperty(PropertyName = "data")]
        public List<TeamMemberObject> teamMembers { get; set; }
    }
    class SelfMenuModule : RXModule
    {
        public SelfMenuModule() : base("SelfMenuModule", new RXWindow("Self")) { }

        [RemoteEvent]
        public async Task requestSelfMenu(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !await player.CanInteractAntiFloodNoMSG(0.5)) return;
            if (GangwarModule.IsPlayerInGangwar(player)) return;

            await this.Window.OpenWindow(player);
        }

        [RemoteEvent]
        public async Task OwnDoc(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !await player.CanInteractAntiFloodNoMSG(0.5)) return;

            var window = new RXWindow("Licences");

            using var db = new RXContext();

            var playerlicenses = await db.PlayerLicenses.Where(x => x.PlayerId == player.Id).ToListAsync();

            var licenses = new List<object>();

            foreach (var lic in playerlicenses)
            {
                licenses.Add(new
                {
                    i = lic.LicenseId,
                });
            }

            await window.OpenWindow(player, licenses);

        }


        [RemoteEvent]
        public async Task RqTeam(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !await player.CanInteractAntiFloodNoMSG(0.5)) return;
            if (GangwarModule.IsPlayerInGangwar(player)) return;

            var pteam = player.Team;

            if (pteam.Id == 0)
            {
                await player.SendNotify("Du bist in keiner Fraktion!");
                return;
            }

            var teammember = new List<TeamMemberObject>();
            using var db = new RXContext();

            foreach (var member in await db.Players.Where(x => x.TeamId == pteam.Id).ToListAsync())
            {
                var oplayer = await PlayerController.FindPlayerById(member.Id);

                if (oplayer != null)
                {
                    var teammemberdata = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == member.Id);

                    if (teammemberdata == null) continue;
                    teammember.Add(new TeamMemberObject() { PlayerId = oplayer.Id, InDuty = oplayer.InDuty, InventoryAccess = teammemberdata.Inventory, Dienstnummer = teammemberdata.Dienstnummer, PlayerPhone = oplayer.Phone, PlayerRank = oplayer.Teamrank, IsOnline = true, LastOnline = oplayer.LastSeen.ToString("dd.MM.yyyy hh:mm"), PlayerName = await oplayer.GetNameAsync(), BankAccess = oplayer.TeamMemberData.Bank, InviteAccess = oplayer.TeamMemberData.Manage });
                }
            
                  else
                 {
                      var teammemberdata = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == member.Id);
            
                     if (teammemberdata == null) continue;
                     teammember.Add(new TeamMemberObject() { PlayerId = member.Id, InDuty = false, InventoryAccess = teammemberdata.Inventory, Dienstnummer = teammemberdata.Dienstnummer, PlayerPhone = member.Phone, PlayerRank = member.TeamrankId, IsOnline = false, LastOnline = member.LastSeen.ToString("dd.MM.yyyy hh:mm"), PlayerName = member.Username, BankAccess = teammemberdata.Bank, InviteAccess = teammemberdata.Manage });
                }
              }
            teammember = teammember.OrderBy(x => x.PlayerRank).Reverse().ToList();

            await player.TriggerEventAsync("RsTeam", NAPI.Util.ToJson(teammember));

        }

        [RemoteEvent]
        public async Task OpenTeam(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !await player.CanInteractAntiFloodNoMSG(0.5)) return;
            if (GangwarModule.IsPlayerInGangwar(player)) return;

            var window = new RXWindow("Team");

            var pteam = player.Team;

            if (pteam.Id == 0)
            {
                await player.SendNotify("Du bist in keiner Fraktion!");
                return;
            }

            bool inviteperms = false;
            bool bankperms = false;

            if (player.Teamrank > 9)
            {
                inviteperms = true;
                bankperms = true;
            } else
            {

                if (player.TeamMemberData.Manage)
                {
                    inviteperms = true;
                }
                if (player.TeamMemberData.Bank)
                {
                    bankperms = true;
                }

            }


            var team = new TeamObject() { PlayerId = player.Id,  Dienstnummer = pteam.IsLowestState(), IsTeam = true, MOTD = pteam.MOTD.Replace("\"","'"), TeamId = pteam.Id, Color = pteam.Hex, TeamName = pteam.Name, BankAccess = bankperms, InviteAccess = inviteperms, PlayerRank = player.Teamrank };

            var teammember = new List<TeamMemberObject>();

            using var db = new RXContext();

            foreach (var member in await db.Players.Where(x => x.TeamId == pteam.Id).ToListAsync())
            {
                var oplayer = await PlayerController.FindPlayerById(member.Id);

                if (oplayer != null)
                {
                    var teammemberdata = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == member.Id);

                    if (teammemberdata == null) continue;
                    teammember.Add(new TeamMemberObject() { PlayerId = oplayer.Id, InDuty = oplayer.InDuty, InventoryAccess = teammemberdata.Inventory, Dienstnummer = teammemberdata.Dienstnummer, PlayerPhone = oplayer.Phone, PlayerRank = oplayer.Teamrank, IsOnline = true, LastOnline = oplayer.LastSeen.ToString("dd.MM.yyyy hh:mm"), PlayerName = await oplayer.GetNameAsync(), BankAccess = oplayer.TeamMemberData.Bank, InviteAccess = oplayer.TeamMemberData.Manage });
                }
            }
            //       else
            //      {
            //          var teammemberdata = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == member.Id);
            //
            //         if (teammemberdata == null) continue;
            //         teammember.Add(new TeamMemberObject() { PlayerId = member.Id, PlayerRank = member.TeamrankId, IsOnline = false, LastOnline = member.LastSeen, PlayerName = member.Username, BankAccess = teammemberdata.Bank, InviteAccess = teammemberdata.Manage });
            //    }
            //    }


            team.teamMembers = teammember.OrderBy(x => x.PlayerRank).Reverse().ToList();


            await window.OpenWindow(player, team);
        }

    }

    public class SortIntDescending : IComparer<int>
    {
        int IComparer<int>.Compare(int a, int b) //implement Compare
        {
            if (a > b)
                return -1; //normally greater than = 1
            if (a < b)
                return 1; // normally smaller than = -1
            else
                return 0; // equal
        }
    }

}
