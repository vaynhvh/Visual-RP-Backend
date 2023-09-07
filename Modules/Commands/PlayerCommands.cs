using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Admin.Tablet;
using Backend.Modules.Discord;
using Backend.Modules.Faction;
using Backend.Modules.Gangwar;
using Backend.Modules.Leitstellen;
using Backend.Modules.Scenarios;
using Backend.Modules.Tablet.Apps;
using Backend.MySql;
using Backend.MySql.Models;
using GTANetworkAPI;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Utilities.IO;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Commands
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class PlayerCommands : RXModule
    {
        public PlayerCommands() : base("PlayerCommands") { }

        //[HandleExceptions]
        [RXCommand("onlist")]
        public async Task onlist(RXPlayer player, string[] args)
        {
            await player.SendNotify("Aktive Spieler: " + PlayerController.GetPlayers().Count + " - Eingeloggt: " + PlayerController.GetValidPlayers().Count);
        }


        [RXCommand("givelic")]
        public async Task givelic(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }
            using var db = new RXContext();

            if (player.Team.Type == TeamType.DMV)
            {
                if (player.Teamrank < 3) return;
               
                switch (args[1])
                {
                    case "car":
                        await db.PlayerLicenses.AddAsync(new DbPlayerLicenses() { DateOfSign = DateTime.Now, LicenseId = 12, PlayerId = target.Id, SignerId = player.Id });
                        await player.SendNotify("Lizenz wurde ausgestellt!");
                        await target.SendNotify("Glückwunsch, Sie haben eine Lizenz erhalten!");
                        break;
                    case "truck":
                        await db.PlayerLicenses.AddAsync(new DbPlayerLicenses() { DateOfSign = DateTime.Now, LicenseId = 13, PlayerId = target.Id, SignerId = player.Id });
                        await player.SendNotify("Lizenz wurde ausgestellt!");
                        await target.SendNotify("Glückwunsch, Sie haben eine Lizenz erhalten!");
                        break;
                    case "motorcycle":
                        await db.PlayerLicenses.AddAsync(new DbPlayerLicenses() { DateOfSign = DateTime.Now, LicenseId = 14, PlayerId = target.Id, SignerId = player.Id });
                        await player.SendNotify("Lizenz wurde ausgestellt!");
                        await target.SendNotify("Glückwunsch, Sie haben eine Lizenz erhalten!");
                        break;
                    case "helicopter":
                        await db.PlayerLicenses.AddAsync(new DbPlayerLicenses() { DateOfSign = DateTime.Now, LicenseId = 16, PlayerId = target.Id, SignerId = player.Id });
                        await player.SendNotify("Lizenz wurde ausgestellt!");
                        await target.SendNotify("Glückwunsch, Sie haben eine Lizenz erhalten!");
                        break;
                    case "boat":
                        await db.PlayerLicenses.AddAsync(new DbPlayerLicenses() { DateOfSign = DateTime.Now, LicenseId = 17, PlayerId = target.Id, SignerId = player.Id });
                        await player.SendNotify("Lizenz wurde ausgestellt!");
                        await target.SendNotify("Glückwunsch, Sie haben eine Lizenz erhalten!");
                        break;
                }
            }

            if (player.Team.Type == TeamType.Medic)
            {
                if (player.Teamrank < 8) return;

                switch (args[1])
                {
                    case "erstehilfe":
                        await db.PlayerLicenses.AddAsync(new DbPlayerLicenses() { DateOfSign = DateTime.Now, LicenseId = 3, PlayerId = target.Id, SignerId = player.Id });
                        await player.SendNotify("Lizenz wurde ausgestellt!");
                        break;
                }
            }

            await db.SaveChangesAsync();

        }


        [RXCommand("report")]
        public async Task report(RXPlayer player, string[] args)
        {
            if (string.IsNullOrEmpty(player.LastKiller))
            {
                await player.SendNotify("Du wurdest von niemanden getötet!");
                return;
            }
            string message = string.Join(" ", args);

            var target = await PlayerController.FindPlayerByName(player.LastKiller);
            string kills = "Keine Kills gefunden!";
            if (target != null)
            {
                kills = string.Join(",", target.PlayerKills.ToArray());
            }


            RX.SendNotifyToAllWhich(x => x.Rank != null && x.Rank.Permission > 80, player.Rank.Name + " " + await player.GetNameAsync() + " hat einen Report abgesendet! (GETÖTET VON: " + player.LastKiller + ") | Grund: " + message + " | Player Kills: " + kills, 8000, "darkred", "WICHTIG");

            DiscordModule.Logs.Add(new DiscordLog("SPIELER REPORT", player.Rank.Name + " " + await player.GetNameAsync() + " hat einen Report abgesendet! (GETÖTET VON: " + player.LastKiller + ") | Grund: " + message + " | Player Kills: " + kills, "https://canary.discord.com/api/webhooks/1142047699223445575/00uZqse_Y7iYhum73oFroXO9X8mehH1RQkFqVSK-xRXo2nwJWXW8-cfxLY2rCK0hOI3f"));
        }

        [RXCommand("lr")]
        public async Task letzterritt(RXPlayer player, string[] args)
        {
            if (string.IsNullOrEmpty(player.LastKiller))
            {
                await player.SendNotify("Du wurdest von niemanden getötet!");
                return;
            }


            var target = await PlayerController.FindPlayerByName(player.LastKiller);
            string kills = "Keine Kills gefunden!";
            if (target != null)
            {
                kills = string.Join(",", target.PlayerKills.ToArray());
            }


            int announce = 0;

            for (announce = 0; announce < 5; announce++)
            {
                RX.SendNotifyToAllWhich(x => x.Rank != null && x.Rank.Permission > 80, player.Rank.Name + " " + await player.GetNameAsync() + " meldet einen letzten Ritt! (GETÖTET VON: " + player.LastKiller + ") | Player Kills: " + kills, 8000, "darkred", "WICHTIG");
            }

            DiscordModule.Logs.Add(new DiscordLog("LETZTER RITT REPORT", player.Rank.Name + " " + await player.GetNameAsync() + " meldet einen letzten Ritt! (GETÖTET VON: " + player.LastKiller + ") | Player Kills: " + kills, "https://canary.discord.com/api/webhooks/1142047699223445575/00uZqse_Y7iYhum73oFroXO9X8mehH1RQkFqVSK-xRXo2nwJWXW8-cfxLY2rCK0hOI3f"));
        }

        //[HandleExceptions]
        [RXCommand("ooc")]
        public async Task ooc(RXPlayer player, string[] args)
        {
            if (!await player.CanInteractAntiFloodNoMSG(2)) return;

            string message = string.Join(" ", args);

            if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message)) return;

            if (message.ToLower().Contains("discord") || message.Contains("1337") && message.Contains("$") || message.ToLower().Contains("twitch") || message.ToLower().Contains("youtube") || message.ToLower().Contains("dc.gg") || message.ToLower().Contains(".com") || message.ToLower().Contains(".gg") || message.ToLower().Contains(".de") || message.ToLower().Contains("http") || message.ToLower().Contains("nigga") || message.ToLower().Contains("nogger") || message.ToLower().Contains("niger") || message.ToLower().Contains("schwanzello") || message.ToLower().Contains("schwanzcello") || message.ToLower().Contains("fdt")) return;

            var surroundingUsers = await PlayerController.GetPlayersInRange(await player.GetPositionAsync(), 15.0f);

            foreach (RXPlayer user in surroundingUsers)
            {
                if (user == null || !user.IsLoggedIn) continue;

                if (await user.GetDimensionAsync() == await player.GetDimensionAsync())
                {
                    await user.SendNotify(message, 5000, "DarkGreen", $"OOC - ({ await player.GetNameAsync() })");
                }
            }
        }

        [RXCommand("leitstelle")]
        public async Task leitstelle(RXPlayer player, string[] args)
        {
            if (player == null) return;

            uint teamId = player.TeamId;

            if (!LeitstellenModule.hasLeitstelleFunction(teamId))
            {
                await player.SendNotify("Deine Fraktion hat keine Leistelle!");
                return;
            }


            TeamLeitstellenObject teamLeitstellenObject = LeitstellenModule.GetLeitstelle(teamId);
            if (teamLeitstellenObject == null) return;

            if (teamLeitstellenObject.Acceptor != null)
            {
                if (teamLeitstellenObject.Acceptor.Id == player.Id)
                {
                    teamLeitstellenObject.Acceptor = null;
                    await player.SendNotify("Du hast die Einsatzleitung beendet!");
                    await player.TriggerEventAsync("clearcustommarks", CustomMarkersKeys.Leitstelle);
                    return;
                }
            }

            teamLeitstellenObject.Acceptor = player;

            player.Team.SendNotification($"{await player.GetNameAsync()} ist nun Einsatzleiter deiner Fraktion.", 5000, 0, "Info");
            await player.SendNotify("Du hast nun die Einsatzleitung übernommen!");
            return;
        }

        [RXCommand("support")]
        public async Task support(RXPlayer player, string[] args)
        {
            if (!await player.CanInteractAntiFloodNoMSG(2)) return;

            string playerName = await player.GetNameAsync();


            if (SupportApp.Tickets.FirstOrDefault(x => x.Creator == playerName || x.Id == player.Id) != null)
            {
                SupportApp.Tickets.RemoveAll(x => x.Creator == playerName || x.Id == player.Id);

                await player.SendNotify("Dein Ticket wurde geschlossen.", 3500, "red");
                return;
            }

            if (String.Join(" ", args).Length > 100)
            {
                await player.SendNotify("Grund ist zu lang. Max. 100 Zeichen", 3500);
                return;
            }

            var ticket = new Ticket
            {
                Id = player.Id,
                Created = DateTime.Now,
                Creator = playerName,
                Text = String.Join(" ", args)
            };

            SupportApp.Tickets.Add(ticket);

            await player.SendNotify("Deine Supportanfrage wurde eingereicht. Bitte hab einen kleinen Augenblick Geduld ;)", 5000, "darkred", "SUPPORT");
            await player.SendNotify("Wenn sich deine Anfrage erledigt hat, kannst du sie mit (/support) beenden", 5000, "darkred", "SUPPORT");

            RX.SendNotifyToAllWhich(x => x.InAduty, playerName + ": " + String.Join(" ", args), 10000, "darkred", "Neues Ticket");
        }

        [RXCommand("chat")]
        public async Task chat(RXPlayer player, string[] args)
        {
            if (!await player.CanInteractAntiFloodNoMSG(1)) return;

            string message = String.Join(' ', args);
            string playerName = await player.GetNameAsync();

            var acceptedTicket = SupportApp.AcceptedTickets.FirstOrDefault(x => x.Creator == playerName);
            if (acceptedTicket == null)
            {
                await player.SendNotify("Du kannst diesen Befehl aktuell nicht benutzen.", 3500);
                return;
            }

            if (!acceptedTicket.ChatStatus)
            {
                await player.SendNotify("Du kannst diesen Befehl aktuell nicht benutzen.", 3500);
                return;
            }

            string name = acceptedTicket.Admin;

            var target = await PlayerController.FindPlayerByStartsName(name, true);
            if (target == null) return;

            Conversation ConversationMessage = new Conversation(player, false, message);
            SupportConversation.AddConversationMessage(player, ConversationMessage);

            var ConversationClientMessage = new ConversationObject { Id = ConversationMessage.Player.Id, Sender = await ConversationMessage.Player.GetNameAsync(), Receiver = ConversationMessage.Receiver, Message = ConversationMessage.Message, Date = ConversationMessage.Created_at };

            await SupportConversation.SendMessage(target, JsonConvert.SerializeObject(ConversationClientMessage));

            await target.SendNotify("Antwort von " + await player.GetNameAsync() + " erhalten.");
            await player.SendNotify("Die Antwort wurde an " + await target.GetNameAsync() + " gesendet.");
        }
    }
}
