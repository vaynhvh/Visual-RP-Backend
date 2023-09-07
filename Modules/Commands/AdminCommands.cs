using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Animations;
using Backend.Modules.Bank;
using Backend.Modules.Discord;
using Backend.Modules.Fishing;
using Backend.Modules.Gangwar;
using Backend.Modules.Inventory;
using Backend.Modules.Phone;
using Backend.Modules.Player;
using Backend.Modules.Rank;
using Backend.Modules.Tablet.Apps;
using Backend.Modules.Vehicle;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using MySqlConnector;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Metrics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Commands
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class AdminCommands : RXModule
    {
        public AdminCommands() : base("AdminCommands") { }

        //[HandleExceptions]
        [RemoteEvent]
        public void vanish(RXPlayer player, bool vanish)
        {
            if (player.Rank.Permission > 0)
                player.Invisible = vanish;
        }

        //[HandleExceptions]
        [RXCommand("nametags", 91)]
        public async Task nametags(RXPlayer player, string[] args)
        {
            await player.TriggerEventAsync("togglePlayerNametags");

            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " Hat die Nametags Aktiviert/Deaktivert ", DiscordModule.abuse));
        }

        [RXCommand("loadlschangar", 1)]
        public async Task loadlschangar(RXPlayer player, string[] args)
        {
            await player.TriggerEventAsync("loadlschangar");
        }

        [RXCommand("unloadlschangar", 1)]
        public async Task unloadlschangar(RXPlayer player, string[] args)
        {
            await player.TriggerEventAsync("unloadlschangar");
        }

        //[HandleExceptions]
        [RXCommand("aduty", 1)]
        public async Task aduty(RXPlayer player, string[] args)
        {
            await SupportApp.ToggleAduty(player);
        }
        [RXCommand("gduty", 97)]
        public async Task gduty(RXPlayer player, string[] args)
        {
            await player.TriggerEventAsync("updateGameDesignDuty", !player.InGduty);

            player.InGduty = !player.InGduty;


            if (player.InGduty)
            {

                await player.SendNotify("Du hast den GameDesignmodus betreten!", 3500, "white", "Game-Design");

            }
            else
            {
                await player.SendNotify("Du hast den GameDesignmodus verlassen!", 3500, "white", "Game-Design");
            }
        }
        

        //[HandleExceptions]
        [RXCommand("v", 91)]
        public async Task v(RXPlayer player, string[] args)
        {
            player.Invisible = !player.Invisible;

            await player.SendNotify("Unsichtbarkeit " + (player.Invisible ? "aktiviert!" : "deaktiviert!"), 3500, "red", "Administration");
        }

        //[HandleExceptions]
        [RXCommand("tpm", 98)]
        public async Task tpm(RXPlayer player, string[] args) => await player.TriggerEventAsync("TeleportToWaypoint");

        //[HandleExceptions]
        [RXCommand("askin", 99)]
        public async Task askin(RXPlayer player, string[] args)
        {
            if (int.TryParse(args[0], out var id))
            {
                await player.SetClothesAsync(1, 135, id);
                await player.SetClothesAsync(11, 287, id);
                await player.SetClothesAsync(8, 15, 0);
                await player.SetClothesAsync(3, 3, 0);
                await player.SetClothesAsync(4, 114, id);
                await player.SetClothesAsync(6, 78, id);
                await player.SetClothesAsync(5, 0, 0);
                await player.SetClothesAsync(2, 0, 0);
                await player.SetAccessoriesAsync(0, -1, 0);

                await player.SendNotify("Adminkleidung " + id + " gesetzt!", 3500, "red", "Administration");
            }
        }

        //[HandleExceptions]
        [RXCommand("setclothes", 99)]
        public async Task setclothes(RXPlayer player, string[] args)
        {
            if (int.TryParse(args[0], out var componentId) && int.TryParse(args[1], out var drawableId) && int.TryParse(args[2], out var textureId))
            {
                await player.SetClothesAsync(componentId, drawableId, textureId);

                await player.SendNotify("Kleidung gesetzt!", 3500, "red", "Administration");
            }
        }

        //[HandleExceptions]
        [RXCommand("setprop", 99)]
        public async Task setprop(RXPlayer player, string[] args)
        {
            if (int.TryParse(args[0], out var componentId) && int.TryParse(args[1], out var drawableId) && int.TryParse(args[2], out var textureId))
            {
                await player.SetAccessoriesAsync(componentId, drawableId, textureId);

                await player.SendNotify("Prop gesetzt!", 3500, "red", "Administration");
            }
        }

        [RXCommand("house", 91)]
        public async Task house(RXPlayer player, string[] args)
        {
            using var db = new RXContext();

            var houses = await db.Houses.ToListAsync();

            var house = houses.Find(x => x.id == int.Parse(args[0]));
            if (house == null)
            {
                await player.SendNotify("Dieses Haus existiert nicht!");
                return;
            }

            await player.SetPositionAsync(new Vector3(house.posX, house.posY, house.posZ));
        }

        [RXCommand("edithouse", 100)]
        public async Task edithouse(RXPlayer player, string[] args)
        {
            using var db = new RXContext();

            var houses = await db.Houses.ToListAsync();

            var house = houses.Find(x => x.id == int.Parse(args[0]));

            if (house == null)
            {
                await player.SendNotify("Dieses Haus existiert nicht!");
                return;
            }

            house.price = uint.Parse(args[1]);
            house.slots = uint.Parse(args[2]);
            house.weight = uint.Parse(args[3]);

            await db.SaveChangesAsync();
            await player.SendNotify("Haus gespeichert!");

        }
        //[HandleExceptions]
        [RXCommand("staatskonto", 100)]
        public async Task staatskonto(RXPlayer player, string[] args)
        {
            if (!player.IsLoggedIn) return;

            player.ResetData("atmId");

            var bankAccount = BankModule.BankAccounts.FirstOrDefault(x => x.Id == 1);
            if (bankAccount == null) return;

            var bank = new RXBank
            {
                Title = "Staatskonto",
                Balance = bankAccount.Balance,
                Money = player.Cash,
                BankId = 0,
            };

            await bankAccount.History.forEach(history =>
            {

            });

            await new RXWindow("Bank").OpenWindow(player, bank);
        }

        //[HandleExceptions]
        [RXCommand("reloadskin", 99)]
        public async Task reloadskin(RXPlayer player, string[] args)
        {
            await player.LoadCharacter();

            await player.SendNotify("Skin erfolgreich reloaded!", 3500, "red", "Administration");
        }

        [RXCommand("coord", 1)]
        public async Task coord(RXPlayer player, string[] args)
        {
            Vector3 pos = await player.GetPositionAsync();
            await player.SendNotify("Coords: " + pos.ToString().Replace(",", ".") + " Heading: " + await NAPI.Task.RunReturnAsync(() => player.Heading), 3500, "red", "Administration");
            RXLogger.Print("Coords: " + pos.X.ToString().Replace(",", ".") + "f, " + pos.Y.ToString().Replace(",", ".") + "f, " + pos.Z.ToString().Replace(",", ".") + "f, "+ await NAPI.Task.RunReturnAsync(() => player.Heading.ToString().Replace(",", ".")) + "f", LogType.INFO);
        }
        [RXCommand("setlivery", 98)]
        public async Task setlivery(RXPlayer player, string[] args)
        {
            var veh = await player.GetVehicleAsync();

            if (veh == null) return;

            veh.RXLivery = int.Parse(args[0]);
        }

        [RXCommand("sethunger", 98)]
        public async Task sethunger(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }
            target.Hunger = int.Parse(args[1]);

            await player.SendNotify("Hunger gestillt!");

            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat von " + target.Name + " das Essen gestillt! ", DiscordModule.abuse));
        }

        [RXCommand("setsport", 98)]
        public async Task setsport(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }
            target.Sport = int.Parse(args[1]);

            await player.SendNotify("Fitness ist wichtig!");

            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat von " + target.Name + " Das Traininig gesetzt! ", DiscordModule.abuse));
        }

        [RXCommand("setthirst", 98)]
        public async Task setthirst(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }

            target.Thirst = int.Parse(args[1]);

            await player.SendNotify("Durst gestillt!");

            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat von " + target.Name + " den Durst gestillt! ", DiscordModule.abuse));
        }


        //[HandleExceptions]
        [RXCommand("go", 91)]
        public async Task go(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }

            await player.SetPositionAsync(await target.GetPositionAsync());
            await player.SetDimensionAsync(await target.GetDimensionAsync());

            await player.SendNotify("Du wurdest zu " + (await target.GetNameAsync()) + " teleportiert!", 3500, "red", "Administration");
            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat sich zu " + target.Name + " Teleportet! ", DiscordModule.abuse));
        }

        [RXCommand("goveh", 91)]
        public async Task goveh(RXPlayer player, string[] args)
        {
            var target = VehicleController.FindVehicleById(args[0]);
            if (target == null)
            {
                await player.SendNotify("Dieses Fahrzeug existiert nicht!", 3500, "red", "Administration");
                return;
            }

            if (target.IsNull)
            {
                await player.SendNotify("Dieses Fahrzeug ist nicht ausgeparkt!", 3500, "red", "Administration");
                return;
            }

            await player.SetPositionAsync(await target.GetPositionAsync());

            await player.SendNotify("Du wurdest zu Fahrzeug " +  target.Id + " teleportiert!", 3500, "red", "Administration");
        }

        [RXCommand("bringveh", 91)]
        public async Task bringveh(RXPlayer player, string[] args)
        {
            var target = VehicleController.FindVehicleById(args[0]);
            if (target == null)
            {
                await player.SendNotify("Dieses Fahrzeug existiert nicht!", 3500, "red", "Administration");
                return;
            }

            if (target.IsNull)
            {
                await player.SendNotify("Dieses Fahrzeug ist nicht ausgeparkt!", 3500, "red", "Administration");
                return;
            }

            await target.SetPositionAsync(await player.GetPositionAsync());

            await player.SendNotify("Fahrzeug " + target.Id + " wurde zu dir teleportiert!", 3500, "red", "Administration");
        }


        [RXCommand("startEffect", 99)]
        public async Task startEffect(RXPlayer player, string[] args)
        {
            await player.TriggerEventAsync("startScreenEffect", args[0], int.Parse(args[1]), true); 
        }

        [RXCommand("setStress", 99)]
        public async Task setStress(RXPlayer player, string[] args)
        {
            player.Stress = int.Parse(args[0]);
            await player.SendNotify("Dein Stress-Level wurde geändert!");
        }

        [RXCommand("stopEffect", 99)]
        public async Task stopEffect(RXPlayer player, string[] args)
        {
            await player.TriggerEventAsync("stopScreenEffect", args[0]);
        }

        [RXCommand("addpaintballspawn", 99)]
        public async Task addpaintballspawn(RXPlayer player, string[] args)
        {

            using var db = new RXContext();

            uint mapid = uint.Parse(args[0]);

            Vector3 ppos = await player.GetPositionAsync();

            await db.PaintballSpawnpoints.AddAsync(new DbPaintballSpawnpoints() { MapId= mapid, Position = ppos.FromPos() });

            await db.SaveChangesAsync();
            await player.SendNotify("Spawnpoint wurde zur Map " + mapid + " hinzugefügt.");

        }


        [RXCommand("prop", 97)]
        public async Task plant(RXPlayer player, string[] args)
        {
            await NAPI.Task.RunReturnAsync(async () => NAPI.Object.CreateObject(NAPI.Util.GetHashKey(args[0]), player.Position.Subtract(new Vector3(0, 0, 1)), new Vector3(), 255, await player.GetDimensionAsync()));
        }

        //[HandleExceptions]
        [RXCommand("revive", 91)]
        public async Task revive(RXPlayer player, string[] args)
        {
            var target = player;

            if (args.Length > 0 && !string.IsNullOrEmpty(args[0]) && args[0] != "revive")
                target = await PlayerController.FindPlayerByStartsName(args[0]);

            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }
            await target.TriggerEventAsync("transitionFromBlurred", 400);

            target.DeathData.DeathTime = DateTime.Now;
            target.DeathData.IsDead = false;
            target.Coma = false;
            target.Freezed = false;
            target.Invincible = false;
            target.Invisible = false;
            target.Injured = false;
            target.Stress = 0;

            await NAPI.Task.RunAsync(() =>
            {
                if (target.DeathProp != null && target.DeathProp.Exists)
                    target.DeathProp.Delete();
            });

            await target.disableAllPlayerActions(false);
            await target.StopAnimationAsync();
            await target.SpawnAsync(await target.GetPositionAsync());

            await new RXWindow("Death").CloseWindow(target);

            await target.SendNotify("Du wurdest von " + player.Rank.Name + " " + (await player.GetNameAsync()) + " wiederbelebt!", 3500, "red", "Administration");
            await player.SendNotify("Du hast " + (await target.GetNameAsync()) + " wiederbelebt!", 3500, "red", "Administration");
            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat " + target.Name + " wiederbelebt! ", DiscordModule.abuse));

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == target.Id);
            if (dbPlayer == null) return;

            dbPlayer.Coma = false;
            dbPlayer.Stress = 0;
            dbPlayer.DeathStatus = false;
            dbPlayer.DeathTime = DateTime.Now;

            await db.SaveChangesAsync();
            target.Freezed = false;
            await target.disableAllPlayerActions(false);
        }

        //[HandleExceptions]
        [RXCommand("get", 91)]
        public async Task get(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }

            await target.SetPositionAsync(await player.GetPositionAsync());
            await target.SetDimensionAsync(await player.GetDimensionAsync());

            await target.SendNotify(player.Rank.Name + " " + (await player.GetNameAsync()) + " hat dich teleportiert!", 3500, "red", "Administration");
            await player.SendNotify("Du hast " + (await target.GetNameAsync()) + " zu dir teleportiert!", 3500, "red", "Administration");
            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat " + target.Name + " zu sich Teleportet! ", DiscordModule.abuse));

        }
        [RXCommand("dimension", 91)]
        public async Task dimension(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }

            await target.SetDimensionAsync(uint.Parse(args[1]));

            await target.SendNotify(player.Rank.Name + " " + (await player.GetNameAsync()) + " hat dich in die Dimension " + args[1] +" gesetzt!", 3500, "red", "Administration");
            await player.SendNotify("Du hast " + (await target.GetNameAsync()) + " in die Dimension " + args[1] + " gesetzt!", 3500, "red", "Administration");
        }

        [RXCommand("killer", 1)]
        public async Task killer(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }

            if (string.IsNullOrEmpty(target.LastKiller))
                await player.SendNotify("Dieser Spieler wurde in dieser Sitzung noch nicht von einem Spieler getötet.", 3500, "red", "Administration");
            else
                await player.SendNotify("Der Spieler " + await target.GetNameAsync() + " wurde als letztes von " + target.LastKiller + " getötet. Waffe: " + target.LastKillerWeapon, 3500, "red", "Administration");
        }

        [RXCommand("freeze", 91)]
        public async Task freeze(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }

            if (target.Freezed)
            {
                await player.SendNotify("Du hast den Spieler entfreezed.", 3500, "red", "Administration");

                DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat " + target.Name + " entfreezed! ", DiscordModule.abuse));

                target.Freezed = false;
            }
            else
            {
                await player.SendNotify("Du hast den Spieler gefreezed.", 3500, "red", "Administration");

                DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat " + target.Name + " gefreezed! ", DiscordModule.abuse));

                target.Freezed = true;
            }
        }

        //[HandleExceptions]
        [RXCommand("uncuff", 97)]
        public async Task uncuff(RXPlayer player, string[] args)
        {
            player.SetTied(false);

            await player.SendNotify("Du hast dich entfesselt!", 3500, "red", "Administration");
        }

        //[HandleExceptions]
        [RXCommand("setitem", 98)]
        public async Task setitem(RXPlayer player, string[] args)
        {
            string amountStr = args[2];
            string itemName = args[1];
            string targetName = args[0];

            if (int.TryParse(amountStr, out int amount))
            {
                var target = await PlayerController.FindPlayerByStartsName(targetName);
                if (target == null) return;

               
                var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == itemName);
                if (model == null)
                {
                    model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == int.Parse(itemName));
                    if (model == null)
                    {
                        return;
                    }
                }

                target.Container.AddItem(model, amount);

                await player.SendNotify("Item gegeben!", 3500, "red", "Administration");
                DiscordModule.Logs.Add(new DiscordLog("Abuse", (await player.GetNameAsync()) + " gibt " + target.Name + " " + amount + "x "+ model.Name + "!", DiscordModule.abuse));
                //  using var db = new RXContext();


                //await db.Logs.AddAsync(new DbLog { Message = player.Rank.Name + " " + await player.GetNameAsync() + " gibt " + targetName + " " + amount + "x " + model.Name + "!", Type = 1 });

                //    await db.SaveChangesAsync();


            }
        }

        [RXCommand("giveweapon", 99)]
        public async Task giveweapon(RXPlayer player, string[] args)
        {
            string amountStr = args[2];
            string itemName = args[1];
            string targetName = args[0];

            if (int.TryParse(amountStr, out int amount))
            {
                var target = await PlayerController.FindPlayerByStartsName(targetName);
                if (target == null) return;

                await target.AddWeaponToLoadout((WeaponHash)NAPI.Util.GetHashKey(itemName), true, amount);

                await player.SendNotify("Waffe gegeben!", 3500, "red", "Administration");
            }
        }

        [RXCommand("fishingspot", 98)]
        public async Task fishingspot(RXPlayer player, string[] args)
        {

            var ppos = await player.GetPositionAsync();

            using var db = new RXContext();

            var fishingspot = new DbFishing() { Heading = await player.GetHeadingAsync(), Position = ppos.FromPos(), InUse = false };

            await db.Fishing.AddAsync(fishingspot);
            await FishingModule.LoadFishingSpot(fishingspot);

            await db.SaveChangesAsync();

        }




        [RXCommand("creategw", 99)]
        public async Task creategw(RXPlayer player, string[] args)
        {

            var ppos = await player.GetPositionAsync();

            using var db = new RXContext();

            var gw = new DbGangwar((uint)GangwarModule.Gangwars.Count + 1, args[0], ppos.FromPos(), 2, float.Parse(args[1]), DateTime.Now);


            GangwarModule.Gangwars.Add(gw);
            await db.Gangwar.AddAsync(gw);
                await db.SaveChangesAsync();
                await player.SendNotify($"ID: {gw.Id} Gangwarzone wurde erstellt!", 3500, "red", "Administration");
            
        }


        [RXCommand("setgwpos", 99)]
        public async Task setgwpos(RXPlayer player, string[] args)
        {
            string gwid = args[0];

            if (int.TryParse(gwid, out int id))
            {
                using var db = new RXContext();
                var gangwar = GangwarModule.Gangwars.Find(x => x.Id == id);
                var dbgangwar = await db.Gangwar.FirstOrDefaultAsync(x => x.Id == id);
                if (gangwar == null) return;
                if (dbgangwar == null) return;

                var ppos = await player.GetPositionAsync();
                if (args[1] == "attacker")
                {
                    gangwar.AttackerPosition = ppos.FromPos();
                    dbgangwar.AttackerPosition = ppos.FromPos();

                }
                else if (args[1] == "defender")
                {
                    gangwar.DefenderPosition = ppos.FromPos();
                    dbgangwar.DefenderPosition = ppos.FromPos();
                }
                else if (args[1] == "attackerauspark")
                {
                    gangwar.AttackerAusparker = ppos.FromPos();
                    gangwar.AttackerAusparkerRotation = await player.GetHeadingAsync();
                    dbgangwar.AttackerAusparker = ppos.FromPos();
                    dbgangwar.AttackerAusparkerRotation = await player.GetHeadingAsync();
                }
                else if (args[1] == "attackerveh")
                {
                    gangwar.AttackerVehSpawn = ppos.FromPos();
                    gangwar.AttackerVehSpawnRotation = await player.GetHeadingAsync();
                    dbgangwar.AttackerVehSpawn = ppos.FromPos();
                    dbgangwar.AttackerVehSpawnRotation = await player.GetHeadingAsync();
                }
                else if (args[1] == "defenderauspark")
                {
                    gangwar.DefenderAusparker = ppos.FromPos();
                    gangwar.DefenderAusparkerRotation = await player.GetHeadingAsync();
                    dbgangwar.DefenderAusparker = ppos.FromPos();
                    dbgangwar.DefenderAusparkerRotation = await player.GetHeadingAsync();
                }
                else if (args[1] == "defenderveh")
                {
                    gangwar.DefenderVehSpawn = ppos.FromPos();
                    gangwar.DefenderVehSpawnRotation = await player.GetHeadingAsync();
                    dbgangwar.DefenderVehSpawn = ppos.FromPos();
                    dbgangwar.DefenderVehSpawnRotation = await player.GetHeadingAsync();
                }

                    await db.SaveChangesAsync();
                await player.SendNotify("Position wurde gesetzt!", 3500, "red", "Administration");
            }
        }



        [RXCommand("setgwflag", 99)]
        public async Task setgwflag(RXPlayer player, string[] args)
        {
            string gwid = args[0];

            if (int.TryParse(gwid, out int id))
            {
                using var db = new RXContext();

                var gangwar = GangwarModule.Gangwars.Find(x => x.Id == id);
                var dbgangwar = await db.Gangwar.FirstOrDefaultAsync(x => x.Id == id);
                if (gangwar == null) return;
                if (dbgangwar == null) return;
                if (gangwar == null) return;

                var ppos = await player.GetPositionAsync();

                if (args[1] == "1")
                {
                    gangwar.Flag1 = ppos.FromPos();
                    dbgangwar.Flag1 = ppos.FromPos();

                }
                else if (args[1] == "2")
                {
                    gangwar.Flag2 = ppos.FromPos();
                    dbgangwar.Flag2 = ppos.FromPos();

                }
                else
                {
                    gangwar.Flag3 = ppos.FromPos();
                    dbgangwar.Flag3 = ppos.FromPos();

                }
                await db.SaveChangesAsync();

                await player.SendNotify("Flaggen-Position wurde gesetzt!", 3500, "red", "Administration");
            }
        }


        [RXCommand("unban", 97)]
        public async Task unban(RXPlayer player, string[] args)
        {
            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Username == args[0]);
            if (dbPlayer == null)
            {
                await player.SendNotify("Der Spieler wurde nicht gefunden.", 3500, "red", "Administration");
                return;
            }
            else
            {
                string socialClubName = dbPlayer.SocialClubId;
                string socialClubId = dbPlayer.SocialClubNumber;
                string playerAddress = dbPlayer.IP;
                string hardwareId = dbPlayer.HWID;
                string clientHash = dbPlayer.ClientHash;

                await db.BlacklistedIdentifiers.Where(
                    x => x.Identifier == socialClubName ||
                    x.Identifier == socialClubId ||
                    x.Identifier == playerAddress ||
                    x.Identifier == hardwareId ||
                    x.Identifier == clientHash
                ).ForEachAsync(x => db.BlacklistedIdentifiers.Remove(x));

                await player.SendNotify("Der Spieler wurde erfolgreich entbannt.", 3500, "red", "Administration");
                DiscordModule.Logs.Add(new DiscordLog("Ban-Kick", (await player.GetNameAsync()) + " hat " + args[0] + " entbannt!", DiscordModule.BanKick));
            }

            await db.SaveChangesAsync();
        }

        [RXCommand("xcm", 97)]
        public async Task xcm(RXPlayer player, string[] args)
        {
            string socialClubName = "";
            string playerAddress = "";
            string socialClubId = "";
            string hardwareId = "";
            string clientHash = "";
            string targetName = "";
            int targetWarns = 0;
            int targetForumId = 0;
            string targetDiscordID = "";

            using var db = new RXContext();

            DbPlayer dbPlayer = null;

            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Username == args[0]);
                if (dbPlayer == null)
                {
                    await player.SendNotify("Bitte gebe den vollständigen Namen ein, der Spieler ist offline.", 3500, "red", "Administration");
                    return;
                }
                else
                {
                    socialClubName = dbPlayer.SocialClubId;
                    socialClubId = dbPlayer.SocialClubNumber;
                    playerAddress = dbPlayer.IP;
                    hardwareId = dbPlayer.HWID;
                    clientHash = dbPlayer.ClientHash;
                    targetName = args[0];
                    targetWarns = dbPlayer.Warns;
                    targetForumId = dbPlayer.ForumId;
                    targetDiscordID = dbPlayer.DiscordID;
                }
            }
            else
            {
                dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == target.Id);
                if (dbPlayer == null) return;

                socialClubName = await NAPI.Task.RunReturnAsync(() => target.SocialClubName);
                playerAddress = await NAPI.Task.RunReturnAsync(() => target.Address);
                socialClubId = await NAPI.Task.RunReturnAsync(() => target.SocialClubId.ToString());
                hardwareId = await NAPI.Task.RunReturnAsync(() => target.Serial);
                clientHash = target.ClientHash;
                targetName = dbPlayer.Username;
                targetWarns = target.Warns;
                targetForumId = target.ForumId;
                targetDiscordID = target.DiscordID;
            }

            if (player.Rank.Permission < target.Rank.Permission)
            {
                await player.SendNotify("Du kannst das nicht!", 3500, "red");
                return;
            }

            string reason = string.Join(" ", args, 1, args.Length - 1);

            await player.SendNotify("Der Spieler wurde erfolgreich gebannt.", 3500, "red", "Administration");
            await player.GivePTAPoints(targetName, targetDiscordID, $"Spieler mit der Begründung {reason} gebannt (+10 Punkte)", 10);

            if (target != null)
            {
                await target.SendNotify("Du wirst in wenigen Sekunden vom Gameserver gebannt: Grund: " + reason, 10000, "red", "Bann");
                await Task.Delay(2000);
                await target.KickAsync();
            }

            await SendForumKonversation(targetForumId, targetName, targetWarns, reason);

            RX.SendGlobalNotifyToAll(player.Rank.Name + " " + await player.GetNameAsync() + " hat " + targetName + " einen permanenten Communityausschluss erteilt!" + (reason == "" ? "" : " (Grund: " + reason + ")"), 8000, "red", Icon.Admin);

            DiscordModule.Logs.Add(new DiscordLog("Ban-Kick", (await player.GetNameAsync()) + " hat " + targetName + " gebannt! (Grund: " + reason + ")", DiscordModule.BanKick));

            await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = socialClubId });
            await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = playerAddress });
            await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = socialClubName });
            await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = hardwareId });
            await db.BlacklistedIdentifiers.AddAsync(new DbIdentifier { Identifier = clientHash });

            await db.SaveChangesAsync();
        }

        public static async Task SendForumKonversation(int forumid, string playername, int warncount, string banreason)
        {
            /*HttpWebRequest request = (HttpWebRequest)WebRequest.Create("https://sipkauirp.de/emailsender.php?password=GVMPSWANSLUTSCHERXDROFL&userid=" + forumid + "&" + "name=" + playername + "&" + "warns=" + warncount + "&" + "banreason=" + banreason);
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;

            using (HttpWebResponse response = (HttpWebResponse)await request.GetResponseAsync())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream))
            {
                await reader.ReadToEndAsync();
            }*/
        }

        //[HandleExceptions]
        [RXCommand("kick", 91)]
        public async Task kick(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }

            if (player.Rank.Permission < target.Rank.Permission)
            {
                await player.SendNotify("Du kannst das nicht!", 3500, "red");
                return;
            }

            string reason = string.Join(" ", args, 1, args.Length - 1);

            await target.SendNotify("Du wirst in wenigen Sekunden vom Gameserver gekickt: Grund: " + reason, 10000, "red", "Kick");
            DiscordModule.Logs.Add(new DiscordLog("Ban-Kick", (await player.GetNameAsync()) + " hat " + target.Name + " gekickt! (Grund: " + reason + ")", DiscordModule.BanKick));
            await Task.Delay(1500);
            await target.KickAsync();

            RX.SendGlobalNotifyToAll(player.Rank.Name + " " + await player.GetNameAsync() + " hat " + await target.GetNameAsync() + " vom Server gekickt!" + (reason == "" ? "" : " (Grund: " + reason + ")"), 8000, "red", Icon.Admin);
        }
        [RXCommand("setped", 99)]
        public void setped(RXPlayer player, string[] args)
        {

            NAPI.Task.Run(() => player.SetSkin((PedHash)NAPI.Util.GetHashKey(args[0])));

        }
        //[HandleExceptions]
        [RXCommand("info", 96)]
        public async void info(RXPlayer player, string[] args)
        {
            string message = string.Join(" ", args);

            if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message)) return;

            if (!Char.IsUpper(message.First()))
            {
                var firstChar = message.First();
                message = firstChar.ToString().ToUpper() + message.Substring(1);
            }

            if (message.Last() != '.' && message.Last() != '!' && message.Last() != '?') message += ".";

            RX.SendGlobalNotifyToAll(message, 8000, "orange", Icon.Admin);

            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync() + " Schreibt " + message + " In die Announce! "), DiscordModule.abuse));
        }

        [RXCommand("tc", 1)]
        public async Task tc(RXPlayer player, string[] args)
        {
            string message = string.Join(" ", args);

            if (string.IsNullOrEmpty(message) || string.IsNullOrWhiteSpace(message)) return;

            RX.SendNotifyToAllWhich(x => x.Rank != null && x.Rank.Permission > 80, message, 8000, "red", "Teamchat (" + await player.GetNameAsync() + ")");

            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync() + " Schreibt " + message + " In den Ingame Teamchat! "), DiscordModule.abuse));
        }


        //[HandleExceptions]
        [RXCommand("tp", 100)]
        public async Task tp(RXPlayer player, string[] args)
        {
            string xStr = args[0];
            string yStr = args[1];
            string zStr = args[2];

            if (int.TryParse(xStr, out var x) && int.TryParse(yStr, out var y) && int.TryParse(zStr, out var z))
            {
                await player.SetPositionAsync(new Vector3(x, y, z));
            }
        }

        //[HandleExceptions]
        [RXCommand("silentkick", 98)]
        public async Task silentkick(RXPlayer player, string[] args)
        {
            var target = await PlayerController.FindPlayerByStartsName(args[0], false);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                return;
            }

            if (player.Rank.Permission < target.Rank.Permission)
            {
                await player.SendNotify("Du kannst das nicht!", 3500, "red");
                return;
            }

            string reason = string.Join(" ", args, 1, args.Length - 1);

            await target.SendNotify("Du wirst in wenigen Sekunden vom Gameserver gekickt: Grund: " + reason, 3500, "red", "Kick");
            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat " + target.Name + " leise gekickt!" + (reason == "" ? "" : " (Grund: " + reason + ")")));
            await Task.Delay(5000);
            await target.KickAsync();
        }


        //[HandleExceptions]
        [RXCommand("setteam", 97)]
        public async Task setteam(RXPlayer player, string[] args)
        {
            if (uint.TryParse(args[1], out var teamId) && uint.TryParse(args[2], out var teamrank))
            {
                var target = await PlayerController.FindPlayerByStartsName(args[0]);
                if (target == null)
                {
                    await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                    return;
                }

                target.TeamId = teamId;
                target.Teamrank = teamrank;

                using var db = new RXContext();

                var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == target.Id);
                if (dbPlayer == null) return;

                dbPlayer.TeamId = teamId;
                dbPlayer.TeamrankId = teamrank;

                await db.SaveChangesAsync();

                await PhoneModule.requestApps(target);

                await player.SendNotify("Spieler in Fraktion gesetzt!", 3500, "red", "Administration");
                await target.SendNotify("Du wurdest von " + await player.GetNameAsync() + " in Fraktion " + (teamId == 0 ? "Zivilist" : target.Team.Name) + " gesetzt! Rang: " + teamrank, 3500, "red", "Administration");
                DiscordModule.Logs.Add(new DiscordLog("Set Fraktion", (await player.GetNameAsync()) + " hat " + target.Name + " in " + (teamId == 0 ? "Zivilist" : target.Team.Name) + " gesetzt! Rang: " + teamrank, DiscordModule.Setfrak));
            }
        }

        //[HandleExceptions]
        [RXCommand("setrank", 97)]
        public async Task setrank(RXPlayer player, string[] args)
        {
            if (uint.TryParse(args[1], out var rankId))
            {
                if (rankId >= player.Rank.Permission)
                {
                    await player.SendNotify("Du kannst nicht den gleichen oder einen höheren Rang als deinen vergeben!", 3500, "red", "Administration");
                    return;
                }

                var target = await PlayerController.FindPlayerByStartsName(args[0]);
                if (target == null)
                {
                    await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                    return;
                }

                var rank = RankModule.Ranks.FirstOrDefault(x => x.Permission == rankId);
                if (rank == null) return;

                target.Rank = rank;

                using var db = new RXContext();

                var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == target.Id);
                if (dbPlayer == null) return;

                dbPlayer.RankId = rankId;

                DiscordModule.Logs.Add(new DiscordLog("Test", (await player.GetNameAsync() + " hat " + target.Name + " Den Team Rank " + rankId + " gesetzt! "), DiscordModule.abuse));

                await db.SaveChangesAsync();
            }
        }

        //[HandleExceptions]
        [RXCommand("veh", 96)]
        public async Task veh(RXPlayer player, string[] args)
        {
            await NAPI.Task.RunAsync(async () =>
            {
                uint hash = NAPI.Util.GetHashKey(args[0]);

                RXVehicle vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(hash, player.Position, 0, 0, 0);//await MP.Vehicles.NewAsync((int)hash, await player.Player.GetPositionAsync());
                vehicle.NumberPlate = player.Rank.Permission.ToString();
                vehicle.SetSharedData("engineStatus", true);
                vehicle.SetSharedData("lockedStatus", false);

                await Task.Delay(100);

                await player.SetIntoVehicleAsync(vehicle, 0);
                await player.SendNotify("Das Fahrzeug wurde erfolgreich gespawnt!", 3500, "red", "Administration");
            });

            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat sich ein Auto gespawnt mit dem command /veh! ", DiscordModule.abuse));

            await Task.Delay(1000);

            await player.TriggerEventAsync("disableVehicleRadio");
        }

        //[HandleExceptions]
        [RXCommand("veh2", 96)]
        public async Task veh2(RXPlayer player, string[] args)
        {
            await NAPI.Task.RunAsync(async () =>
            {
                uint hash = NAPI.Util.GetHashKey(args[0]);

                RXVehicle vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(hash, player.Position, 0, 0, 0);//await MP.Vehicles.NewAsync((int)hash, await player.Player.GetPositionAsync());
                vehicle.NumberPlate = player.Rank.Permission.ToString();
                vehicle.SetSharedData("engineStatus", true);
                vehicle.SetSharedData("lockedStatus", false);

                var model = VehicleModelModule.VehicleModels.FirstOrDefault(x => x.Hash.ToLower() == args[0].ToLower());
                if (model != null) vehicle.ModelData = model;

                await Task.Delay(100);

                await player.SetIntoVehicleAsync(vehicle, 0);
                await player.SendNotify("Das Fahrzeug wurde erfolgreich gespawnt!", 3500, "red", "Administration");
            });

            DiscordModule.Logs.Add(new DiscordLog("Command", (await player.GetNameAsync()) + " hat sich ein Auto gespawnt mit dem command /veh2! ", DiscordModule.abuse));

            await Task.Delay(1000);

            await player.TriggerEventAsync("disableVehicleRadio");
        }

        //[HandleExceptions]
        [RXCommand("parkveh", 99)]
        public async Task parkveh(RXPlayer player, string[] args)
        {
            await NAPI.Task.RunAsync(() =>
            {
                if (!player.IsInVehicle) return;

                var vehicle = (RXVehicle)player.Vehicle;

                if (vehicle.ModelData != null)
                {
                    using var db = new RXContext();

                    var dbVehicle = db.Vehicles.FirstOrDefault(x => x.Id == vehicle.Id);
                    if (dbVehicle == null) return;

                    dbVehicle.Stored = true;

                    db.SaveChanges();
                }

                vehicle.Occupants.ForEach(x => ((RXPlayer)x).WarpOutOfVehicle());

                vehicle.Delete();
            });

            await player.SendNotify("Fahrzeug wurde eingeparkt!", 3500, "green");
        }

        //[HandleExceptions]
        [RXCommand("dv", 91)]
        public async Task dv(RXPlayer player, string[] args)
        {
            await NAPI.Task.RunAsync(() =>
            {
                if (!player.IsInVehicle) return;

                var vehicle = (RXVehicle)player.Vehicle;

                vehicle.Occupants.ForEach(x => ((RXPlayer)x).WarpOutOfVehicle());

                vehicle.Delete();
            });

            await player.SendNotify("Fahrzeug wurde gelöscht!", 3500, "green");
        }
        public static void Execute(string query)
        {
            if (query == "") return;
            try
            {
                using (var conn = new MySqlConnection(Configuration.ConnectionString))
                using (var cmd = conn.CreateCommand())
                {
                    conn.Open();
                    cmd.CommandText = query;
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        public DataTable FillTable(string tableName)
        {
            DataTable table = new DataTable();

            using (MySqlConnection connection = new MySqlConnection(Configuration.GameDesignConnectionString))
            {
                connection.Open();
                // Select * is not a good thing, but in this cases is is very usefull to make the code dynamic/reusable 
                // We get the tabel layout for our DataTable
                string query = $"SELECT * FROM " + tableName;
                using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, connection))
                {
                    adapter.Fill(table);
                };
            }

            return table;
        }


        [RXCommand("restart", 99)]
        public async Task restart(RXPlayer player, string[] args)
        {
            //dttableNew = dttableOld.Clone();

            //        foreach (DataRow drtableOld in dttableOld.Rows)
            //        {
            //         if (/*put some Condition */)
            //           {
            //         dtTableNew.ImportRow(drtableOld);
            //           }
            //          }

            RX.SendGlobalNotifyToAll("Der Server wird in 10 Sekunden neugestartet! (Grund: Dev-Restart)", 10000, "darkred", Icon.Dev);
            await Task.Delay(10000);

            System.Environment.Exit(1);
        }

        [RXCommand("syncdb", 99)]
        public async Task syncdb(RXPlayer player, string[] args)
        {
            //dttableNew = dttableOld.Clone();

            //        foreach (DataRow drtableOld in dttableOld.Rows)
            //        {
            //         if (/*put some Condition */)
            //           {
            //         dtTableNew.ImportRow(drtableOld);
            //           }
            //          }
             
            string[] synctables = new string[] { "workstations", "itemexport", "itemexport_items", "farming", "farmingpos", "farmingprocess", "item_models", "vehiclemodel", "vehicleshop", "vehicleshopoffers", "vehicleshopspawns", "clothes_female", "clothes_male", "props_female", "props_male" };

            foreach (string table in synctables)
            {

                Execute("DELETE FROM " + table);

                DataTable newTable = FillTable(table);
                using var connection = new MySqlConnection(Configuration.ConnectionString + ";AllowLoadLocalInfile=True");
                await connection.OpenAsync();

                // bulk copy the data
                var bulkCopy = new MySqlBulkCopy(connection);
                bulkCopy.DestinationTableName = table;
                await bulkCopy.WriteToServerAsync(newTable);

                await player.SendNotify("Datenbank Table " + table + " wird synchronisiert!", 3500, "green");
            }
            RX.SendGlobalNotifyToAll("Der Server wird in 10 Sekunden neugestartet! (Grund: Datenbank-Gamedesign Sync)", 10000, "darkred", Icon.Dev);
            await Task.Delay(10000);

            System.Environment.Exit(1);
        }
        private List<MySqlBulkCopyColumnMapping> GetMySqlColumnMapping(DataTable dataTable)
        {
            List<MySqlBulkCopyColumnMapping> colMappings = new List<MySqlBulkCopyColumnMapping>();
            int i = 0;
            foreach (DataColumn col in dataTable.Columns)
            {
                colMappings.Add(new MySqlBulkCopyColumnMapping(i, col.ColumnName));
                i++;
            }
            return colMappings;
        }

        //[HandleExceptions]
        [RXCommand("parkall", 99)]
        public async Task parkall(RXPlayer player, string[] args)
        {
            await NAPI.Task.RunAsync(() =>
            {
                using var db = new RXContext();

                db.Vehicles.forEachDefault(x =>
                {
                    x.Stored = true;

                    if (x.GarageId == 0) x.GarageId = 2;
                });

                db.SaveChanges();

                var list = VehicleController.GetValidVehicles();

                list.forEachAlternative(x =>
                {
                    x.Occupants.ForEach(x => ((RXPlayer)x).WarpOutOfVehicle());

                    x.Delete();
                });
            });

            RX.SendGlobalNotifyToAll("Es wurden alle Fahrzeuge in die letzte Garage eingeparkt.", 8000, "orange", Icon.Dev);

            DiscordModule.Logs.Add(new DiscordLog("Fahrzeug", (await player.GetNameAsync()) + " hat Alle Autos Eingeparkt! ", DiscordModule.abuse));

            await player.SendNotify("Fahrzeuge wurden eingeparkt!", 3500, "green");
        }

        //[HandleExceptions]
        [RXCommand("dvall", 99)]
        public async Task dvall(RXPlayer player, string[] args)
        {
            await NAPI.Task.RunAsync(() =>
            {
                using var db = new RXContext();

                var list = VehicleController.GetVehicles();

                list.forEachAlternative(x =>
                {
                    x.Occupants.ForEach(x => ((RXPlayer)x).WarpOutOfVehicle());

                    x.Delete();
                });
            });

            DiscordModule.Logs.Add(new DiscordLog("Fahrzeug", (await player.GetNameAsync()) + " hat Alle Autos gelöscht! ", DiscordModule.abuse));

            await player.SendNotify("Fahrzeuge wurden gelöscht!", 3500, "green");
        }

        //[HandleExceptions]
        [RXCommand("dvradius", 91)]
        public async Task dvradius(RXPlayer player, string[] args)
        {
            if (int.TryParse(args[0], out var radius))
            {
                await NAPI.Task.RunAsync(() =>
                {
                    var list = VehicleController.GetVehicles().Where(x => x.Position.DistanceTo(player.Position) <= radius).ToList();

                    list.forEachAlternative(x =>
                    {
                        x.Occupants.ForEach(x => ((RXPlayer)x).WarpOutOfVehicle());

                        x.Delete();
                    });
                });
            }

            await player.SendNotify("Fahrzeuge im Radius wurden gelöscht!", 3500, "green");
        }

        //[HandleExceptions]
        [RXCommand("repair", 100)]
        public async Task repair(RXPlayer player, string[] args)
        {
            if (await player.GetIsInVehicleAsync())
            {
                await NAPI.Task.RunAsync(() =>
                {
                    if (player.IsInVehicle && player.Vehicle != null) player.Vehicle.Repair();
                });

                await player.SendNotify("Fahrzeug repariert!", 3500, "green");
            }
        }

        //[HandleExceptions]
        [RXCommand("anim", 99)]
        public async Task anim(RXPlayer player, string[] args)
        {
            string animDict = args[0];
            string animName = args[1];
            string animFlag = args[2];

            if (!uint.TryParse(animFlag, out var flag)) return;

            if (!AnimationModule.animFlagDic.ContainsKey(flag) || AnimationModule.animFlagDic[flag] == null) return;

            await player.PlayAnimationAsync((int)flag, animDict, animName);
        }

        //[HandleExceptions]
        [RXCommand("parkradius", 91)]
        public async Task parkradius(RXPlayer player, string[] args)
        {
            if (int.TryParse(args[0], out var radius))
            {
                await NAPI.Task.RunAsync(() =>
                {
                    using var db = new RXContext();

                    var list = VehicleController.GetValidVehicles().Where(x => x.Position.DistanceTo(player.Position) <= radius).ToList();

                    list.forEachAlternative(x =>
                    {
                        if (x.ModelData != null)
                        {
                            var dbVehicle = db.Vehicles.FirstOrDefault(x => x.Id == x.Id);
                            if (dbVehicle != null) dbVehicle.Stored = true;
                        }

                        x.Occupants.ForEach(x => ((RXPlayer)x).WarpOutOfVehicle());

                        x.Delete();
                    });

                    db.SaveChanges();
                });
            }

            await player.SendNotify("Fahrzeuge im Radius wurden eingeparkt!", 3500, "green");
        }

        [RXCommand("duty", 1)]
        public async Task duty(RXPlayer player, string[] args)
        {
            if (player.Team.HasDuty)
            {
                player.InDuty = !player.InDuty;
                await player.TriggerEventAsync(PlayerDatas.DutyEvent, player.InDuty);
                await player.SendNotify("Du hast deinen Dienst getoggelt!");
            }
        }

        [RXCommand("speed", 100)]
        public async Task speed(RXPlayer player, string[] args)
        {
            if (player == null || !player.IsInVehicle) return;

            if (!int.TryParse(args[0], out int x)) return;

            RXVehicle rxVeh = await player.GetVehicleAsync();
            if (rxVeh == null) return;

            if (rxVeh.ModelData == null)
            {
                await player.SendNotify("Speed konnte nicht Gesetzt werden!");
                return;
            }

            rxVeh.ModelData.Multiplier = x;
            await player.SendNotify($"Du hast den Speed auf {x}x gestellt");
            DiscordModule.Logs.Add(new DiscordLog("Command", $"{(await player.GetNameAsync())} hat sein Fahrzeug {x}x schneller gemacht", DiscordModule.abuse));
        }

        [RXCommand("givemoney", 100)]
        public async Task GiveMoney(RXPlayer player, string[] args)
        {
            if (player == null) return;

            var target = await PlayerController.FindPlayerByStartsName(args[0]);
            if (target == null)
            {
                await player.SendNotify("Der Spieler ist nicht online!", 3500, "red", "Administration");
                DiscordModule.Logs.Add(new DiscordLog("Command", $"{await player.GetNameAsync()} hat versucht dem Spieler {args[0]} {args[1]}$ zu geben! (Player not Found)", DiscordModule.abuse));
                return;
            }

            if (int.TryParse(args[1], out int money))
            {
                await target.GiveMoney(money);
                await player.SendNotify($"Du hast dem Spieler {await target.GetNameAsync()} {money}$ gegeben!");
                DiscordModule.Logs.Add(new DiscordLog("Command", $"{await player.GetNameAsync()} hat dem Spieler {await target.GetNameAsync()} {money}$ gegeben!", DiscordModule.abuse));
            } else
            {
                DiscordModule.Logs.Add(new DiscordLog("Command", $"{await player.GetNameAsync()} hat versucht dem Spieler {await target.GetNameAsync()} {money}$ zu geben! (Money is not an Integer)", DiscordModule.abuse));
            }
        }
    }
}
