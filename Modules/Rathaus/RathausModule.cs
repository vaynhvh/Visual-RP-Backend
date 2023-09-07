using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Garage;
using Backend.Modules.Native;
using Backend.Modules.Phone;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Rathaus
{
    class RathausModule : RXModule
    {
        public RathausModule() : base("Rathaus") { }

        public static GTANetworkAPI.Vector3 ChangeNumberPoint = new GTANetworkAPI.Vector3(-549.2355f, -190.101f, 38.22352f);
        public static GTANetworkAPI.Vector3 ChangeNamePoint = new GTANetworkAPI.Vector3(-552.025f, -189.9765f, 38.234802f);

        public override async void LoadAsync()
        {
            new NPC(PedHash.Airhostess01SFY, ChangeNumberPoint, -127.53998f, 0u);
            var mcb = await NAPI.Entity.CreateMCB(ChangeNumberPoint, new Color(255, 140, 0), 0u, 2.4f, 2.4f, false, MarkerType.UpsideDownCone);

            mcb.ColShape.Message = new RXMessage
            {
                Text = "Benutze E um deine Telefonnummer zu ändern!",
                Color = "olive",
                Duration = 3500,
                Title = "LOS SANTOS GOVERNMENT"
            };

            mcb.ColShape.Action = async player => await OpenChangeNumber(player);

            new NPC(PedHash.Airhostess01SFY, ChangeNamePoint, -157.00146f, 0u);
            var mcb2 = await NAPI.Entity.CreateMCB(ChangeNamePoint, new Color(255, 140, 0), 0u, 2.4f, 2.4f, false, MarkerType.UpsideDownCone);

            mcb2.ColShape.Message = new RXMessage
            {
                Text = "Benutze E um deinen Namen zu ändern!",
                Color = "olive",
                Duration = 3500,
                Title = "LOS SANTOS GOVERNMENT"
            };

            mcb2.ColShape.Action = async player => await OpenChangeName(player);
        }
        public async Task OpenChangeName(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;


            object confirmationBoxObject = new
            {
                t = "Gebe nun einen neuen Namen an",
                e = "ChangePlayerName",
            };

            var confirmation = new RXWindow("Input");

            await confirmation.OpenWindow(player, confirmationBoxObject);

        }

        public async Task OpenChangeNumber(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;


            var window = new RXWindow("ChangePhoneNumber");

            await window.OpenWindow(player);
       
        }
        [RemoteEvent]
        public async Task CheckPhoneNumber(RXPlayer player, uint phoneNumber)
        {
            if (!player.CanInteract()) return;
            using var db = new RXContext();

            foreach (var tplayer in await db.Players.ToListAsync())
            {
                if (tplayer.Phone == phoneNumber)
                {
                    await player.SendNotify("Diese Nummer ist bereits vergeben!");
                    return;
                }
            }

            await player.SendNotify("Diese Nummer ist verfügbar!");

        }

        [RemoteEvent]
        public async Task ChangePhoneNumber(RXPlayer player, uint phoneNumber)
        {
            if (!player.CanInteract()) return;


            using var db = new RXContext();

            foreach (var tplayer in await db.Players.ToListAsync())
            {
                if (tplayer.Phone == phoneNumber)
                {
                    await player.SendNotify("Diese Nummer ist bereits vergeben!");
                    return;
                }
            }


            int price = 0;

            if (phoneNumber.ToString().ToCharArray().Length == 4)
            {
                price = 700000;
            } else if (phoneNumber.ToString().ToCharArray().Length == 5)
            {
                price = 400000;
            } else
            {
                price = 250000;
            }
            if (!await player.BankAccount.TakeBankMoney(price, "Telefonnummer Änderung"))
            {
                await player.SendNotify($"Du hast nicht genug Geld auf dem Konto!");
                return;
            }
            player.Phone = phoneNumber;
            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);

            dbPlayer.Phone = phoneNumber;
            await db.SaveChangesAsync();
            await player.SendNotify("Deine Nummer wurde erfolgreich geändert!");

        }

        [RemoteEvent]
        public async Task changePhoneNumberRandom(RXPlayer player, string lol, string lolo)
        {
            if (!player.CanInteract()) return;

            var number = await PhoneModule.generateRandomPhonenumber(player);
            if (!await player.BankAccount.TakeBankMoney(150000, $"Telefonnummer Änderung - {number}"))
            {
                await player.SendNotify($"Du hast nicht genug Geld auf dem Konto!");
                return;
            }

            player.Phone = number;
            using var db = new RXContext();
            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);

            dbPlayer.Phone = number;
            await db.SaveChangesAsync();
            await player.SendNotify("Deine Nummer wurde erfolgreich geändert!");

        }

        [RemoteEvent]
        public async Task ChangePlayerName(RXPlayer player, string newname)
        {
            if (!player.CanInteract()) return;

            if (!newname.Contains("_"))
            {
                await player.SendNotify("Dein Name muss ein _ beinhalten!");
                return;
            }
            else if (newname.Length > 32)
            {
                await player.SendNotify("Dein Name muss kürzer als 32 Zeichen sein!");
                return;
            }
            else if (await newname.ContainsSymbols())
            {
                await player.SendNotify("Dein Name darf keine Sonderzeichen enthalten!");
                return;

            }

            if ((newname.Split('l').Count() - 1) > 2 && (newname.Split('I').Count() - 1) > 2)
            {
                await player.SendNotify("Es gab ein Problem mit deinem Namen!");
                return;
            }

            var split = newname.Split("_");
            if (split[0].Length < 3 || split[1].Length < 3)
            {
                await player.SendNotify("Name zu kurz!");
                return;
            }


            int kosten = player.Level * 100000;
            if (!await player.BankAccount.TakeBankMoney(kosten, $"Namensänderung - {newname}"))
            {
                await player.SendNotify($"Du hast nicht genug Geld auf dem Konto!");
                return;
            }

            using var db = new RXContext();
            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
            var dbBankAccount = await db.BankAccounts.FirstOrDefaultAsync(x => x.Name == dbPlayer.Username);
            NAPI.Task.Run(() => player.Name = newname);
            dbBankAccount.Name = newname;
            dbPlayer.Username = newname;
            await db.SaveChangesAsync();
            await player.SendNotify("Dein Name wurde erfolgreich geändert!");
        }

    }
}
