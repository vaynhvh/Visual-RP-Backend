using DSharpPlus.Entities;
using DSharpPlus;
using DSharpPlus.SlashCommands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Backend.MySql;
using Backend.MySql.Models;
using Microsoft.EntityFrameworkCore;
using GTANetworkAPI;
using Org.BouncyCastle.Asn1.X509;
using Backend.Models;
using Backend.Modules.Rank;
using Backend.Controllers;
using System.Linq;
using Backend.Modules.Phone.Apps;
using Backend.Utils;
using System.Reflection;
using System.Xml.Linq;
using System.Diagnostics.Metrics;
using Backend.Modules.Player;
using System.Security.Policy;
using Backend.Utils.Extensions;

namespace Backend.Modules.Discord
{
    class UprankModule : RXModule
    {
        public UprankModule() : base("UprankModule") { }
        /*
         public async override Task OnMinute()
         {

             using var db = new RXContext();

             var activepta = await db.PTASettings.FirstOrDefaultAsync(x => x.Active);

             if (activepta == null) return;

             var minutes = activepta.PTAEnd.Subtract(DateTime.Now).TotalMinutes;
             if (minutes == null) return;

             string staffstring = "**Upranks/Deranks (PTA Results):**\n";

             if (minutes < 1)
             {
                 await db.PTASettings.AddAsync(new DbPTASettings() { Active = true, PTAStart = DateTime.Now, PTAEnd = DateTime.Now.AddDays(7) });
                 activepta.Active = false;
                 db.PTASettings.Update(activepta);

                 var discord = await Resource.discord.GetGuildAsync(1097223311030751274);



                 var players = await db.Players.Where(x => x.RankId > 90 && x.DiscordID != "0").OrderBy(x => x.PTAPoints).Reverse().ToListAsync();
                 List<double> numbers = new List<double>()
 {
     92, 93, 94, 95, 96, 97, 98, 99, 100
 };
                 numbers.Sort();
                 foreach (var player in players)
                 {
                     var playerrank = RankModule.Ranks.FirstOrDefault(x => x.Permission == player.RankId);
                     if (playerrank == null) continue;

                     if (playerrank.UprankPoints == 0) continue;

                     var member = await discord.GetMemberAsync(ulong.Parse(player.DiscordID));

                     if (player.PTAPoints >= playerrank.UprankPoints)
                     {
                         double nextHighest = numbers.FirstOrDefault(x => x > playerrank.Permission);

                         var nextRank = RankModule.Ranks.Find(PermissionId => PermissionId.Permission == nextHighest);
                         if (nextRank == null || nextHighest == null) continue;

                         staffstring += "\n> " + player.Username + " - " + member.Mention + " | Uprank von " + playerrank.Name + " zu " + nextRank.Name;
                         await member.RevokeRoleAsync(discord.GetRole(playerrank.DiscordRole), "Uprank");
                         await member.GrantRoleAsync(discord.GetRole(nextRank.DiscordRole), "Uprank");
                         player.RankId = (uint)nextRank.Permission;
                         player.PTAPoints = 0;
                         player.PTA = 0;
                     }
                     else
                     {
                         if (player.PTAPoints <= playerrank.UprankPoints / 3)
                         {
                             player.PTAWarns++;
                             staffstring += "\n> " + player.Username + " - " + member.Mention + " | PTA Warn (Inaktivität)";

                             if (player.PTAWarns == 2)
                             {

                                 double nextLowest = numbers.LastOrDefault(x => x < (int)playerrank.Permission);
                                 var nextRank = RankModule.Ranks.Find(PermissionId => PermissionId.Permission == (int)nextLowest);

                                 if (nextRank == null || nextLowest == null)
                                 {
                                     staffstring += "\n> " + player.Username + " - " + member.Mention + " | Teamkick (Zu viele PTA Warns)";
                                     await member.RevokeRoleAsync(discord.GetRole(playerrank.DiscordRole), "Derank");
                                     await member.RevokeRoleAsync(discord.GetRole(1097223368136212672), "Derank");
                                 }
                                 else
                                 {
                                     staffstring += "\n> " + player.Username + " - " + member.Mention + " | Derank von " + playerrank.Name + " zu " + nextRank.Name;
                                     await member.RevokeRoleAsync(discord.GetRole(playerrank.DiscordRole), "Derank");
                                     await member.GrantRoleAsync(discord.GetRole(nextRank.DiscordRole), "Derank");
                                 }

                                 player.PTAPoints = 0;
                                 player.PTA = 0;

                                 player.RankId = (uint)nextRank.Permission;
                             }
                         }
                     }


                 }

                 await discord.GetChannel(1112144033284903002).SendMessageAsync(staffstring);


                 await db.SaveChangesAsync();
                 return;
             }



         }
              */
    }
    public class DiscordSlashCommands : ApplicationCommandModule
    {
        [SlashCommand("charinfo", "Hole dir die Informationen eines Chars.")]
        public async Task charinfo(InteractionContext ctx,
    [Option("playername", "Name des Charakters")] string pname = "Max_Mustermann")
        {

            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null) return;

            if (player.RankId > 50) {
                DbPlayer target = await db.Players.FirstOrDefaultAsync(x => x.Username == pname);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Cyan,
                    Description = "Hier sind Informationen über den Charakter von " + target.Username + "!",
                    Title = "Charakter-Informationen",
                };
                DbBankAccount bank = await db.BankAccounts.FirstOrDefaultAsync(x => x.Id == target.Id);

                embed.AddField("Bargeld", target.Cash + "$", true);
                embed.AddField("Bank", bank.Balance.ToString() + "$", true);
                embed.AddField("HP", target.HP.ToString(), false);
                embed.AddField("Armor", target.Armor.ToString(), false);
                embed.AddField("Teamrank", target.RankId.ToString(), true);
                embed.AddField("Warns", target.Warns.ToString(), true);
                embed.AddField("Ban-Ablaufsdatum", target.BanExpires.ToString(), false);

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed)) ;

            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Keine Rechte. Wie schade."));
            }

        }



        [SlashCommand("addwallpaper", "Füge ein Wallpáper hinzu")]
        public async Task addwallpaper(InteractionContext ctx,
   [Option("name", "Name des Wallpapers")] string pname = "Hier könnte ihre Werbung stehen",
           [Option("url", "Image URL")]
        string url = ".png", [Option("onlyteam", "Nur fürs team")]
        bool onlystaff = false)
        {

            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null) return;

            if (player.RankId > 50)
            {





                await db.PhoneWallpaper.AddAsync(new DbPhoneWallpaper() { Name = pname, Image = url, RestrictedForStaff = onlystaff, RestrictedPlayer = 0, RestrictedTeam = 0 });


                await db.SaveChangesAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Cyan,
                    Description = "Wallpaper wurde erfolgreich hinzugefügt.",
                    Title = "Wallpaper",
                };

                embed.AddField("Name", pname, true);
                embed.AddField("Url", url, true);


                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Keine Rechte. Wie schade."));
            }

        }

        [SlashCommand("addloginsong", "Füge einen Login-Song hinzu")]
        public async Task addloginsong(InteractionContext ctx, [Option("text", "Text der im Login gezeigt werden soll")] string pname = "Hier könnte ihre Werbung stehen",[Option("url", "MP3 URL")]
        string url = ".mp3")
        {

            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null) return;

            if (player.RankId > 50)
            {





                await db.LoginSongs.AddAsync(new DbLoadingscreenSongs() { Text = pname, Url = url});


                await db.SaveChangesAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Cyan,
                    Description = "Musik wurde erfolgreich hinzugefügt.",
                    Title = "Login Musik",
                };

                embed.AddField("Text", pname, true);
                embed.AddField("Url", url, true);


                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Keine Rechte. Wie schade."));
            }

        }

        public static List<string> random_spawns = new List<string>
        {
            "-1042,-2745,21",
        };

        /*[SlashCommand("createacc", "Erstellt einen User")]
        public async Task createacc(InteractionContext ctx, [Option("test", "test")] string username = "test", [Option("test2", "test2")] string discordid = "test2")
        {

            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null) return;

            if (player.RankId > 90)
            {
                await db.Players.AddAsync(new DbPlayer
                {
                    Username = username,
                    DiscordID = discordid,
                    Position = random_spawns[new Random().Next(random_spawns.Count)]
                });

                await db.SaveChangesAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Cyan,
                    Description = $"Der Spieler mit dem Namen: {username} und mit der Discord ID {discordid} wurde erfolgreich erstellt!",
                    Title = "Visual-RolePlay",
                };
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Keine Rechte. Wie schade."));
            }
        }*/

        [SlashCommand("restart", "Start den Server in einem gewissen Abständ neu.")]
        public async Task restart(InteractionContext ctx, [Option("Sekunden", "Wähle die Sekunden-Anzahl des Restarts")] string seconds = "15")
        {

            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null) return;

            if (player.RankId > 90)
            {



                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Cyan,
                Description = $"Server wird in {seconds} Sekunden neugestartet.",
                Title = "Visual-RolePlay",
            };
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

            RX.SendGlobalNotifyToAll("Der Server wird in " + seconds + " Sekunden neugestartet!", 10000, "darkred", Icon.Dev);
            await Task.Delay(int.Parse(seconds) * 1000);
            System.Environment.Exit(1);
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Keine Rechte. Wie schade."));
            }
        }

        [SlashCommand("pta", "Betrachte die aktuellen PTA Stats.")]
        public async Task pta(InteractionContext ctx)
        {

            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Dein Account konnte nicht gefunden werden."));
                return;
            }

            if (player.RankId < 1)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("U stupid?"));
                return;
            }
            var activepta = await db.PTASettings.FirstOrDefaultAsync(x => x.Active);
            var players = await db.Players.Where(x => x.RankId > 90 && x.DiscordID != "0").ToListAsync();
            string staffstring = "**Staff PTA:**\n ``` ";

            players = players.OrderBy(x => x.PTAPoints).Reverse().ToList();
            players = players.OrderBy(x => x.RankId).Reverse().ToList();

            foreach (var team in players)
            {
                try
                {
                    var dc = await ctx.Guild.GetMemberAsync(ulong.Parse(team.DiscordID));
                    if (dc == null)
                    {
                        RXLogger.Print(team.DiscordID + " nix gefunden!");
                        continue;
                    }

                    var rank = RankModule.Ranks.Find(x => x.Permission == team.RankId);
                    string rankname = "Unbekannt";
                    if (rank != null)
                    {
                        rankname = rank.Name;
                    }

                    staffstring += $"\n{team.Username} - {dc.Username} ({rankname}) | {team.PTAPoints} Punkte.";
                } catch (Exception e)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Der Rang von Discord-ID: " + team.DiscordID + " muss auf 0 gesetzt werden wegen Exception: " + e.Message));

                }
            }
            staffstring += "\n```\n**PTA Phase endet: " + activepta.PTAEnd.ToString("HH:mm | dd.MM.yyyy") + "**";
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(staffstring));
        }

        [SlashCommand("ptalogs", "Betrachte die aktuellen PTA Logs eines Teammitglieds.")]
        public async Task ptalogs(InteractionContext ctx, [Option("Teammitglied", "Wähle das Teammitglied")] DiscordUser user)
        {

            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Dein Account konnte nicht gefunden werden."));
                return;
            }

            if (player.RankId < 96)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("U stupid?"));
                return;
            }
            var activepta = await db.PTASettings.FirstOrDefaultAsync(x => x.Active);
            var target = await db.Players.FirstOrDefaultAsync(x => x.RankId > 1 && x.DiscordID == user.Id.ToString());

            DiscordEmbedBuilder embed = new DiscordEmbedBuilder
            {
                Color = DiscordColor.Cyan,
                Description = "PTA Logs über " + target.Username,
                Title = "Visual-RolePlay",
            };


            embed.AddField("PTA Stunden", target.PTA.ToString(), true);
            embed.AddField("PTA Punkte", target.PTAPoints.ToString(), true);

            var ptalogstring = "```\n";

            var targetlogs = await db.PTA.Where(x => x.TeamDiscord == target.DiscordID).ToListAsync();

            foreach (var log in targetlogs)
            {
                ptalogstring += "\nUsername: " + log.Username + " | User-DiscordID: " + log.UserDiscord + " | Begründung " + log.Reason + " | Punkteanzahl: " + log.Points + " | Datum: " + log.Date.ToString("HH:mm dd.MM.yyyy");
            }
            ptalogstring += "```";
            embed.AddField("PTA Logs", ptalogstring, false);


            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

        }

        [SlashCommand("verify", "Erhalte den Code zum erstellen deines Accounts.")]
        public async Task verify(InteractionContext ctx)
        {

            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player != null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du hast bereits einen Account!"));
                return;
            }

            if (LoginModule.DiscordCodes.ContainsKey(ctx.Member.Id.ToString()))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du hast bereits einen Code angefordert!"));

            }

            var logincode = LoginModule.RandomString(7);

            LoginModule.DiscordCodes.Add(ctx.Member.Id.ToString(), logincode);
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AsEphemeral(true).WithContent("Dein Code lautet: " + logincode));
        }

        [SlashCommand("commands", "Betrachte alle Commands.")]
        public async Task commands(InteractionContext ctx)
        {

            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Dein Account konnte nicht gefunden werden."));
                return;
            }

            if (player.RankId < 90)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("U stupid?"));
                return;
            }
            string staffstring = "**Alle Commands:**\n";

            IEnumerable<MethodInfo> commands = AppDomain.CurrentDomain.GetAssemblies()
   .SelectMany(x => x.GetTypes())
   .Where(x => x.IsClass)
   .SelectMany(x => x.GetMethods())
   .Where(x => x.GetCustomAttributes(typeof(RXCommand), false).FirstOrDefault() != null);

            foreach(var command in commands)
            {

                var cmd = ((RXCommand)command.GetCustomAttributes(typeof(RXCommand), false)[0]).Name.ToLower();
                staffstring += "\n> /" + cmd + " | " + ((RXCommand)command.GetCustomAttributes(typeof(RXCommand), false)[0]).Permission;

            }
            staffstring += "\n";
            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent(staffstring));
        }

        /*
                     IEnumerable<MethodInfo> commands = AppDomain.CurrentDomain.GetAssemblies()
                        .SelectMany(x => x.GetTypes())
                        .Where(x => x.IsClass)
                        .SelectMany(x => x.GetMethods())
                        .Where(x => x.GetCustomAttributes(typeof(RXCommand), false).FirstOrDefault() != null);

                */
        [SlashCommand("feedback", "Gebe einem Teammitglied Feedbeck für seine Arbeit.")]
        public async Task feedback(InteractionContext ctx, [Option("Teammitglied", "Wähle das Teammitglied")] DiscordUser user, [Option("Rating", "Gebe das Rating zwischen 0-5 an. Dabei ist 0 (sehr schlecht) und 5 (sehr gut)", true)] string rates = "5")
        {
            if (!uint.TryParse(rates, out uint rate)) return;

            using var db = new RXContext();

            if (!ctx.Channel.Name.Contains("ticket"))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du kannst nur in Tickets Feedback verteilen."));
                return;
            }

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du hast keinen Ingame Account."));
                return;
            }

            if (player.RankId > 1 && player.RankId < 97)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du kannst als Teammitglied kein Feedback verteilen."));
                return;
            }

            var target = await db.Players.FirstOrDefaultAsync(x => x.RankId > 1 && x.DiscordID == user.Id.ToString());

            if (target == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Das Teammitglied konnte nicht Ingame gefunden werden."));
                return;
            }

            var ptalogs = await db.PTA.Where(x => x.TeamDiscord == target.DiscordID && x.UserDiscord == player.DiscordID).ToListAsync();

            foreach (var log in ptalogs)
            {
                var hours = DateTime.Now.Subtract(log.Date).TotalHours;

                if (hours < 24)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du hast diesem Teammitglied bereits heute Feedback gegeben."));
                    return;
                }
            }

            switch (rate)
            {
                case 5:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = "Discord-Ticketbewertung mit " + rate + " Punkten!", Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                case 4:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = "Discord-Ticketbewertung mit " + rate + " Punkten!", Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                case 3:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = "Discord-Ticketbewertung mit " + rate + " Punkten!", Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                case 2:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = "Discord-Ticketbewertung mit " + rate + " Punkten!", Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                case 1:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = "Discord-Ticketbewertung mit " + rate + " Punkten!", Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                case 0:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = "Discord-Ticketbewertung mit " + rate + " Punkten!", Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                default:
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Solch ein Rating kannst du nicht vergeben!"));
                    break;
            }


            await db.SaveChangesAsync();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Feedback wurde erfolgreich gespeichert. Vielen Dank für deine Teilnahme!"));

        }
        [SlashCommand("lob", "Gebe einem Teammitglied Feedbeck für seine Arbeit.")]
        public async Task lob(InteractionContext ctx, [Option("Teammitglied", "Wähle das Teammitglied")] DiscordUser user, [Option("Begründung", "Gebe eine kurze Begründung an.", true)] string reason = "Kein Grund angegeben",[Option("Rating", "Gebe das Rating zwischen 0-5 an. Dabei ist 0 (sehr schlecht) und 5 (sehr gut)", true)] string rates = "5")
        {
            if (!uint.TryParse(rates, out uint rate)) return;

            using var db = new RXContext();

            if (ctx.Channel.Id != 1142934997527961691)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du kannst nur in dem Lob Channel Feedback verteilen."));
                return;
            }

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du hast keinen Ingame Account."));
                return;
            }

            if (player.RankId > 1 && player.RankId < 97)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du kannst als Teammitglied kein Feedback verteilen."));
                return;
            }

            var target = await db.Players.FirstOrDefaultAsync(x => x.RankId > 1 && x.DiscordID == user.Id.ToString());

            if (target == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Das Teammitglied konnte nicht Ingame gefunden werden."));
                return;
            }

            var ptalogs = await db.PTA.Where(x => x.TeamDiscord == target.DiscordID && x.UserDiscord == player.DiscordID).ToListAsync();

            foreach (var log in ptalogs)
            {
                var hours = DateTime.Now.Subtract(log.Date).TotalHours;

                if (hours < 24)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du hast diesem Teammitglied bereits heute Feedback gegeben."));
                    return;
                }
            }

            switch (rate)
            {
                case 5:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = reason, Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                case 4:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = reason, Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                case 3:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = reason, Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                case 2:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = reason, Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                case 1:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = reason, Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                case 0:
                    target.PTAPoints += (int)rate;
                    await db.PTA.AddAsync(new DbPTA() { Points = (uint)rate, UserDiscord = player.DiscordID, Username = player.Username, Reason = reason, Date = DateTime.Now, Teamname = target.Username, TeamDiscord = target.DiscordID });
                    break;
                default:
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Solch ein Rating kannst du nicht vergeben!"));
                    break;
            }


            await db.SaveChangesAsync();

            await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Lob wurde erfolgreich gespeichert. Vielen Dank für deine Teilnahme!"));

        }


        [SlashCommand("sync", "Sync deine Team-Rechte mit deinem Ingame-Account.")]
        public async Task sync(InteractionContext ctx)
        {

            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null)
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Dein Account konnte nicht gefunden werden."));
                return;
            }

            List<ulong> roles = new List<ulong>();
           
            foreach (DiscordRole r in ctx.Member.Roles)
            {
                roles.Add(r.Id);
            }

            if (roles.Contains(1139567815335084202))
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent("Du hast dafür keine Rechte."));
                return;
            }

            foreach (RXRank rank in RankModule.Ranks)
            {
                if (roles.Contains(rank.DiscordRole))
                {
                    player.RankId = (uint)rank.Permission; 
                    DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.IndianRed,
                        Description = $"Deinem Ingame-Account wurde der Rang " + rank.Name + " gegeben!",
                        Title = "Visual-RolePlay",
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));


                    var onlineplayer = await PlayerController.FindPlayerById(player.Id);

                    if (onlineplayer != null)
                    {
                        onlineplayer.Rank = rank;
                    }
                    await db.SaveChangesAsync();

                    break;
                }

            }


            
        }


        [SlashCommand("serverinfo", "Siehe die Server Informationen")]
        public async Task serverinfo(InteractionContext ctx)
        {

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Cyan,
                    Description = "Spieler-Online (" + NAPI.Pools.GetAllPlayers().Count + "/" + NAPI.Server.GetMaxPlayers() + ")",
                    Title = "Visual-RolePlay",
                };
            using var db = new RXContext();

            List<DbBankAccount> bank = await db.BankAccounts.ToListAsync();
            List<DbPlayer> players = await db.Players.ToListAsync();


            int bankMoney = 0;
            int barMoney = 0;
            foreach (DbPlayer b in players)
                barMoney += b.Cash;

            foreach (DbBankAccount b in bank)
                bankMoney += b.Balance;

            embed.AddField("Fahrzeuge ausgeparkt", NAPI.Pools.GetAllVehicles().Count.ToString(), true);
            embed.AddField("Geld im Umlauf (Bar)", barMoney.ToString() + "$", true);
            embed.AddField("Geld im Umlauf (Bank)", bankMoney.ToString() +"$", true);


                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));


        }

        [SlashCommand("resetpassword", "Setzt das Passwort eines Spielers zurück")]
        public async Task resetpassword(InteractionContext ctx, [Option("user", "Account des Charakters")] DiscordUser user,
            [Option("playername", "Name des Charakters")] string playername = "Max_Mustermann")
        {
            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null) return;

            if (player.RankId > 50)
            {
                DbPlayer dbTarget = await db.Players.FirstOrDefaultAsync(x => x.Username == playername);

                RXPlayer target = await PlayerController.FindPlayerByName(playername, false);

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Cyan,
                    Description = $"Ein Charakter mit dem Namen: {playername} wurde nicht gefunden!",
                    Title = "Passwort Reset",
                };

                if (dbTarget == null)
                {
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    return;
                }

                if (dbTarget.DiscordID != user.Id.ToString())
                {
                    embed = new DiscordEmbedBuilder
                    {
                        Color = DiscordColor.Cyan,
                        Description = $"Dem Discord Account: {user.Username} gehört der Charakter mit dem Namen: {playername} nicht (Besitzer: <@{dbTarget.DiscordID}>)",
                        Title = "Passwort Reset",
                    };
                    await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                    return;
                }

                if (target != null)
                {
                    if (target.IsLoggedIn)
                    {
                        embed = new DiscordEmbedBuilder
                        {
                            Color = DiscordColor.Cyan,
                            Description = $"Der Charakter mit dem Namen: {playername} ist auf dem Server und ist eingeloggt!",
                            Title = "Passwort Reset",
                        };
                        await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
                        return;
                    } else
                    {
                        dbTarget.password = "";
                        await target.TriggerEventAsync("closeWindow", "Login");
                        object confirmationBoxObject = new
                        {
                            t = "Gebe nun ein Passwort ein!",
                            e = "ValidatePassword",
                            c = "RetryLogin"
                        };

                        var confirmation = new RXWindow("Input");

                        await confirmation.OpenWindow(target, confirmationBoxObject);
                    }
                }

                dbTarget.password = "";

                await db.SaveChangesAsync();

                embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Cyan,
                    Description = $"Das Passwort von dem Charakter: {playername} wurde erfolgreich zurückgesetzt!",
                    Title = "Passwort Reset",
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));

            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Keine Rechte. Wie schade."));
            }
        }

        [SlashCommand("createchar", "Erstellt einen Charakter")]
        public async Task createchar(InteractionContext ctx, [Option("playername", "Name des Charakters")] string playername = "Max_Mustermann",
            [Option("isMale", "1 für Mann 0 für Frau")] string isMale = "1")
        {
            using var db = new RXContext();

            DbPlayer player = await db.Players.FirstOrDefaultAsync(x => x.DiscordID == ctx.Member.Id.ToString());

            if (player == null) return;

            if (player.RankId >= 99)
            {
                DbPlayer dbPlayer = new DbPlayer
                {
                    Id = await db.Players.CountAsync() == 0 ? 1 : (await db.Players.MaxAsync(con => con.Id) + 1),
                    Username = playername,
                    IsMale = isMale == "1" ? 1 : 0,
                    ClientHash = "",
                    DiscordID = "0",
                    password = "",
                    Thirst = 100,
                    Hunger = 100,
                    Paytime = 0,
                    WalletAdress = RX.GenerateRandomHexString(22)
                };

                await db.Players.AddAsync(dbPlayer);
                await db.SaveChangesAsync();

                DiscordEmbedBuilder embed = new DiscordEmbedBuilder
                {
                    Color = DiscordColor.Cyan,
                    Description = $"Es wurde Erfolgreich ein Account mit dem Namen: {playername}, Erstellt",
                    Title = $"{playername}",
                };

                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().AddEmbed(embed));
            }
            else
            {
                await ctx.CreateResponseAsync(InteractionResponseType.ChannelMessageWithSource, new DiscordInteractionResponseBuilder().WithContent($"Keine Rechte. Wie schade."));
            }
        }

    }
}
