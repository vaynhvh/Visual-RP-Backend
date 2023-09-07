using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Phone;
using Backend.MySql;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Laptop.Apps
{
    public class FraktionMember
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "rang")]
        public uint Rank { get; set; }

        [JsonProperty(PropertyName = "rights_storage")]
        public bool Storage { get; set; }

        [JsonProperty(PropertyName = "rights_bank")]
        public bool Bank { get; set; }

        [JsonProperty(PropertyName = "rights_manage")]
        public bool Manage { get; set; }
    }

    class FraktionApp : RXModule
    {
        public FraktionApp() : base("FraktionApp", new RXWindow("FraktionListApp")) { }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task requestFraktionMembers(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

            using var db = new RXContext();

            List<FraktionMember> fraktionMembers = new List<FraktionMember>();

            var list = await db.Players.Where(x => x.TeamId == player.TeamId).ToListAsync();
            if (list == null || list.Count < 1) return;

            foreach (var member in list)
            {
                var teamMemberData = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == member.Id);
                if (teamMemberData == null) continue;

                fraktionMembers.Add(new FraktionMember
                {
                    Id = member.Id,
                    Name = member.Username,
                    Rank = member.TeamrankId,
                    Title = teamMemberData.Title,
                    Bank = teamMemberData.Bank,
                    Manage = teamMemberData.Manage,
                    Storage = teamMemberData.Inventory
                });
            }

            fraktionMembers = fraktionMembers.OrderByDescending(t => t.Rank).ToList();

            await this.Window.TriggerEvent(player, "responseMembers", JsonConvert.SerializeObject(new { manage = (player.Teamrank == 12 || (player.TeamMemberData != null && player.TeamMemberData.Manage)), list = fraktionMembers }));
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task editFraktionMember(RXPlayer player, uint memberId, string rankStr, string title)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

            if (!uint.TryParse(rankStr, out var rank)) return;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == memberId);
            if (dbPlayer == null) return;

            var teamMemberData = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == memberId);
            if (teamMemberData == null) return;

            if (dbPlayer.TeamId > 0 && player.Team != null && dbPlayer.TeamId == player.Team.Id && ((player.Teamrank > dbPlayer.TeamrankId && player.Teamrank > 9 && rank < player.Teamrank) || (player.TeamMemberData != null && player.TeamMemberData.Manage && teamMemberData != null && !teamMemberData.Manage && dbPlayer.TeamrankId < 10) || (dbPlayer.Id == player.Id && (teamMemberData.Manage || dbPlayer.TeamrankId == 12))))
            {
                teamMemberData.Title = title;

                if (dbPlayer.TeamId > 0 && player.Team != null && dbPlayer.TeamId == player.Team.Id && ((player.Teamrank > dbPlayer.TeamrankId && player.Teamrank > 9 && rank < player.Teamrank) || (player.TeamMemberData != null && player.TeamMemberData.Manage && teamMemberData != null && !teamMemberData.Manage && dbPlayer.TeamrankId < 10)))
                {
                    dbPlayer.TeamrankId = rank;

                    var target = await PlayerController.FindPlayerById(memberId);
                    if (target != null) target.Teamrank = rank;
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
        public async Task kickFraktionMember(RXPlayer player, uint memberId, string rank)
        {
            if (!player.IsLoggedIn || player.Team == null || player.TeamId == 0) return;

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == memberId);
            if (dbPlayer == null) return;

            var teamMemberData = await db.TeamMemberDatas.FirstOrDefaultAsync(x => x.PlayerId == memberId);
            if (teamMemberData == null) return;

            if (dbPlayer.TeamId > 0 && player.Team != null && dbPlayer.TeamId == player.Team.Id && ((player.Teamrank > dbPlayer.TeamrankId && player.Teamrank > 9) || (player.TeamMemberData != null && player.TeamMemberData.Manage && teamMemberData != null && !teamMemberData.Manage && dbPlayer.TeamrankId < 10)))
            {
                teamMemberData.Title = "";
                teamMemberData.Manage = false;
                teamMemberData.Bank = false;
                teamMemberData.Inventory = false;

                dbPlayer.TeamrankId = 0;
                dbPlayer.TeamId = 0;

                await db.SaveChangesAsync();

                var target = await PlayerController.FindPlayerById(memberId);
                if (target != null)
                {
                    target.Teamrank = 0;
                    target.TeamId = 0;

                    await PhoneModule.requestApps(target);
                    await target.TriggerEventAsync("updateTeamId", 0);

                    await target.SendNotify("Du wurdest von " + await player.GetNameAsync() + " aus der Fraktion geworfen!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
                }

                await player.SendNotify("Du hast " + await target.GetNameAsync() + " aus der Fraktion geworfen!", 3500, player.Team.RGB.ConvertHTML(), player.Team.Name);
            }
            else
            {
                await player.SendNotify("Du besitzt dafür keine Berechtigung!");
            }
        }
    }
}
