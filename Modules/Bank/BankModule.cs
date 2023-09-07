using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Discord;
using Backend.Modules.Phone.Apps;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Bank
{
    public class MainBank
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public Vector3 Position { get; set; }

        public MainBank(uint id, string name, Vector3 position)
        {
            Id = id;
            Name = name;
            Position = position;
        }
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class BankModule : RXModule
    {
        public BankModule() : base("Bank", new RXWindow("Bank")) { }

        public static List<DbBankAccount> BankAccounts = new List<DbBankAccount>();

        public static List<uint> robbedatms = new List<uint>();

        public static List<MainBank> Banks = new List<MainBank>
        {
            new MainBank(300, "", new Vector3(263.96445, 212.34377, 106.28325)),
            new MainBank(301, "Fleeca Bank Vinewood", new Vector3(314.04037, -278.26508, 54.17044)),
            new MainBank(302, "Fleeca Bank Rockford 2", new Vector3(-350.6924, -48.758533, 49.036892)),
            new MainBank(303, "Fleeca Bank Rockford", new Vector3(-1213.4681, -330.5639, 37.786896)),
            new MainBank(304, "Maze Bank Vespucci", new Vector3(-1315.9424, -835.7865, 16.961773)),
            new MainBank(305, "Fleeca Bank Würfelpark", new Vector3(150.0527, -1040.3718, 29.373518)),
            new MainBank(306, "Fleeca Bank West Highway", new Vector3(-2963.2795, 482.6949, 15.70308)),
            new MainBank(307, "Fleeca Bank Harmony", new Vector3(1175.6918, 2705.85, 38.09327)),
            new MainBank(308, "Blaine County Paleto Bay", new Vector3(-113.61559, 6468.9453, 31.626696)),
        };

        public static Dictionary<uint, Vector3> ATM = new Dictionary<uint, Vector3>
        {
            { 1, new Vector3(-2975.39,380.171,14.9982) },
            { 2, new Vector3(-3043.96,594.665,7.7368) },
            { 3, new Vector3(-3240.93,997.295,12.5455) },
            { 4, new Vector3(-2959.19,487.732,15.4639) },
            { 5, new Vector3(-2956.71,487.666,15.4639) },
            { 6, new Vector3(-3144.32,1127.68,20.8549) },
            { 7, new Vector3(-386.735,6045.95,31.5016) },
            { 8, new Vector3(-283.174,6226.16,31.4933) },
            { 9, new Vector3(-133.005,6366.48,31.4754) },
            { 10, new Vector3(-97.3712,6455.26,31.4659) },
            { 11, new Vector3(-95.5665,6457.03,31.4603) },
            { 12, new Vector3(174.211,6637.88,31.5731) },
            { 13, new Vector3(155.872,6642.79,31.6029) },
            { 14, new Vector3(1735.32,6410.47,35.0372) },
            { 15, new Vector3(1701.36,6426.35,32.6379) },
            { 16, new Vector3(1703.06,4933.44,42.0637) },
            { 17, new Vector3(1686.76,4815.77,42.0088) },
            { 18, new Vector3(1968.22,3743.63,32.3438) },
            { 19, new Vector3(1822.53,3683.07,34.2767) },
            { 20, new Vector3(540.419,2671.09,42.1565) },
            { 21, new Vector3(1171.42,2702.49,38.1755) },
            { 22, new Vector3(1172.49,2702.54,38.1747) },
            { 23, new Vector3(2564.63,2584.97,38.0831) },
            { 24, new Vector3(2558.4,389.48,108.623) },
            { 25, new Vector3(2558.84,350.924,108.622) },
            { 26, new Vector3(1077.7,-776.454,58.2397) },
            { 27, new Vector3(1153.73,-326.681,69.2051) },
            { 28, new Vector3(1166.81,-456.08,66.8141) },
            { 29, new Vector3(1138.29,-469.09,66.7271) },
            { 30, new Vector3(380.916,323.439,103.566) },
            { 31, new Vector3(236.495,219.685,106.287) },
            { 32, new Vector3(237.095,218.635,106.287) },
            { 33, new Vector3(237.248,217.737,106.287) },
            { 34, new Vector3(237.854,216.97,106.287) },
            { 35, new Vector3(238.24,216.002,106.287) },
            { 36, new Vector3(356.848,173.496,103.069) },
            { 37, new Vector3(-165.064,234.901,94.9219) },
            { 38, new Vector3(-165.158,232.632,94.9219) },
            { 39, new Vector3(-1827.34,785.072,138.303) },
            { 40, new Vector3(-2293.63,354.695,174.602) },
            { 41, new Vector3(-2294.65,356.563,174.602) },
            { 42, new Vector3(-2295.46,358.312,174.602) },
            { 43, new Vector3(-2072.45,-317.302,13.316) },
            { 44, new Vector3(-1205.73,-324.884,37.8581) },
            { 45, new Vector3(-1204.84,-326.44,37.834) },
            { 46, new Vector3(-1305.36,-706.284,25.3224) },
            { 49, new Vector3(-1282.52,-210.937,42.446) },
            { 50, new Vector3(-1286.18,-213.4,42.446) },
            { 51, new Vector3(-1289.2,-226.793,42.446) },
            { 52, new Vector3(-1285.65,-224.343,42.446) },
            { 53, new Vector3(-1109.75,-1690.76,4.37501) },
            { 54, new Vector3(-846.802,-340.142,38.6802) },
            { 55, new Vector3(-846.226,-341.402,38.6802) },
            { 56, new Vector3(-867.613,-186.135,37.8429) },
            { 57, new Vector3(-866.562,-187.747,37.8333) },
            { 58, new Vector3(-821.621,-1081.99,11.1324) },
            { 59, new Vector3(-57.6823,-92.5967,57.7789) },
            { 60, new Vector3(89.5119,2.39246,68.315) },
            { 61, new Vector3(-526.545,-1222.93,18.455) },
            { 62, new Vector3(228.185,338.479,105.563) },
            { 63, new Vector3(-537.823,-854.419,29.2902) },
            { 64, new Vector3(285.684,143.406,104.169) },
            { 65, new Vector3(527.227,-160.691,57.0894) },
            { 66, new Vector3(-717.543,-915.58,19.2156) },
            { 67, new Vector3(-303.314,-829.719,32.4173) },
            { 68, new Vector3(296.445,-894.158,29.2307) },
            { 69, new Vector3(-301.718,-830.081,32.4173) },
            { 70, new Vector3(295.763,-896.027,29.2172) },
            { 71, new Vector3(-258.764,-723.367,33.4654) },
            { 72, new Vector3(147.769,-1035.76,29.3429) },
            { 73, new Vector3(146.035,-1035.14,29.3448) },
            { 74, new Vector3(-256.161,-716.088,33.517) },
            { 75, new Vector3(-254.537,-692.413,33.6049) },
            { 76, new Vector3(119.138,-883.713,31.123) },
            { 77, new Vector3(114.354,-776.454,31.4181) },
            { 78, new Vector3(111.222,-775.377,31.4383) },
            { 79, new Vector3(5.1865,-919.813,29.5591) },
            { 80, new Vector3(24.4466,-946.04,29.3576) },
            { 81, new Vector3(-203.758,-861.348,30.2676) },
            { 82, new Vector3(-710.067,-818.993,23.7292) },
            { 83, new Vector3(-712.934,-819.018,23.7295) },
            { 84, new Vector3(33.2104,-1348.18,29.497) },
            { 85, new Vector3(-660.852,-854.069,24.4846) },
            { 86, new Vector3(130.109,-1292.7,29.2695) },
            { 87, new Vector3(129.66,-1291.97,29.2695) },
            { 88, new Vector3(129.215,-1291.15,29.2695) },
            { 89, new Vector3(-618.304,-708.927,30.0528) },
            { 90, new Vector3(-618.355,-706.806,30.0528) },
            { 91, new Vector3(-614.633,-704.746,31.236) },
            { 92, new Vector3(-611.721,-704.75,31.2359) },
            { 93, new Vector3(-56.637,-1752.26,29.421) },
            { 94, new Vector3(-1571.13,-547.326,34.9578) },
            { 95, new Vector3(-1570.13,-546.529,34.9527) },
            { 96, new Vector3(-1415.98,-211.923,46.5004) },
            { 97, new Vector3(-1430.08,-211.09,46.5004) },
            { 98, new Vector3(-1410.28,-98.6399,52.4354) },
            { 99, new Vector3(-1409.68,-100.472,52.3845) },
            { 100, new Vector3(289.017,-1256.86,29.4408) },
            { 101, new Vector3(288.75,-1282.28,29.64) },
            { 102, new Vector3(2682.9,3286.39,55.2411) },
            { 103, new Vector3(-1091.53,2708.51,18.9453) },
            { 104, new Vector3(-3040.73,593.046,7.90893) }
        };

        //[HandleExceptions]
        public override async void LoadAsync()
        {
            using var db = new RXContext();

            TransferDBContextValues(Banks, async bank =>
            {
                var mcb = await NAPI.Entity.CreateMCB(bank.Position, new Color(255, 140, 0), 0u, 2.4f, 2.4f, false, MarkerType.VerticalCylinder, true, 431, 25, bank.Id == 300 ? "Staatsbank" : "Bank");

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um auf dein Konto zuzugreifen!",
                    Color = "dgray",
                    Duration = 3500,
                    Title = bank.Id == 300 ? "Pacific Staatsbank" : bank.Name
                };

                mcb.ColShape.Action = async player => await OpenBank(player, bank.Id);
            });

            using (var context = new RXContext())
            {
                if (await context.BankAccounts.CountAsync() == 0 || (await context.BankAccounts.FirstOrDefaultAsync(x => x.Id == 1)).Name != "Staatskonto")
                {
                    var acc = await context.BankAccounts.FirstOrDefaultAsync(x => x.Id == 1);
                    if (acc != null) context.BankAccounts.Remove(acc);

                    var pl = await context.Players.FirstOrDefaultAsync(x => x.BankAccountId == 1);
                    if (pl != null) pl.BankAccountId = 0;

                    await context.BankAccounts.AddAsync(new DbBankAccount
                    {
                        Id = 1,
                        Name = "Staatskonto",
                        Balance = 50000000
                    });

                    await context.SaveChangesAsync();
                }
            }
            

            TransferDBContextValues(await db.BankAccounts.ToListAsync(), account =>
            {
                BankAccounts.Add(account);
            });

            TransferDBContextValues(ATM, async bank =>
            {
                var mcb = await NAPI.Entity.CreateMCB(bank.Value, new Color(255, 140, 0), 0u, 2.4f, 2.4f);//, true, MarkerType.VerticalCylinder, true, 1, 0, "ATM");

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um auf dein Konto zuzugreifen!",
                    Color = "white",
                    Duration = 3500,
                    Title = "ATM"
                };

                mcb.ColShape.Action = async player => await OpenBank(player, bank.Key);
            });
        }

        //[HandleExceptions]
        public override async Task OnTwoSecond()
        {
            using var db = new RXContext();

            List<DbBankAccount> copyBankAccounts = new List<DbBankAccount>();

            TransferDBContextValues(await db.BankAccounts.ToListAsync(), async account =>
            {
                using var _db = new RXContext();

                account.History = await _db.BankHistories.Where(x => x.AccountId == account.Id).ToListAsync();

                copyBankAccounts.Add(account);
            });

            BankAccounts = copyBankAccounts;
        }

        //[HandleExceptions]
        public async Task OpenBank(RXPlayer player, uint atmId)
        {
            try
            {
                if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

                var playerBank = player.BankAccount;
                if (playerBank == null) return;

                var bank = new RXBank
                {
                    Title = "Bank",
                    Balance = playerBank.Balance,
                    Money = player.Cash,
                    BankId = atmId,
                    DepositeeFeeMin = 0,
                    DepositFeeMax = 0,
                    DepositFeePer = 0,
                    WithdrawFeeMax = 0,
                    WithdrawFeePer = 0,
                };
                await this.Window.OpenWindow(player, bank);

                await NAPI.Task.RunAsync(() => player.SetData("atmId", atmId));

            } catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        [RemoteEvent]
        public async Task RqBankInfo(RXPlayer player)
        {
            if (player == null) return;
            await player.TriggerEventAsync("RsBankInfo", NAPI.Util.ToJson(player.BankAccount.History));
        }

        [RemoteEvent]
        public async Task WithdrawFrakMoney(RXPlayer player, int balance)
        {
            await FrakBankTransaction(player, 0, balance);
        }
        //[HandleExceptions]

        [RemoteEvent]
        public async Task DepositFrakMoney(RXPlayer player, int balance)
        {
            await FrakBankTransaction(player, balance, 0);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task WithdrawMoney(RXPlayer player, int balance)
        {
            await BankTransaction(player, 0, balance);
        }
        //[HandleExceptions]

        [RemoteEvent]
        public async Task DepositMoney(RXPlayer player, int balance)
        {
            await BankTransaction(player, balance, 0);
        }

        [RemoteEvent]
        public async Task bankingAppTransfer(RXPlayer player, string toPlayer, int amount)
        {
            if (player == null) return;

            if (amount > BankAppTransfer.bankingmaxcap) { return; }
            if (amount < BankAppTransfer.bankingmincap) { return; }




            await bankTransfer(player, amount, toPlayer, "Überweisung Handy");
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task bankTransfer(RXPlayer player, int amount, string target, string reason)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await NAPI.Task.RunReturnAsync(() => !player.HasData("atmId")) || await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("atmId") == 0)) return;

            uint atmId = await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("atmId"));

            var playerBank = player.BankAccount;
            if (playerBank == null) return;

            if (reason.Length > 16)
            {
                await player.SendNotify("Maximale Länge des Grundes betragen 16 Zeichen!", 3500, "red", "Konto");
            }

            RXPlayer targetPlayer = await PlayerController.FindPlayerByName(target);

            if (targetPlayer != null && targetPlayer != player && targetPlayer.BankAccount != null)
            {
                if (amount <= 0)
                {
                    await this.Window.CloseWindow(player);
                    return;
                }

                var targetBank = targetPlayer.BankAccount;
                if (targetBank == null) return;

                if (await playerBank.TakeBankMoney(amount, "Überweisung an " + await targetPlayer.GetNameAsync() + (reason.Length > 0 ? (" (" + reason + ")") : "") + $" (ATM{atmId})"))
                {
                    await targetBank.GiveBankMoney(amount, "Überweisung von " + await player.GetNameAsync() + (reason.Length > 0 ? (" (" + reason + ")") : "") + $" (ATM{atmId})");

                    DiscordModule.Logs.Add(new DiscordLog("Bank", (await player.GetNameAsync()) + " hat " + targetPlayer.GetNameAsync() + " " + amount.FormatMoneyNumber() + " überwiesen! (Grund: " + reason + ")" , DiscordModule.Bank));

                    await player.SendNotify("Du hast " + amount.FormatMoneyNumber() + " an " + await targetPlayer.GetNameAsync() + " überwiesen. (Grund: " + reason + ")", 3500, "dgray", "Konto");
                    await targetPlayer.SendNotify("Der Spieler " + await player.GetNameAsync() + " hat dir " + amount.FormatMoneyNumber() + " überwiesen. (Grund: " + reason + ")", 3500, "dgray", "Konto");

                    await this.Window.TriggerEvent(player, "success");

                    return;
                }
            }
            else
            {
                await player.SendNotify("Der Spieler wurde nicht gefunden!", 3500, "red", "Konto");
                await this.Window.TriggerEvent(player, "error");

                return;
            }
        }

        public async Task FrakBankTransaction(RXPlayer player, int einzahlen, int auszahlen)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;

            if (player.Team.Id == 0) return;


            if (!player.TeamMemberData.Bank)
            {
                await player.SendNotify("Du hast kein Zugriff auf die Fraktionsbank!");
                return;
            }

            var playerBank = player.Team.BankAccount;
            if (playerBank == null) return;

            if (auszahlen > 0 && auszahlen <= playerBank.Balance)
            {
                if (await playerBank.TakeBankMoney(auszahlen, $"Geldtransfer (FRAKBANK) - Auszahlung | " + await player.GetNameAsync()))
                {
                    await player.GiveMoney(auszahlen);
                    await player.SendNotify("Du hast " + auszahlen.FormatMoneyNumber() + " von dem Fraktionskonto abgehoben.", 3500, "green", "Konto");
                    DiscordModule.Logs.Add(new DiscordLog("Bank", (await player.GetNameAsync()) + " hat " + auszahlen.FormatMoneyNumber() + " von dem FraktionsKonto abgehoben", DiscordModule.Bank));

                    await this.Window.TriggerEvent(player, "success", auszahlen.FormatMoneyNumber() + " abgehoben.");

                    return;
                }
                else
                {
                    await player.SendNotify("Du besitzt nicht genügend Geld auf dem Konto.", 3500, "red", "Konto");
                    await this.Window.TriggerEvent(player, "error");

                    return;
                }
            }

            if (einzahlen > 0 && einzahlen <= player.Cash)
            {
                if (await player.TakeMoney(einzahlen))
                {
                    await playerBank.GiveBankMoney(einzahlen, $"Geldtransfer (FRAKBANK) - Einzahlung | " + await player.GetNameAsync());
                    await player.SendNotify("Du hast " + einzahlen.FormatMoneyNumber() + " auf dein Fraktionskonto eingezahlt.", 3500, "green", "Konto");
                    DiscordModule.Logs.Add(new DiscordLog("Bank", (await player.GetNameAsync()) + " hat " + einzahlen.FormatMoneyNumber() + " auf sein Fraktionskonto eingezahlt", DiscordModule.Bank));

                    await this.Window.TriggerEvent(player, "success", einzahlen.FormatMoneyNumber() + " eingezahlt.");

                    return;
                }
                else
                {
                    await player.SendNotify("Du besitzt nicht genügend Bargeld.", 3500, "red", "Konto");
                    await this.Window.TriggerEvent(player, "error");

                    return;
                }
            }
        }

        //[HandleExceptions]
        public async Task BankTransaction(RXPlayer player, int einzahlen, int auszahlen)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await NAPI.Task.RunReturnAsync(() => !player.HasData("atmId")) || await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("atmId") == 0)) return;

            var playerBank = player.BankAccount;
            if (playerBank == null) return;

            if (auszahlen > 0 && auszahlen <= playerBank.Balance)
            {
                if (await playerBank.TakeBankMoney(auszahlen, $"Geldtransfer (ATM) - Auszahlung"))
                {
                    await player.GiveMoney(auszahlen);
                    await player.SendNotify("Du hast " + auszahlen.FormatMoneyNumber() + " von deinem Konto abgehoben.", 3500, "green", "Konto");
                    DiscordModule.Logs.Add(new DiscordLog("Bank", (await player.GetNameAsync()) + " hat " + auszahlen.FormatMoneyNumber() + " von seinem Konto abgehoben", DiscordModule.Bank));


                    await this.Window.TriggerEvent(player, "success", auszahlen.FormatMoneyNumber() + " abgehoben.");

                    return;
                }
                else
                {
                    await player.SendNotify("Du besitzt nicht genügend Geld auf dem Konto.", 3500, "red", "Konto");
                    await this.Window.TriggerEvent(player, "error");

                    return;
                }
            }

            if (einzahlen > 0 && einzahlen <= player.Cash)
            {
                if (await player.TakeMoney(einzahlen))
                {
                    await playerBank.GiveBankMoney(einzahlen, $"Geldtransfer (ATM) - Einzahlung");
                    await player.SendNotify("Du hast " + einzahlen.FormatMoneyNumber() + " auf dein Konto eingezahlt.", 3500, "green", "Konto");
                    DiscordModule.Logs.Add(new DiscordLog("Bank", (await player.GetNameAsync()) + " hat " + einzahlen.FormatMoneyNumber() + " auf sein Konto eingezahlt", DiscordModule.Bank));

                    await this.Window.TriggerEvent(player, "success", einzahlen.FormatMoneyNumber() + " eingezahlt.");
                    
                    return;
                }
                else
                {
                    await player.SendNotify("Du besitzt nicht genügend Bargeld.", 3500, "red", "Konto");
                    await this.Window.TriggerEvent(player, "error");

                    return;
                }
            }
        }
    }
}
