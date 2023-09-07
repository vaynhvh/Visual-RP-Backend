using Backend.Controllers;
using Backend.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Tablet.Apps
{

    public class TicketData
    {
        [JsonProperty("a")]
        public int Aduty { get; set; }

        [JsonProperty("d")]
        public List<Ticket> d { get; set; }

        [JsonProperty("dd")]
        public List<AcceptedTicket> dd { get; set; }
    }

    public class Ticket
    {
        [JsonProperty("i")]
        public uint Id { get; set; }

        [JsonProperty("n")]
        public string Creator { get; set; }

        [JsonProperty("m")]
        public string Text { get; set; }

        [JsonProperty("d")]
        public DateTime Created { get; set; }
    }

    public class AcceptedTicket
    {
        [JsonProperty("i")]
        public uint Id { get; set; }

        [JsonProperty("n")]
        public string Creator { get; set; }

        [JsonProperty("m")]
        public string Text { get; set; }

        [JsonProperty("admin")]
        public string Admin { get; set; }

        [JsonProperty("d")]
        public DateTime Created { get; set; }

        public bool ChatStatus { get; set; }
    }

    class SupportApp : RXModule
    {
        public SupportApp() : base("SupportApp") { }

        public static List<Ticket> Tickets = new List<Ticket>();
        public static List<AcceptedTicket> AcceptedTickets = new List<AcceptedTicket>();
        public static Dictionary<uint, uint> TicketRate = new Dictionary<uint, uint>();

        [RemoteEvent]
        public async Task requestOpenSupportTickets(RXPlayer player)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            var supportOpenTickets = new RXWindow("Support");


            string pname = await player.GetNameAsync();


            TicketData data = new TicketData() { Aduty = player.InAduty ? 1 : 0, d = Tickets, dd = AcceptedTickets.FindAll(x => x.Admin == pname) };

            await supportOpenTickets.OpenWindow(player, data);

        }

        public override async Task OnPlayerDisconnect(RXPlayer player, DisconnectionType type, string reason)
        {
            string playerName = await player.GetNameAsync();
            TicketRate.Remove(player.Id);
            Tickets.RemoveAll(x => x.Creator == playerName || x.Id == player.Id);
            AcceptedTickets.RemoveAll(x => x.Creator == playerName || x.Id == player.Id);
            return;
        }

        [RemoteEvent]
        public async Task AssignTicket(RXPlayer player, string playerTicket)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            var target = await PlayerController.FindPlayerByStartsName(playerTicket, true);
            if (target == null)
            {
                await player.SendNotify("Da der Spieler offline ist, wurde das Ticket geschlossen.", 3500);
                Tickets.RemoveAll(x => x.Creator == playerTicket);
                return;
            }

            await target.SendNotify("Dein Ticket wurde angenommen.", 3500);

            var ticket = Tickets.FirstOrDefault(x => x.Creator == playerTicket);
            if (ticket == null) return;

            var acceptedTicket = new AcceptedTicket
            {
                Id = ticket.Id,
                Creator = ticket.Creator,
                Text = ticket.Text,
                Admin = await player.GetNameAsync(),
                Created = ticket.Created
            };

            await player.SendNotify("Ticket angenommen.", 3500);

            if (!Tickets.Contains(ticket)) return;

            Tickets.Remove(ticket);
            AcceptedTickets.Add(acceptedTicket);

            await player.TriggerEventAsync("AddAssignedTicket", NAPI.Util.ToJson(acceptedTicket));

        }

        [RemoteEvent]
        public async Task GetAssignedTickets(RXPlayer player)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            string pname = await player.GetNameAsync();

            await player.TriggerEventAsync("SendAssignedTickets", NAPI.Util.ToJson(AcceptedTickets.FindAll(x => x.Admin == pname)));
        }

        [RemoteEvent]
        public static async Task ToggleAduty(RXPlayer player)
        {

            if (player.Rank.Permission < 10) return;

            await player.TriggerEventAsync("setPlayerAduty", !player.InAduty);
            await player.TriggerEventAsync("updateAduty", !player.InAduty);

            player.InAduty = !player.InAduty;

            if (!await player.GetIsInVehicleAsync()) await player.SpawnAsync(await player.GetPositionAsync() + new Vector3(0, 0, 0.52f));

            if (player.InAduty)
            {
                player.Invincible = true;

                await player.SendNotify("Du hast den Adminmodus betreten!", 3500, "red", "Administration");

                int id = player.Rank.ClothesId;

                await player.SetClothesAsync(1, 135, id);
                await player.SetClothesAsync(11, 287, id);
                await player.SetClothesAsync(8, 15, 0);
                await player.SetClothesAsync(3, 3, 0);
                await player.SetClothesAsync(4, 114, id);
                await player.SetClothesAsync(6, 78, id);
                await player.SetClothesAsync(5, 0, 0);
                await player.SetClothesAsync(2, 0, 0);
                await player.SetAccessoriesAsync(0, -1, 0);

                await player.RemoveAllWeaponsAsync();
            }
            else
            {
                player.Invincible = false;

                await player.SendNotify("Du hast den Adminmodus verlassen!", 3500, "red", "Administration");
                await player.LoadCharacter();
            }
        }

        [RemoteEvent]
        public async Task GoToTicket(RXPlayer player, string playerTicket)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            var target = await PlayerController.FindPlayerByStartsName(playerTicket, true);
            if (target == null)
            {
                await player.SendNotify("Da der Spieler offline ist, wurde das Ticket geschlossen.", 3500);
                AcceptedTickets.RemoveAll(x => x.Creator == playerTicket);
                return;
            }

            await player.SetDimensionAsync(await target.GetDimensionAsync());
            await player.SetPositionAsync(await target.GetPositionAsync());

            await player.SendNotify("Du hast dich zu " + playerTicket + " teleportiert.", 3500, "red", "Administration");
            await target.SendNotify(player.Rank.Name + " " + await player.GetNameAsync() + " hat sich zu dir teleportiert.", 3500, "red", "Administration");
        }

        [RemoteEvent]
        public async Task SpectateToTicket(RXPlayer player, string playerTicket)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            if (player.IsSpectate)
            {
                await StopSpectate(player);
                return;
            }

            var target = await PlayerController.FindPlayerByStartsName(playerTicket, true);
            if (target == null)
            {
                await player.SendNotify("Da der Spieler offline ist, wurde das Ticket geschlossen.", 3500);
                AcceptedTickets.RemoveAll(x => x.Creator == playerTicket);
                return;
            }

            if (playerTicket == await player.GetNameAsync())
            {
                await player.SendNotify("Du kannst dich aufgrund eines Attach-Bugs nicht selbst specateten.");
                return;
            }

            await player.SendNotify("Du spectatest nun " + playerTicket + ".", 3500, "red", "Administration");

            player.Freezed = true;
            player.Invisible = true;
            player.Collision = false;
            player.IsSpectate = true;

            Vector3 enter = await player.GetPositionAsync();

            await NAPI.Task.RunAsync(() => { player.SetData("SpecEnterPos", enter); });


            await player.SetPositionAsync(await target.GetPositionAsync());

            await player.TriggerEventAsync("SpectatePlayer", target);

            await player.TriggerEventAsync("freezePlayer", true);
        }
        [RemoteEvent]
        public static async Task StopSpectate(RXPlayer player)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            await player.SendNotify("Du spectatest nun niemanden mehr.", 3500, "red", "Administration");

            await player.TriggerEventAsync("StopSpectatePlayer");

            Vector3 enter = await NAPI.Task.RunReturnAsync(() => { return player.GetData<Vector3>("SpecEnterPos"); });

            await player.SetPositionAsync(enter);

            player.Freezed = false;
            player.Invisible = false;
            player.IsSpectate = false;
            player.Collision = true;

        }

        [RemoteEvent]
        public async Task GoToPlayer(RXPlayer player, string playerTicket)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            var target = await PlayerController.FindPlayerByStartsName(playerTicket, true);
            if (target == null)
            {
                await player.SendNotify("No. Spieler ist offline", 3500);
                return;
            }

            await player.SetDimensionAsync(await target.GetDimensionAsync());
            await player.SetPositionAsync(await target.GetPositionAsync());

            await player.SendNotify("Du hast dich zu " + playerTicket + " teleportiert.", 3500, "red", "Administration");
            await target.SendNotify(player.Rank.Name + " " + await player.GetNameAsync() + " hat sich zu dir teleportiert.", 3500, "red", "Administration");
        }

        [RemoteEvent]
        public async Task SpectatePlayer(RXPlayer player, string playerTicket)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            if (player.IsSpectate)
            {
                await StopSpectate(player);
                return;
            }

            var target = await PlayerController.FindPlayerByStartsName(playerTicket, true);
            if (target == null)
            {
                await player.SendNotify("No. Spieler ist offline", 3500);

                return;
            }

            if (playerTicket == await player.GetNameAsync())
            {
                await player.SendNotify("Du kannst dich aufgrund eines Attach-Bugs nicht selbst specateten.");
                return;
            }

            await player.SendNotify("Du spectatest nun " + playerTicket + ".", 3500, "red", "Administration");

            player.Freezed = true;
            player.Invisible = true;
            player.Collision = false;
            player.IsSpectate = true;

            Vector3 enter = await player.GetPositionAsync();

            await NAPI.Task.RunAsync(() => { player.SetData("SpecEnterPos", enter); });


            await player.SetPositionAsync(await target.GetPositionAsync());

            await player.TriggerEventAsync("SpectatePlayer", target);

            await player.TriggerEventAsync("freezePlayer", true);
        }

        [RXCommand("ticketrate")]
        public async Task ticketrate(RXPlayer player, string[] args)
        {

            if (TicketRate.ContainsKey(player.Id))
            {
                if (!uint.TryParse(args[0], out uint rate)) return;

                var target = await PlayerController.FindPlayerById(TicketRate.GetValueOrDefault(player.Id));

                if (target == null)
                {
                    await player.SendNotify("Vielen Dank für dein Rating!");
                    TicketRate.Remove(player.Id);
                    return;
                }

                switch(rate)
                {
                    case 5:
                        await player.SendNotify("Vielen Dank für dein Rating!");
                        await target.GivePTAPoints(await player.GetNameAsync(), player.DiscordID, $"Ticketbewertung mit {rate} Punkten", rate);
                        break;
                    case 4:
                        await player.SendNotify("Vielen Dank für dein Rating!");
                        await target.GivePTAPoints(await player.GetNameAsync(), player.DiscordID, $"Ticketbewertung mit {rate} Punkten", rate);
                        break;
                    case 3:
                        await player.SendNotify("Vielen Dank für dein Rating!");
                        await target.GivePTAPoints(await player.GetNameAsync(), player.DiscordID, $"Ticketbewertung mit {rate} Punkten", rate);
                        break;
                    case 2:
                        await player.SendNotify("Vielen Dank für dein Rating!");
                        await target.GivePTAPoints(await player.GetNameAsync(), player.DiscordID, $"Ticketbewertung mit {rate} Punkten", rate);
                        break;
                    case 1:
                        await player.SendNotify("Vielen Dank für dein Rating!");
                        await target.GivePTAPoints(await player.GetNameAsync(), player.DiscordID, $"Ticketbewertung mit {rate} Punkten", rate);
                        break;
                    case 0:
                        await player.SendNotify("Vielen Dank für dein Rating!");
                        await target.GivePTAPoints(await player.GetNameAsync(), player.DiscordID, $"Negative Ticketbewertung mit {rate} Punkten", rate);
                        break;
                    default:
                        await player.SendNotify("So eine Bewertung kannst du nicht vergeben!");
                        break;
                }

                TicketRate.Remove(player.Id);

            }
            else
            {
                await player.SendNotify("Du kannst derzeit kein Ticket bewerten!");
            }

        }

        [RemoteEvent]
        public async Task DeleteTicket(RXPlayer player, string playerTicket)
        {
            if (!player.CanInteract() || player.Rank.Permission < 10) return;

            await player.SendNotify("Ticket geschlossen.", 3500);

            var target = await PlayerController.FindPlayerByStartsName(playerTicket, true);
            if (target != null) await target.SendNotify("Dein Ticket wurde geschlossen. Bitte bewerte nun den Ticket-Support mit dem Command /ticketrate 0-5 (Dabei ist 0 für schlecht und 5 für sehr gut)", 15000);


            if (target != null)
            {
                if (TicketRate.ContainsKey(target.Id))
                {
                    TicketRate.Remove(target.Id);
                }

                TicketRate.Add(target.Id, player.Id);
            }


            var acceptedTicket = AcceptedTickets.FirstOrDefault(x => x.Creator == playerTicket);
            if (acceptedTicket != null) AcceptedTickets.Remove(acceptedTicket);


            var acceptedTickeet = Tickets.FirstOrDefault(x => x.Creator == playerTicket);
            if (acceptedTickeet != null) Tickets.Remove(acceptedTickeet);

            SupportConversation.RemoveConversation(target);
        }

        public static async Task<AcceptedTicket> GetAcceptedTicketByOwner(RXPlayer player)
        {
            string playerName = await player.GetNameAsync();

            return AcceptedTickets.FirstOrDefault(x => x.Creator == playerName);
        }

        public static async Task<Ticket> GetOpenTicketByOwner(RXPlayer player)
        {
            string playerName = await player.GetNameAsync();

            return Tickets.FirstOrDefault(x => x.Creator == playerName);
        }

        public static async Task<bool> ChangeChatStatus(RXPlayer player, bool status)
        {
            var createdTicket = await GetAcceptedTicketByOwner(player);
            if (createdTicket == null) return false;

            createdTicket.ChatStatus = status;
            return true;
        }

        public static async Task<bool> getCurrentChatStatus(RXPlayer player)
        {
            var createdTicket = await GetAcceptedTicketByOwner(player);
            if (createdTicket == null) return false;

            bool status = createdTicket.ChatStatus;
            return status;
        }
    }
}
