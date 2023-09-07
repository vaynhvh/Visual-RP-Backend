using Backend.Controllers;
using Backend.Models;
using Backend.Models.Appearance;
using Backend.Modules.Bank;
using Backend.Modules.Blitzer;
using Backend.Modules.Discord;
using Backend.Modules.Faction;
using Backend.Modules.Inventory;
using Backend.Modules.Labs;
using Backend.Modules.Laptop.Apps;
using Backend.Modules.Phone;
using Backend.Modules.Phone.Apps;
using Backend.Modules.Rank;
using Backend.Modules.Voice;
using Backend.Modules.Workstation;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using DSharpPlus.Entities;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Backend.Models.RXContainer;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Player
{
    public class LoginObject
    {
        [JsonProperty("n")]
        public string Name { get; set; }

        [JsonProperty("d")]
        public List<DbLoadingscreenSongs> Songs { get; set; }

    }



    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class LoginModule : RXModule
    {
        public LoginModule() : base("Login", new RXWindow("Login")) { textInputBox = new RXWindow("TextInputBox"); }

        public static Dictionary<string, string> DiscordCodes = new Dictionary<string, string>();

        public RXWindow textInputBox;

        public static int voiceHash = 1; 
        public static List<string> random_spawns = new List<string>
        {
            "-1042,-2745,21",
        };

        public static string PaintballSpawn = "-116.22954, -1772.4661, 29.823433";
        /*

        public object textInputBoxObject = new
        {
            textBoxObject = new
            {
                Title = "Registrierung",
                Message = "Gebe bitte deinen Benutzernamen ein (Format: Vorname_Nachname). Sollte es keinen Account mit dem gleichen Namen geben, wirst du registriert.",
                Callback = "RegisterUser",
                CloseCallback = "RetryLogin"
            }
        };
        */
        public object textInputBoxObject = new
        {
            t = "Gebe bitte deinen Benutzernamen ein (Format: Vorname_Nachname). Sollte es keinen Account mit dem gleichen Namen geben, wirst du registriert.",
            e = "RegisterUser",
            c = "RetryLogin"
        };

        [RemoteEvent]
        public async Task sendClientIdentificationHash(RXPlayer player, string hash)
        {
            if (player.IsLoggedIn || !string.IsNullOrEmpty(player.ClientHash)) return;

            if (string.IsNullOrEmpty(hash))
            {
                await player.KickAsync();

                return;
            }

            player.ClientHash = hash;
        }

        //[HandleExceptions]
        public override async Task OnPlayerConnect(RXPlayer player)
        {
            try
            {
                if (player.IsLoggedIn) return;

                await Task.Delay(500);

                if (!Configuration.Open)
                {
                    await player.KickAsync("Der Server ist noch am starten!");

                    return;
                }

                if (string.IsNullOrEmpty(player.ClientHash))
                {
                    object confirmationBoxObject = new
                    {
                        t = "Dein Clienthash ist Leer. Möchtest du ihn haben?",
                        ft = "Ja",
                        st = "Nein",
                        fe = "seeclienthash",
                        se = "Close",
                        d = 0,
                    };

                    var confirmation = new RXWindow("Confirm");

                    await confirmation.OpenWindow(player, confirmationBoxObject);

                    return;
                }

                using var db = new RXContext();

                List<DbIdentifier> blacklistedIdentifiers = await db.BlacklistedIdentifiers.ToListAsync();
                if (blacklistedIdentifiers == null) return;

                if (blacklistedIdentifiers.Count > 0)
                {
                    string socialClubName = await NAPI.Task.RunReturnAsync(() => player.SocialClubName);
                    string playerAddress = await NAPI.Task.RunReturnAsync(() => player.Address);
                    string socialClubId = await NAPI.Task.RunReturnAsync(() => player.SocialClubId.ToString());
                    string hardwareId = await NAPI.Task.RunReturnAsync(() => player.Serial);
                    string clientHash = player.ClientHash;

                    bool socialClubNameBlacklisted = blacklistedIdentifiers.FirstOrDefault(x => x.Identifier == socialClubName) != null;
                    bool playerAddressBlacklisted = blacklistedIdentifiers.FirstOrDefault(x => x.Identifier == playerAddress) != null;
                    bool socialClubIdBlacklisted = blacklistedIdentifiers.FirstOrDefault(x => x.Identifier == socialClubId) != null;
                    bool hardwareIdBlacklisted = blacklistedIdentifiers.FirstOrDefault(x => x.Identifier == hardwareId) != null;
                    bool clientHashBlacklisted = blacklistedIdentifiers.FirstOrDefault(x => x.Identifier == clientHash) != null;

                    if (socialClubNameBlacklisted || playerAddressBlacklisted || socialClubIdBlacklisted || hardwareIdBlacklisted || clientHashBlacklisted)
                    {
                        if (!socialClubNameBlacklisted) await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = socialClubName });
                        if (!playerAddressBlacklisted) await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = playerAddress });
                        if (!socialClubIdBlacklisted) await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = socialClubId });
                        if (!hardwareIdBlacklisted) await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = hardwareId });
                        if (!clientHashBlacklisted) await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = clientHash });

                        await db.SaveChangesAsync();

                        await player.KickAsync("Dein Ban ist aktiv!");
                        return;
                    }
                }

                await player.SetDimensionAsync(Convert.ToUInt32(new Random().Next(2500, 999999)));
                await player.SpawnAsync(new Vector3(-2593.6301f, -487.0295f, -19.78751f));

                player.Freezed = true;
                player.Invincible = true;
                player.Invisible = true;


                await player.TriggerEventAsync("OnPlayerReady");

                var social = await player.GetSocialNameAsync();
                var charname = await player.GetNameAsync();

                DbPlayer dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Username == charname);
                if (dbPlayer == null)
                {
                    player.Kick("Du hast keinen Account");
                    /*var confirmation = new RXWindow("Input");

                    await confirmation.OpenWindow(player, textInputBoxObject);*/
                    return;
                }

                if (dbPlayer.DiscordID == "0")
                {
                    object confirmationBoxObject = new
                    {
                        t = "Gebe nun den Discord Code ein, den du per /verify in unserem Discord erhalten hast!",
                        e = "ValidateDiscordAccount",
                        c = "RetryLogin"
                    };

                    var confirmation = new RXWindow("Input");

                    await confirmation.OpenWindow(player, confirmationBoxObject);
                    return;
                }
                if (dbPlayer.password == "")
                {
                    object confirmationBoxObject = new
                    {
                        t = "Gebe nun ein Passwort ein!",
                        e = "ValidatePassword",
                        c = "RetryLogin"
                    };

                    var confirmation = new RXWindow("Input");

                    await confirmation.OpenWindow(player, confirmationBoxObject);
                    return;
                }
                /*
               if (dbPlayer.RankId < 1)
               {
                  await player.KickAsync("Ban aktiv! Spaß Wartungsarbeiten.");
                  return;
               }*/

                await player.SetNameAsync(dbPlayer.Username);

                /*DiscordGuild guild = await Resource.discord.GetGuildAsync(1130973899715260497);

                DiscordMember member = await guild.GetMemberAsync(ulong.Parse(dbPlayer.DiscordID));


                var roles = member.Roles.ToList();

                if (roles.Contains(guild.GetRole(1139567791448539146)))
                {
                    if (DateTime.Now.Subtract(dbPlayer.LastSeen).TotalHours < 6 && DateTime.Now.Subtract(dbPlayer.LastSeen).TotalDays < 1)
                    {
                        await TryLogin(player, player.DiscordLoginHash);
                        return;
                    }
                }

                if (roles.Contains(guild.GetRole(1139567789808570449)))
                {
                    if (DateTime.Now.Subtract(dbPlayer.LastSeen).TotalHours < 6 && DateTime.Now.Subtract(dbPlayer.LastSeen).TotalDays < 1)
                    {
                        await TryLogin(player, player.DiscordLoginHash);
                        return;
                    }
                }*/

                LoginObject loginObject = new LoginObject() { Name = dbPlayer.Username, Songs = await db.LoginSongs.ToListAsync() };

                await this.Window.OpenWindow(player, loginObject);

            } catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        [RemoteEvent]
        public async Task seesocialclubid(RXPlayer player)
        {
            player.SendNotification(player.SocialClubId.ToString());
            player.SendNotification("Du wirst in 5 Sekunden gekickt!");
            await Task.Delay(5000);
            player.Kick("Zeit abgelaufen");
        }

        [RemoteEvent]
        public async Task seehwid(RXPlayer player)
        {
            player.SendNotification(await player.GetSerialAsync());
            player.SendNotification("Du wirst in 5 Sekunden gekickt!");
            await Task.Delay(5000);
            player.Kick("Zeit abgelaufen");
        }

        [RemoteEvent]
        public async Task seeip(RXPlayer player)
        {
            player.SendNotification(await player.GetAddressAsync());
            player.SendNotification("Du wirst in 5 Sekunden gekickt!");
            await Task.Delay(5000);
            player.Kick("Zeit abgelaufen");
        }

        [RemoteEvent]
        public async Task seeclienthash(RXPlayer player)
        {
            player.SendNotification(player.ClientHash);
            player.SendNotification("Du wirst in 5 Sekunden gekickt!");
            await Task.Delay(5000);
            player.Kick("Zeit abgelaufen");
        }

        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        //[HandleExceptions]
        [RemoteEvent]
        public async Task RegisterUser(RXPlayer player, string register_name)
        {
            try { 
            if (player.IsLoggedIn) return;

            using var db = new RXContext();

            bool flag = true;
            string msg = "";

            if (!register_name.Contains("_"))
            {
                flag = false;
                msg = "Dein Name muss einen Unterstrich enthalten.";
            }
            else if (register_name.Length > 32)
            {
                flag = false;
                msg = "Dein Name darf nicht länger als 32 Zeichen sein.";
            }
            else if (await register_name.ContainsSymbols())
            {
                flag = false;
                msg = "Dein Name darf keine Symbole oder Sonderzeichen enthalten.";
            }

            if ((register_name.Split('l').Count() - 1) > 2 && (register_name.Split('I').Count() - 1) > 2)
            {
                msg = "Es ist ein Problem mit deinem Namen aufgetreten. Aus Sicherheitsgründen wurdest du gekickt und es wurde ein Log an die Administration gesendet.";

                lock (player) if (RX.PlayerExists(player)) NAPI.Task.Run(() => player.Kick());

                return;
            }


            if (!flag)
            {
                await player.SendNotificationAsync(msg, true);

                await textInputBox.OpenWindow(player, textInputBoxObject);

                return;
            }
            else
            {
                DbPlayer dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Username == register_name);
                if (dbPlayer != null)
                {
                    await player.SendNotificationAsync("Dieser Account existiert bereits.", true);

                    await textInputBox.OpenWindow(player, textInputBoxObject);

                    return;
                }

                await player.SetNameAsync(register_name);

                var name = await player.GetNameAsync();
                var social = await player.GetSocialNameAsync();
                var socialid = await player.GetSocialIdAsync();

                dbPlayer = await db.Players.FirstOrDefaultAsync(p => p.Username == name);
                if (dbPlayer == null)
                {
                    dbPlayer = await db.Players.FirstOrDefaultAsync(c => c.SocialClubId == social);
                     if (dbPlayer != null)
                     {
                         await player.KickAsync("Du hast bereits einen Account!");
                         return;
                     }
                     
                    await db.Players.AddAsync(new DbPlayer
                    {
                        Username = await player.GetNameAsync(),
                        DiscordID = "0",
                        SocialClubId = await player.GetSocialNameAsync(),
                        SocialClubNumber = await NAPI.Task.RunReturnAsync(() => player.SocialClubId.ToString()),
                        HWID = await player.GetSerialAsync(),
                        IP = await player.GetAddressAsync(),
                        ClientHash = player.ClientHash,
                        Position = random_spawns[new Random().Next(random_spawns.Count)]
                    });
                    await db.SaveChangesAsync();

                    object confirmationBoxObject = new
                    {
                        t = "Gebe nun den Discord Code ein, den du per /verify in unserem Discord erhalten hast!",
                        e = "ValidateDiscordAccount",
                        c = "RetryLogin"
                    };

                    var confirmation = new RXWindow("Input");

                    await confirmation.OpenWindow(player, confirmationBoxObject);


                    return;
                }
            }
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        [RemoteEvent]
        public async Task ValidateDiscordAccount(RXPlayer player, string code)
        {
            try { 
            if (player.IsLoggedIn) return;
            using var db = new RXContext();

            var name = await player.GetNameAsync();
            var social = await player.GetSocialNameAsync();
            var socialid = await player.GetSocialIdAsync();

            DbPlayer dbPlayer = await db.Players.FirstOrDefaultAsync(p => p.Username == name);
            if (dbPlayer == null)
            {
                await player.KickAsync("No Acc?");
                return;
            }

            if (code.Contains("DROP") || code.Contains("INSERT") || code.Contains("UPDATE") || code.Contains("\'"))
            {
                await player.BanPlayer("SQL Injection");
                return;
            }

            foreach (var data in DiscordCodes)
            {
                if (data.Value != code) continue;
                dbPlayer.DiscordID = data.Key;
                DiscordCodes.Remove(data.Key);
                await db.SaveChangesAsync();
                await RetryLogin(player);
                break;
            }
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        public List<string> unsecurePasswords = new List<string>
        {
            "pass",
            "1234",
            "abcd",
            "admin",
            "password",
            "12345",
            "123456",
            "12345678",
            "password1",
            "qwerty",
            "letmein",
            "welcome",
            "iloveyou",
            "monkey",
            "1234567",
            "sunshine",
            "princess",
            "abc123",
            "superman",
            "trustno1",
            "password123",
            "password12",
            "1234567890",
            "qazwsx",
            "admin123",
            "password12345",
            "default",
            "user",
            "open",
            "login",
            "Test123",
            "nigga123",
            "nigga",
            "Test"
        };

        [RemoteEvent]
        public async Task ValidatePassword(RXPlayer player, string password)
        {
            try
            {
                if (player.IsLoggedIn) return;
                using var db = new RXContext();

                var name = await player.GetNameAsync();
                var social = await player.GetSocialNameAsync();
                var socialid = await player.GetSocialIdAsync();

                DbPlayer dbPlayer = await db.Players.FirstOrDefaultAsync(p => p.Username == name);
                if (dbPlayer == null)
                {
                    await player.KickAsync("No Acc?");
                    return;
                }

                if (password.Contains("DROP") || password.Contains("INSERT") || password.Contains("UPDATE") || password.Contains("\'"))
                {
                    await player.BanPlayer("SQL Injection");
                    return;
                }

                if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(password))
                {
                    await player.SendNotificationAsync("Bitte gebe ein Passwort ein!");

                    object confirmationBoxObject = new
                    {
                        t = "Gebe nun ein Passwort ein!",
                        e = "ValidatePassword",
                        c = "RetryLogin"
                    };

                    var confirmation = new RXWindow("Input");

                    await confirmation.OpenWindow(player, confirmationBoxObject);
                    return;
                }

                if (password.Length < 4)
                {
                    await player.SendNotificationAsync("Dein Passwort muss mindestens 4 Zeichen lang sein!");

                    object confirmationBoxObject = new
                    {
                        t = "Gebe nun ein Passwort ein!",
                        e = "ValidatePassword",
                        c = "RetryLogin"
                    };

                    var confirmation = new RXWindow("Input");

                    await confirmation.OpenWindow(player, confirmationBoxObject);
                    return;
                }

                if (password.Length > 16)
                {
                    await player.SendNotificationAsync("Dein Passwort darf maximal 16 Zeichen lang sein!");

                    object confirmationBoxObject = new
                    {
                        t = "Gebe nun ein Passwort ein!",
                        e = "ValidatePassword",
                        c = "RetryLogin"
                    };

                    var confirmation = new RXWindow("Input");

                    await confirmation.OpenWindow(player, confirmationBoxObject);
                    return;
                }

                if (unsecurePasswords.Contains(password))
                {
                    await player.SendNotificationAsync("Dieses Passwort ist zu Unsicher!");

                    object confirmationBoxObject = new
                    {
                        t = "Gebe nun ein Passwort ein!",
                        e = "ValidatePassword",
                        c = "RetryLogin"
                    };

                    var confirmation = new RXWindow("Input");

                    await confirmation.OpenWindow(player, confirmationBoxObject);
                    return;
                }

                dbPlayer.password = ComputeSha256Hash(password);
                await db.SaveChangesAsync();
                await RetryLogin(player);
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task RetryLogin(RXPlayer player)
        {
            if (player.IsLoggedIn) return;

            try { 
            await OnPlayerConnect(player);

            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
            return;
        }

        static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task TryLogin(RXPlayer player, string pw)
        {
            try {
            if (player.IsLoggedIn || string.IsNullOrEmpty(player.ClientHash)) return;
            using var db = new RXContext();

            var name = await player.GetNameAsync();
            var social = await player.GetSocialNameAsync();
            var socialid = await player.GetSocialIdAsync(); 

            DbPlayer dbPlayer = await db.Players.FirstOrDefaultAsync(p => p.Username == name);
            if (dbPlayer == null)
            {
                await player.KickAsync("Du hast keinen Charakter!");
                return;
            }

            if (string.IsNullOrEmpty(dbPlayer.SocialClubId))
            {
                dbPlayer.SocialClubNumber = socialid.ToString();
                dbPlayer.SocialClubId = await player.GetSocialNameAsync();

                await db.SaveChangesAsync();

                await player.ChangeLoaderMessage("Account-Daten werden gespeichert!");
            }
            else
            {
                await player.ChangeLoaderMessage("Einloggen...");
            }

            if (ComputeSha256Hash(pw) != dbPlayer.password)
            {
                await this.Window.TriggerEvent(player, "status", "Passwort falsch.");
                return;
            }

            if (await player.GetSocialNameAsync() != dbPlayer.SocialClubId)
            {
                await this.Window.TriggerEvent(player, "status", "Falsche SocialClub-ID, im Support melden.");
                return;
            }

            if (dbPlayer.IP != await player.GetAddressAsync() || dbPlayer.HWID != await player.GetSerialAsync() || dbPlayer.ClientHash != player.ClientHash)
            {
                dbPlayer.IP = await player.GetAddressAsync();
                dbPlayer.HWID = await player.GetSerialAsync();
                dbPlayer.ClientHash = player.ClientHash;

                await db.SaveChangesAsync();
            }

            player.Id = dbPlayer.Id;

            await player.SetNameAsync(dbPlayer.Username);

            player.DiscordID = dbPlayer.DiscordID;
            player.TeamId = dbPlayer.TeamId;
            player.Teamrank = dbPlayer.TeamrankId;
            player.InventoryId = dbPlayer.InventoryId;
            player.BankAccountId = dbPlayer.BankAccountId;
            player.Cash = dbPlayer.Cash;
            player.Blackmoney = dbPlayer.Blackmoney;
            player.Stress = dbPlayer.Stress;
            player.Hunger = dbPlayer.Hunger;
            player.Thirst = dbPlayer.Thirst;
            player.LastSeen = DateTime.Now;
            player.Jailtime = dbPlayer.Jailtime;
            player.Sport = dbPlayer.Sport;
            player.GangwarContainerId = dbPlayer.GangwarContainerId;
            player.WorkstationInputContainerId = dbPlayer.WorkstationInputContainerId;
            player.WorkstationOutputContainerId = dbPlayer.WorkstationOutputContainerId;
            player.WorkstationId = dbPlayer.WorkstationId;
            player.LabInputContainerId = dbPlayer.LabInputContainerId;
            player.LabOutputContainerId = dbPlayer.LabOutputContainerId;
            player.WalletAdress = dbPlayer.WalletAdress;
            player.WalletValue = dbPlayer.WalletValue;
            player.HouseId = dbPlayer.HouseId;
            player.LuckyWheel = dbPlayer.LuckyWheel;
            player.Attachments = new Dictionary<int, DbAttachment>();
            player.DeathData = new RXDeathData
            {
                IsDead = dbPlayer.DeathStatus,
                DeathTime = dbPlayer.DeathTime
            };
            player.Coma = dbPlayer.Coma;

            if (dbPlayer.Weapons.IsValidJson<List<WeaponLoadoutItem>>())
                player.Weapons = JsonConvert.DeserializeObject<List<WeaponLoadoutItem>>(dbPlayer.Weapons);

            if (dbPlayer.Storages.IsValidJson<List<uint>>())
                player.Storages = JsonConvert.DeserializeObject<List<uint>>(dbPlayer.Storages);

            if (dbPlayer.FunkFav.IsValidJson<List<uint>>())
                player.FunkFav = JsonConvert.DeserializeObject<List<uint>>(dbPlayer.FunkFav);

            if (dbPlayer.RankId > 10)
            {
                player.Rank = RankModule.Ranks.FirstOrDefault(x => x.Permission == dbPlayer.RankId);
            }

            player.ForumId = dbPlayer.ForumId;
            player.Paytime = dbPlayer.Paytime;
            player.Level = dbPlayer.Level;
            player.InDuty = dbPlayer.InDuty;
            player.Warns = dbPlayer.Warns;
            player.IsMale = Convert.ToBoolean(dbPlayer.IsMale);
            player.DateOfEntry = dbPlayer.DateOfEntry;

            DiscordModule.Logs.Add(new DiscordLog("Login", (await player.GetNameAsync()) + " hat sich eingeloggt (Warns: " + player.Warns + ")", DiscordModule.Login));

            DbContainer dbContainer = await db.Containers.FirstOrDefaultAsync(con => con.Id == player.InventoryId);
            if (player.InventoryId == 0 || dbContainer == null)
            {
                dbContainer = new DbContainer
                {
                    Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                    Name = "Inventar",
                    MaxSlots = 12,
                    MaxWeight = 10000
                };

                dbPlayer.InventoryId = dbContainer.Id;

                await db.Containers.AddAsync(dbContainer);
                await db.SaveChangesAsync();
            }

            DbBankAccount dbBankAccount = await db.BankAccounts.FirstOrDefaultAsync(con => con.Id == player.BankAccountId);
            if (dbBankAccount == null)
            {
                dbBankAccount = new DbBankAccount
                {
                    Id = await db.BankAccounts.CountAsync() == 0 ? 1 : (await db.BankAccounts.MaxAsync(con => con.Id) + 1),
                    Name = await player.GetNameAsync(),
                    Balance = 30000,
                    History = new List<DbBankHistory>()
                };

                dbPlayer.BankAccountId = dbBankAccount.Id;

                await db.BankAccounts.AddAsync(dbBankAccount);
                await db.SaveChangesAsync();

                await RX.TakeMoneyFromStaatskonto(30000, "Neuer Spieler " + player.Id);

                await player.GiveMoney(20000);
            }


            DbPlayerCrimeData dbCrimeData = await db.PlayerCrimeData.FirstOrDefaultAsync(con => con.PlayerId == player.Id);
            if (dbCrimeData == null)
            {
                dbCrimeData = new DbPlayerCrimeData
                {
                    Id = await db.PlayerCrimeData.CountAsync() == 0 ? 1 : (await db.PlayerCrimeData.MaxAsync(con => con.Id) + 1),
                    PlayerId = player.Id,
                    Address = "Kein Wohnort bekannt!",
                    Phone = player.Id.ToString(),
                    Note = "",
                    Membership = "Keine Zugehörigkeit bekannt!",
                    Info = "",
                    CanAktenView = true,
                };

                await db.PlayerCrimeData.AddAsync(dbCrimeData);
                await db.SaveChangesAsync();
            }

            DbTeamMemberData dbTeamMemberData = await db.TeamMemberDatas.FirstOrDefaultAsync(con => con.PlayerId == player.Id);
            if (dbTeamMemberData == null)
            {
                dbTeamMemberData = new DbTeamMemberData
                {
                    Id = await db.TeamMemberDatas.CountAsync() == 0 ? 1 : (await db.TeamMemberDatas.MaxAsync(con => con.Id) + 1),
                    PlayerId = player.Id,
                };

                await db.TeamMemberDatas.AddAsync(dbTeamMemberData);
                await db.SaveChangesAsync();
            }

            DbBusinessMemberData dbBusinessMemberData = await db.BusinessMemberDatas.FirstOrDefaultAsync(con => con.PlayerId == player.Id);
            if (dbBusinessMemberData == null)
            {
                dbBusinessMemberData = new DbBusinessMemberData
                {
                    Id = await db.BusinessMemberDatas.CountAsync() == 0 ? 1 : (await db.BusinessMemberDatas.MaxAsync(con => con.Id) + 1),
                    PlayerId = player.Id,
                };

                await db.BusinessMemberDatas.AddAsync(dbBusinessMemberData);
                await db.SaveChangesAsync();
            }

            DbPhoneSettings dbPhoneSettings = await db.PhoneSettings.FirstOrDefaultAsync(con => con.PlayerId == player.Id);
            if (dbPhoneSettings == null)
            {
                dbPhoneSettings = new DbPhoneSettings
                {
                    Id = await db.PhoneSettings.CountAsync() == 0 ? 1 : (await db.PhoneSettings.MaxAsync(con => con.Id) + 1),
                    PlayerId = player.Id,
                };

                await db.PhoneSettings.AddAsync(dbPhoneSettings);
                await db.SaveChangesAsync();
            }



            if (dbPlayer.AnimationShortcuts.IsValidJson<Dictionary<uint, uint>>())
                player.AnimationShortcuts = JsonConvert.DeserializeObject<Dictionary<uint, uint>>(dbPlayer.AnimationShortcuts);


            DbContainer dbGangwarContainer = await db.Containers.FirstOrDefaultAsync(con => con.Id == player.GangwarContainerId);
            if (dbGangwarContainer == null)
            {
                dbGangwarContainer = new DbContainer
                {
                    Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                    Name = "Gangwar-Waffen",
                    MaxSlots = 30,
                    MaxWeight = 10000
                };

                dbPlayer.GangwarContainerId = dbGangwarContainer.Id;

                await db.Containers.AddAsync(dbGangwarContainer);
                await db.SaveChangesAsync();
            }


            DbContainer dbWorkstationInputContainer = await db.Containers.FirstOrDefaultAsync(con => con.Id == player.WorkstationInputContainerId);
            if (dbWorkstationInputContainer == null)
            {
                dbWorkstationInputContainer = new DbContainer
                {
                    Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                    Name = "Workstation Input",
                    MaxSlots = 60,
                    MaxWeight = 60000
                };

                dbPlayer.WorkstationInputContainerId = dbWorkstationInputContainer.Id;

                await db.Containers.AddAsync(dbWorkstationInputContainer);
                await db.SaveChangesAsync();
            }

            DbContainer dbWorkstationOutputContainer = await db.Containers.FirstOrDefaultAsync(con => con.Id == player.WorkstationOutputContainerId);
            if (dbWorkstationOutputContainer == null)
            {
                dbWorkstationOutputContainer = new DbContainer
                {
                    Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                    Name = "Workstation Output",
                    MaxSlots = 60,
                    MaxWeight = 60000
                };

                dbPlayer.WorkstationOutputContainerId = dbWorkstationOutputContainer.Id;

                await db.Containers.AddAsync(dbWorkstationOutputContainer);
                await db.SaveChangesAsync();
            }

            player.WorkstationOutputContainerId = dbWorkstationOutputContainer.Id;
            player.WorkstationInputContainerId = dbWorkstationInputContainer.Id;

            DbContainer dbLabInputContainer = await db.Containers.FirstOrDefaultAsync(con => con.Id == player.LabInputContainerId);
            if (dbLabInputContainer == null)
            {
                dbLabInputContainer = new DbContainer
                {
                    Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                    Name = "Labor Input",
                    MaxSlots = 60,
                    MaxWeight = 60000
                };

                dbPlayer.LabInputContainerId = dbLabInputContainer.Id;

                await db.Containers.AddAsync(dbLabInputContainer);
                await db.SaveChangesAsync();
            }

            DbContainer dbLabOutputContainer = await db.Containers.FirstOrDefaultAsync(con => con.Id == player.LabOutputContainerId);
            if (dbLabOutputContainer == null)
            {
                dbLabOutputContainer = new DbContainer
                {
                    Id = await db.Containers.CountAsync() == 0 ? 1 : (await db.Containers.MaxAsync(con => con.Id) + 1),
                    Name = "Labor Output",
                    MaxSlots = 60,
                    MaxWeight = 60000
                };

                dbPlayer.LabOutputContainerId = dbLabOutputContainer.Id;

                await db.Containers.AddAsync(dbLabOutputContainer);
                await db.SaveChangesAsync();
            }

            await player.TriggerEventAsync("loadlschangar");


            await PoliceEditPersonApp.loadAllBlips(player);


            player.LabInputContainerId = dbLabInputContainer.Id;
            player.LabOutputContainerId = dbLabOutputContainer.Id;

            player.InventoryId = dbContainer.Id;
            player.GangwarContainerId = dbGangwarContainer.Id;

            player.Freezed = false;
            player.Invincible = false;
            player.Invisible = false;

            if (dbPlayer.Phone == 0)
            {
                dbPlayer.Phone = await PhoneModule.generateRandomPhonenumber(player);
                player.Phone = dbPlayer.Phone;
            }
            else
            {
                player.Phone = dbPlayer.Phone;
            }

            await db.SaveChangesAsync();

            await player.TriggerEventAsync("skyMover");

            await this.Window.TriggerEvent(player, "status", "successfully");

            await WorkstationModule.LoadPlayerWorkstationPoints(player, player.WorkstationId);
            await LabManager.LoadPlayerLabPoints(player);

            await player.TriggerEventAsync("onPlayerLoaded", (await player.GetNameAsync()).Split("_")[0], (await player.GetNameAsync()).Split("_")[1], player.Id, 0, 0, 0, player.Cash, 0, 0, player.Team.Id, player.Teamrank, player.Level, 0, 0, 0, 0, "", 0, player.Id, 0, 0, "[]", /*player.Rank.Id*/0, 0.6f, 0, 0, player.Blackmoney, 0, 0, 0, player.Sport, player.Stress, player.Hunger, player.Thirst);

            await player.TriggerEventAsync("setPlayerInfoId", player.Id);

            voiceHash = voiceHash + 1;
            await player.TriggerEventAsync("ConnectTeamspeak");
            await player.TriggerEventAsync("setVoiceData", 2562, "Voice-Channel", "Test123");


            player.VoiceHash = "00" + voiceHash;
            await player.TriggerEventAsync("setPlayerInfoVoiceHash", "00" + voiceHash);
            await NAPI.Task.RunAsync(() =>
            { 
                player.SetSharedData("voiceHash", "00" + voiceHash);
                player.SetSharedData("voiceRange", (int)VoiceRange.normal);
                player.SetData("voiceType", 2562);
            });

            
            await player.TriggerEventAsync("setVoiceHash", "00" + voiceHash);

            await player.TriggerEventAsync("setVoiceType", 2562);

            await player.TriggerEventAsync("togglePaytime", true);
            await player.TriggerEventAsync("setPaytime", player.Paytime);

            await player.TriggerEventAsync("setPlayerInfoForumId", player.ForumId);
            await player.TriggerEventAsync("unloadClientIpl", "rc12b_fixed");
            await player.TriggerEventAsync("unloadClientIpl", "rc12b_destroyed");
            await player.TriggerEventAsync("unloadClientIpl", "rc12b_default");
            await player.TriggerEventAsync("unloadClientIpl", "rc12b_hospitalinterior_lod");
            await player.TriggerEventAsync("unloadClientIpl", "rc12b_hospitalinterior");
            await player.TriggerEventAsync("loadClientIpl", "bkr_biker_interior_placement_interior_5_biker_dlc_int_ware04_milo");

            await player.TriggerEventAsync("responsePhoneContacts", JsonConvert.SerializeObject(ContactsApp.PhoneContacts.Where(x => x.PlayerId == player.Id).ToList()));
            await player.TriggerEventAsync("UpdateWallpaper", player.PhoneSettings.Wallpaper);
            await player.TriggerEventAsync("UpdateRingtone", player.PhoneSettings.Ringtone);
            await player.TriggerEventAsync(PlayerDatas.DutyEvent, player.InDuty);

            if (player.Rank.Permission > 1) player.SetData("acnametags", 1);
            if (player.Rank.Permission > 92) player.SetData("acexplosion", 1); player.SetData("acblacklistweapon", 1); player.SetData("acgiveweapon", 1);

            if (player.AnimationShortcuts.Count() != 16)
            {
                player.AnimationShortcuts = new Dictionary<uint, uint>
                {
                    { 0, 0 },
                    { 1, 0 }
                };

                while (player.AnimationShortcuts.Count() < 16 && player.AnimationShortcuts.Count > 0)
                {
                    player.AnimationShortcuts.Add(player.AnimationShortcuts.Last().Key + 1, 0);
                }

                await player.SaveAnimationShortcuts();
            }

            await player.UpdateAnimationShortcuts();
            await this.Window.CloseWindow(player);

            DbCharacter dbCharacter = await db.Characters.FirstOrDefaultAsync(c => c.Id == player.Id);
            if (dbCharacter == null)
            {
                RXWindow CharacterCreator = new RXWindow("Char");

                player.IsLoggedIn = true;

                await player.SetDimensionAsync((uint)new Random().Next(2500, 1000000));
                await player.SetPositionAsync(new Vector3(-1832.6901f, -1240.9187f, 13.00293f));

                await player.EvalAsync("mp.players.local.setHeading(-185);");

                await Task.Delay(1000);

                lock (player) if (!RX.PlayerExists(player)) return;

                string Customization = "{\"Gender\":0,\"Parents\":{\"FatherShape\":0,\"MotherShape\":0,\"FatherSkin\":0,\"MotherSkin\":0,\"Similarity\":1,\"SkinSimilarity\":1},\"Features\":[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],\"Hair\":{\"Hair\":0,\"Color\":0,\"HighlightColor\":0},\"Appearance\":[{\"Value\":255,\"Opacity\":1},{\"Value\":255,\"Opacity\":1},{\"Value\":1,\"Opacity\":1},{\"Value\":5,\"Opacity\":0.4},{\"Value\":0,\"Opacity\":0},{\"Value\":0,\"Opacity\":0},{\"Value\":255,\"Opacity\":1},{\"Value\":255,\"Opacity\":1},{\"Value\":0,\"Opacity\":0},{\"Value\":255,\"Opacity\":1},{\"Value\":255,\"Opacity\":1}],\"EyebrowColor\":0,\"BeardColor\":0,\"EyeColor\":0,\"BlushColor\":0,\"LipstickColor\":0,\"ChestHairColor\":0}";

                if (dbPlayer.IsMale == 0)
                {


                    
                   Customization = "{\"Gender\":1,\"Parents\":{\"FatherShape\":0,\"MotherShape\":0,\"FatherSkin\":0,\"MotherSkin\":0,\"Similarity\":1,\"SkinSimilarity\":1},\"Features\":[0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0],\"Hair\":{\"Hair\":0,\"Color\":0,\"HighlightColor\":0},\"Appearance\":[{\"Value\":255,\"Opacity\":1},{\"Value\":255,\"Opacity\":1},{\"Value\":1,\"Opacity\":1},{\"Value\":5,\"Opacity\":0.4},{\"Value\":0,\"Opacity\":0},{\"Value\":0,\"Opacity\":0},{\"Value\":255,\"Opacity\":1},{\"Value\":255,\"Opacity\":1},{\"Value\":0,\"Opacity\":0},{\"Value\":255,\"Opacity\":1},{\"Value\":255,\"Opacity\":1}],\"EyebrowColor\":0,\"BeardColor\":0,\"EyeColor\":0,\"BlushColor\":0,\"LipstickColor\":0,\"ChestHairColor\":0}";

                   NAPI.Task.Run(() => player.SetSkin(PedHash.FreemodeFemale01));
                
                }


                Customization customization = JsonConvert.DeserializeObject<Customization>(Customization);

                await CharacterCreator.OpenWindow(player, customization);
            }
            else
            {
                lock (player) if (!RX.PlayerExists(player)) return;

                if (!Configuration.DevMode)
                    await player.TriggerEventAsync("skyMover");

                await player.UpdateHeadBlendAsync(0, 0, 0);

                if (Configuration.PaintballEvent)
                {
                    await player.SpawnAsync(PaintballSpawn.ToPos() + new Vector3(0, 0, 0.52f));
                }
                else
                {

                    if (string.IsNullOrEmpty(dbPlayer.Position))
                        dbPlayer.Position = random_spawns[new Random().Next(random_spawns.Count)];

                    await player.SpawnAsync(dbPlayer.Position.ToPos() + new Vector3(0, 0, 0.52f));

                }

                player.IsLoggedIn = true;

                await player.LoadCharacter(dbCharacter);

                if (player.Rank.Permission > 1) { player.SetData("acnametags", 1); }
                if (player.Rank.Permission > 92) { player.SetData("acexplosion", 1); player.SetData("acblacklistweapon", 1); player.SetData("acgiveweapon", 1); }
                if (player.Rank.Permission > 99) { player.SetData("acignore", 1); }

                await player.SetHealthAsync(dbPlayer.HP);
                await player.SetArmorAsync(dbPlayer.Armor);
                await player.SetDimensionAsync(0);

                await Task.Delay(2000);

                if (player.DeathData.IsDead)
                {
                    await player.TriggerEventAsync("transitionToBlurred", 50);

                    player.DeathData = new RXDeathData
                    {
                        DeathTime = DateTime.Now,
                        IsDead = true
                    };


                    if (Configuration.PaintballEvent)
                    {
                        await player.SpawnAsync(PaintballSpawn.ToPos() + new Vector3(0, 0, 0.52f));
                    }
                    else
                    {
                        await player.SpawnAsync(await player.GetPositionAsync());
                    }

                    player.Freezed = true;

                    await Task.Delay(1000);

                    await DeathController.ApplyDeathEffectsAsync(player);

                    await Task.Delay(1000);

                    if (player.Coma)
                    {
                        await NAPI.Task.RunAsync(async () =>
                            await NAPI.Pools.GetAllObjects().Where(x => x.Position.DistanceTo(player.Position) < 4).forEachAlternativeAsync(x => x.Delete()));

                        player.Invisible = true;
                        player.DeathProp = await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("xm_prop_body_bag"), player.Position.Subtract(new Vector3(0, 0, 1)), new Vector3(), 255, 0));

                        await new RXWindow("Death").OpenWindow(player);
                    }
                }
            }
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }
    }
}
