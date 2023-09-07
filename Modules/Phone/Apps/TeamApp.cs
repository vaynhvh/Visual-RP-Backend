using Backend.Controllers;
using Backend.Models;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone.Apps
{
    public class TeamMember
    {
        [JsonProperty(PropertyName = "id")] public uint Id { get; }
        [JsonProperty(PropertyName = "name")] public string Name { get; }
        [JsonProperty(PropertyName = "number")] public uint Number { get; }
        [JsonProperty(PropertyName = "rank")] public uint Rank { get; }

        [JsonProperty(PropertyName = "inventory")]
        public bool Inventory { get; }

        [JsonProperty(PropertyName = "bank")] public bool Bank { get; }
        [JsonProperty(PropertyName = "manage")] public int Manage { get; }

        public TeamMember(uint id, string name, uint rank, bool inventory, bool bank, int manage, uint number)
        {
            Id = id;
            Name = name;
            Rank = rank;
            Inventory = inventory;
            Bank = bank;
            Manage = manage;
            Number = number;
        }
    }

    public class MembersManageObject
    {
        public List<TeamMember> TeamMemberList { get; set; }
        public int ManagePermission { get; set; }
    }

    class TeamApp : RXModule
    {
        public TeamApp() : base("TeamApp", new RXWindow("TeamListApp")) { }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task requestTeamMembers(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

            List<TeamMember> teamMembers = new List<TeamMember>();

            int managePermission = player.Teamrank > 9 ? (player.Teamrank == 12 ? 2 : 1) : 0;

            List<RXPlayer> teamPlayers = await NAPI.Task.RunReturnAsync(() => PlayerController.GetValidPlayers().Where(x => x.TeamId == player.TeamId).ToList());

            foreach (RXPlayer target in teamPlayers)
            {
                var memberData = target.TeamMemberData;
                if (memberData == null) continue;

                int manage = target.Teamrank > 10 ? (target.Teamrank == 12 ? 2 : 1) : memberData.Manage ? 1 : 0;
                int medic = target.Team.MedicPlayer == target.Id ? 1 : 0;

                teamMembers.Add(new TeamMember(target.Id, await target.GetNameAsync(), target.Teamrank, memberData.Inventory, memberData.Bank, manage, target.Phone));
            }

            teamMembers = teamMembers.OrderByDescending(t => t.Rank).ToList();

            await this.Window.TriggerEvent(player, "responseTeamMembers", JsonConvert.SerializeObject(new MembersManageObject
            {
                ManagePermission = managePermission,
                TeamMemberList = teamMembers
            }));
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task addPlayerConfirmed(RXPlayer player, uint fraktion, string invitingPersonName)
        {
            if (!player.IsLoggedIn) return;

            var inviter = await PlayerController.FindPlayerByName(invitingPersonName);
            if (inviter == null) return;


            if (player.TeamId != 0)
            {
                await player.SendNotify("Du bist bereits in einer Fraktion!", 3500, "red");
                return;
            }

            await player.TriggerEventAsync("updateTeamId", inviter.TeamId);

            player.TeamId = inviter.TeamId;
            player.Teamrank = 0;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
            if (dbPlayer == null) return;

            var memberData = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == player.Id);
            if (memberData == null) return;

            memberData.Inventory = false;
            memberData.Manage = false;
            memberData.Bank = false;

            dbPlayer.TeamId = inviter.TeamId;
            dbPlayer.TeamrankId = 0;

            await db.SaveChangesAsync();

            await PhoneModule.requestApps(player);

            player.Team.SendNotification($"{await player.GetNameAsync()} ist jetzt ein Mitglied - {fraktion}!");
        }


        //[HandleExceptions]
        [RemoteEvent]
        public async Task ChangeDutynumber(RXPlayer player, uint memberId, uint dn)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == memberId);
            if (dbPlayer == null) return;

            if (dbPlayer.TeamId != null && player.Team != null && dbPlayer.TeamId == player.Team.Id && player.Teamrank > dbPlayer.TeamrankId && player.Teamrank > 9)
            {


                    var memberData = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == memberId);
                    if (memberData == null) return;

                memberData.Dienstnummer = dn;

                await db.SaveChangesAsync();

                await player.SendNotify("Die Änderungen wurden erfolgreich gespeichert!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
            }
            else
            {
                await player.SendNotify("Du besitzt dafür keine Berechtigung!");
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task ChangeTeamRank(RXPlayer player, uint memberId, uint rank)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == memberId);
            if (dbPlayer == null) return;

            if (dbPlayer.TeamId != null && player.Team != null && dbPlayer.TeamId == player.Team.Id && player.Teamrank > dbPlayer.TeamrankId && player.Teamrank > 9 && rank < player.Teamrank)
            {


                dbPlayer.TeamrankId = rank;

                var target = await PlayerController.FindPlayerById(memberId);
                if (target != null)
                {

                    target.Teamrank = rank;
                }

                await db.SaveChangesAsync();

                await player.SendNotify("Die Änderungen wurden erfolgreich gespeichert!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
            }
            else
            {
                await player.SendNotify("Du besitzt dafür keine Berechtigung!");
            }
        }


        //[HandleExceptions]
        [RemoteEvent]
        public async Task ChangeTeamBank(RXPlayer player, uint memberId, bool manage)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == memberId);
            if (dbPlayer == null) return;

            if (dbPlayer.TeamId != null && player.Team != null && dbPlayer.TeamId == player.Team.Id && player.Teamrank > 9 && player.TeamMemberData != null && player.TeamMemberData.Manage)
            {



                var teamMemberData = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == dbPlayer.Id);
                if (teamMemberData == null) return;

                teamMemberData.Bank = manage;


                await db.SaveChangesAsync();

                await player.SendNotify("Die Änderungen wurden erfolgreich gespeichert!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
            }
            else
            {
                await player.SendNotify("Du besitzt dafür keine Berechtigung!");
            }
        }


        //[HandleExceptions]
        [RemoteEvent]
        public async Task ChangeTeamMOTD(RXPlayer player, string motd)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

     
            if (player.Teamrank > 9)
            {
                using var db = new RXContext();

                var dbTeam = await db.Teams.FirstOrDefaultAsync(x => x.Id == player.Team.Id);

                if (dbTeam == null) return;
                dbTeam.MOTD = motd;
                player.Team.MOTD = motd;

                await db.SaveChangesAsync();

                await player.SendNotify("Die Änderungen wurden erfolgreich gespeichert!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);

            }
            else
            {
                await player.SendNotify("Du besitzt dafür keine Berechtigung!");
            }
        }


        //[HandleExceptions]
        [RemoteEvent]
        public async Task SendTeamNotify(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;


            if (player.TeamMemberData != null && player.Teamrank > 9)
            {
                foreach (var target in PlayerController.GetValidPlayers().Where(x => x.Team.Id == player.TeamId))
                {
                    await target.SendNotify(await player.GetNameAsync() + " sendet euch die Aufforderung in den Funk zu kommen!", 5000, player.Team.Hex, player.Team.Name);
                    await target.TriggerEventAsync("VoiceSound", "radioAlarm", false, "phoneSound");
                
                }

            }
            else
            {
                await player.SendNotify("Du besitzt dafür keine Berechtigung!");
            }
        }
        [RemoteEvent]
        public async Task ChangeTeamInventory(RXPlayer player, uint memberId, bool manage)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

            using var db = new RXContext();


            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == memberId);
            if (dbPlayer == null) return;

            if (dbPlayer.TeamId != null && player.Team != null && dbPlayer.TeamId == player.Team.Id && player.Teamrank > 9 && player.TeamMemberData != null && player.TeamMemberData.Manage)
            {




                var teamMemberData = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == memberId);
                if (teamMemberData == null) return;

                teamMemberData.Inventory = manage;


                await db.SaveChangesAsync();

                await player.SendNotify("Die Änderungen wurden erfolgreich gespeichert!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
            }
            else
            {
                await player.SendNotify("Du besitzt dafür keine Berechtigung!");
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task ChangeTeamManage(RXPlayer player, uint memberId, bool manage)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

            using var db = new RXContext();


            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == memberId);
            if (dbPlayer == null) return;

            if (dbPlayer.TeamId != null && player.Team != null && dbPlayer.TeamId == player.Team.Id && player.Teamrank > 9)
            {




                var teamMemberData = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == memberId);
                if (teamMemberData == null) return;

                teamMemberData.Manage = manage;
 

                await db.SaveChangesAsync();

                await player.SendNotify("Die Änderungen wurden erfolgreich gespeichert!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
            }
            else
            {
                await player.SendNotify("Du besitzt dafür keine Berechtigung!");
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task KickMemberFromTeam(RXPlayer player, uint memberId)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == memberId);
            if (dbPlayer == null) return;

            if (dbPlayer.TeamId != null && player.Team != null && dbPlayer.TeamId == player.Team.Id && player.Teamrank > dbPlayer.TeamrankId && player.Teamrank > 9)
            {



                var target = await PlayerController.FindPlayerById(memberId);
                if (target != null)
                {
                    target.Teamrank = 0;
                    target.TeamId = 0;
                }

                    var teamMemberData = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == memberId);
                if (teamMemberData == null) return;

                teamMemberData.Manage = false;
                teamMemberData.Bank = false;
                teamMemberData.Inventory = false;
                teamMemberData.Dienstnummer = 0;

                dbPlayer.TeamrankId = 0;
                dbPlayer.TeamId = 0;



                await db.SaveChangesAsync();

                await target.TriggerEventAsync("updateTeamId", 0);

                await target.SendNotify("Du wurdest von " + await player.GetNameAsync() + " aus der Fraktion geworfen!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
                await player.SendNotify("Du hast " + await target.GetNameAsync() + " aus der Fraktion geworfen!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
            }
            else
            {
                await player.SendNotify("Du besitzt dafür keine Berechtigung!");
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task InviteMemberToTeam(RXPlayer player, string targetName)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0 || player.Teamrank < 10) return;

            RXLogger.Print(targetName);
            var target = await PlayerController.FindPlayerByName(targetName);
            if (target == null) return;

            if (await NAPI.Task.RunReturnAsync(() => target.Position.DistanceTo(player.Position) > 20f))
            {
                await player.SendNotify("Der Spieler ist zu weit von dir entfernt!");
                return;
            }

            if (player.Team.GetMemberCount() >= player.Team.MaxMembers)
            {
                await player.SendNotify("Deine Fraktion hat die maximale Anzahl an Mitglieder erreicht! (" + player.Team.MaxMembers + " Mitglieder)");
                return;
            }

            if (target.TeamId != 0)
            {
                await player.SendNotify("Der Spieler ist bereits in einer Fraktion!");
                return;
            }

            await player.SendNotify("Du hast " + await target.GetNameAsync() + " eingeladen!");


            object confirmationBoxObject = new
            {
                t = "Du wurdest von " + await player.GetNameAsync() + " in " + player.Team.Name + " eingeladen. Möchtest du beitreten?",
                ft = "Ja",
                st = "Nein",
                fe = "addPlayerConfirmed",
                se = "Close",
                d = player.Team.Id,
                dd = await player.GetNameAsync()
            };

            var confirmation = new RXWindow("Confirm");

            await confirmation.OpenWindow(target, confirmationBoxObject);
        }
    }
}
