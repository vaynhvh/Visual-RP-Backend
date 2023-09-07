using Backend.Controllers;
using Backend.Models;
using Backend.MySql;
using Backend.MySql.Models;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Phone.Apps
{
    public class CryptoAppObject
    {
        public string walletAddress { get; set; }
        public double value { get; set; }

    }
    public class CryptoMarketOffer
    {
        [JsonProperty(PropertyName = "i")]
        public int Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public double Coins { get; set; }
        [JsonProperty(PropertyName = "v")]
        public double Value { get; set; }
        [JsonProperty(PropertyName = "t")]
        public string Datum { get; set; }
        [JsonProperty(PropertyName = "isOwn")]
        public bool isOwn { get; set; }
    }

    class CryptoApp : RXModule
    {
        public CryptoApp() : base("CryptoApp", new RXWindow("Phone")) { }

        [RemoteEvent]
        public async Task RqCryptoWallet(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            var crypto = new CryptoAppObject() { walletAddress = player.WalletAdress, value = player.WalletValue };

            await player.TriggerEventAsync("RsCryptoWallet", NAPI.Util.ToJson(crypto));
        }


        [RemoteEvent]
        public async Task RqCryptoMarketOffers(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            var crypto = new List<CryptoMarketOffer>();

            crypto.Add(new CryptoMarketOffer() { Id = 1, Coins = 5, Datum = "Jetzt", Value = 3000, isOwn = false });


            using var db = new RXContext();
            var crpytoOther = await db.CryptoMarktOffers.ToListAsync();

            foreach (var crypt in crpytoOther)
            {
                crypto.Add(new CryptoMarketOffer() { Id = (int)crypt.Id, Coins = crypt.Coins, Datum = crypt.Datum, isOwn = crypt.isOwn, Value = crypt.Value});
            }

            await player.TriggerEventAsync("RsCryptoMarketOffers", NAPI.Util.ToJson(crypto));
        }

        [RemoteEvent]
        public async Task TransferCrypto(RXPlayer player, string wallet, double amount)
        {
            if (!player.CanInteract()) return;

            if (player.WalletValue <= amount)
            {
                await player.SendNotify("Du hast nicht genug AvoCoins!");
                await player.TriggerEventAsync("RsTransferCrypto", false);
                return;
            }

            var target = await PlayerController.FindPlayerByWallet(wallet);

            if (target == null)
            {
                await player.SendNotify("Der Besitzer der gefundenen Wallet ist nicht aktiv oder diese Wallet existiert nicht!");
                await player.TriggerEventAsync("RsTransferCrypto", false);
                return;
            }


            player.WalletValue -= amount;
            target.WalletValue += amount;

            await player.SendNotify("Cryptotransaktion an " + wallet + " ausgegangen!");
            await target.SendNotify("Cryptotransaktion von " + player.WalletAdress + " eingegangen!");

            await player.TriggerEventAsync("RsTransferCrypto", true);
        }


        [RemoteEvent]
        public async Task CreateCryptoOffer(RXPlayer player, double amount, double price)
        {
            if (!player.CanInteract()) return;

            if (player.WalletValue <= amount)
            {
                await player.SendNotify("Du hast nicht genug AvoCoins!");
                return;
            }

            player.WalletValue -= amount;
            
            using var db = new RXContext();

            await db.CryptoMarktOffers.AddAsync(new DbCryptoMarktOffers() { Coins = (double)amount, Value = (double)price, isOwn = true, PlayerId = player.Id, Datum = DateTime.Now.ToString("dd\\/MM\\/yyyy h\\:mm") });

            await db.SaveChangesAsync();
            await player.SendNotify("Angebot erstellt!");

            await RqCryptoMarketOffers(player);


        }


        private static Random random = new Random();

        public static string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXY#Z0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }


        [RemoteEvent]
        public async Task CreateWallet(RXPlayer player)
        {
            if (!player.CanInteract()) return;

            if (!string.IsNullOrEmpty(player.WalletAdress))
            {
                await player.SendNotify("Du hast bereits eine Wallet!");
                return;
            }

            player.WalletAdress = RandomString(23);
            player.WalletValue = 0.0;
            var crypto = new CryptoAppObject() { walletAddress = player.WalletAdress, value = player.WalletValue };

            await player.TriggerEventAsync("RsCryptoWallet", NAPI.Util.ToJson(crypto));
        }



        [RemoteEvent]
        public async Task BuyCryptoOffer(RXPlayer player, uint id, uint amount)
        {
            if (!player.CanInteract()) return;


            var crypto = new List<CryptoMarketOffer>();

            crypto.Add(new CryptoMarketOffer() { Id = 1, Coins = 5, Datum = "Jetzt", Value = 3000, isOwn = false });


            using var db = new RXContext();
            var crpytoOther = await db.CryptoMarktOffers.ToListAsync();

            foreach (var crypt in crpytoOther)
            {
                crypto.Add(new CryptoMarketOffer() { Id = (int)crypt.Id, Coins = crypt.Coins, Datum = crypt.Datum, isOwn = crypt.isOwn, Value = crypt.Value });
            }

            var target = crypto.Find(x => x.Id == id);

            if (target == null) return;

            var price = amount * target.Value / target.Coins;

            if (await player.TakeMoney((int)price))
            {
                player.WalletValue += amount;
                target.Coins -= amount;
                await player.SendNotify("Du hast " + amount + " AvoCoins für " + price + "$ gekauft");

            } else
            {
                await player.SendNotify("Du hast nicht so viel Geld!");

            }
            await RqCryptoWallet(player);

        }


    }
}
