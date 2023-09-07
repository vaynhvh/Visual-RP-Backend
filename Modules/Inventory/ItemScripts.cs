using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Bank;
using Backend.Modules.Faction;
using Backend.Modules.Farming;
using Backend.Modules.Fun;
using Backend.Modules.House;
using Backend.Modules.Injury;
using Backend.Modules.Meth;
using Backend.Modules.Scenarios;
using Backend.Modules.Vehicle;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using GTANetworkMethods;
using Microsoft.EntityFrameworkCore;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Task = System.Threading.Tasks.Task;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Inventory
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class ItemScripts : RXModule
    {
        public ItemScripts() : base("ItemScripts") { }

        


        //[HandleExceptions]
        public async override Task PressedE(RXPlayer player)
        {
            if (player.IsLoggedIn && player.DraggingItem)
            {
                player.DraggingItem = false;

                await player.disableAllPlayerActions(false);
                await player.StopAnimationAsync();
               

                await player.SendNotify("Du hast die Aktion abgebrochen!");

                await player.StopProgressbar();

                await Task.Delay(player.DraggingTime);

                lock (player) if (!RX.PlayerExists(player)) return;

                player.IsTaskAllowed = true;
            }
        }

        //[HandleExceptions]
        public static async Task<bool> RunScript(RXPlayer player, int slot, string script)
        {
            if (!player.IsLoggedIn) return await Task.FromResult(false);

            if (string.IsNullOrEmpty(script) || script == "" || script == " ")
            {
                return await Task.FromResult(false);
            }

            IEnumerable<MethodInfo> scripts = AppDomain.CurrentDomain.GetAssemblies()
               .SelectMany(x => x.GetTypes())
               .Where(x => x.IsClass)
               .SelectMany(x => x.GetMethods())
               .Where(x => x.GetCustomAttributes(typeof(ItemScript), false).FirstOrDefault() != null);

            MethodInfo method = scripts.FirstOrDefault(m => m.GetCustomAttributes(typeof(ItemScript), false) != null && m.GetCustomAttributes(typeof(ItemScript), false).Length > 0 && ((ItemScript)m.GetCustomAttributes(typeof(ItemScript), false)[0]) != null && ((ItemScript)m.GetCustomAttributes(typeof(ItemScript), false)[0]).Script.ToLower() == script.ToLower());
            if (method == null)
            {
                return await Task.FromResult(false);
            }

            List<object> obj_list = new List<object> { player, slot };

            object instance = Activator.CreateInstance(method.DeclaringType);

            object[] builder = obj_list.ToArray();

            return await Task.FromResult(await (Task<bool>)method.Invoke(instance, builder));
        }

        //[HandleExceptions]
        [ItemScript("orangeseeds")]
        public async Task<bool> Orangeseeds(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            using var db = new RXContext();

            var count = await db.Plants.Where(x => x.OwnerId == player.Id && x.Type == (int)PlantType.Orange).CountAsync();

            if (count >= 10)
            {
                await player.SendNotify("Du kannst maximal 10 wachsende Orangenpflanzen pflanzen. Bitte ernte eine dieser Pflanzen und versuche es erneut.");

                return false;
            }

            List<RXColShape> colShapes = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllColShapes().Cast<RXColShape>().ToList());
            if (colShapes == null || colShapes.Count < 1) return false;

            RXColShape colShape = await NAPI.Task.RunReturnAsync(() => colShapes.FirstOrDefault(colShape => colShape.IsPointWithin(player.Position)));
            if (colShape == null) return false;

            if (colShape.PlantPlace && await player.GetDimensionAsync() == 0)
            {
                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(20000);

                player.IsTaskAllowed = false;

                await player.PlayAnimationAsync(33, "amb@world_human_gardener_plant@male@idle_a", "idle_a", 8);
                await Task.Delay(20000);

                lock (player) if (!RX.PlayerExists(player)) return false;

                player.IsTaskAllowed = true;

                await player.SendNotify("Du hast erfolgreich eine Orangenpflanze gepflanzt, diese ist in einer Stunde zur Ernte bereit.");

                await player.StopAnimationAsync();
                await player.disableAllPlayerActions(false);

                await SeedModule.CreatePlant(player, PlantType.Orange);

                return true;
            }
            else
            {
                await player.SendNotify("Hier kannst du nichts anbauen, der Boden ist nicht ausreichend gedüngt!");

                return false;
            }
        }

        [ItemScript("weedseeds")]
        public async Task<bool> Weedseeds(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            var plantPot = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Blumentopf");
            if (plantPot == null) return false;

            var fertilizer = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Dünger");
            if (fertilizer == null) return false;

            if (player.Container.GetItemAmount("Blumentopf") < 1 || player.Container.GetItemAmount("Dünger") < 1) return false;

            using var db = new RXContext();

            var count = await db.Plants.Where(x => x.OwnerId == player.Id && x.Type == (int)PlantType.Weed).CountAsync();

            if (count >= 10)
            {
                await player.SendNotify("Du kannst maximal 10 wachsende Weedpflanzen pflanzen. Bitte ernte eine dieser Pflanzen und versuche es erneut.");

                return false;
            }

            if (await player.GetDimensionAsync() == 0)
            {
                player.Container.RemoveItem(plantPot);
                player.Container.RemoveItem(fertilizer);

                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(20000);

                player.IsTaskAllowed = false;

                await player.PlayAnimationAsync(33, "amb@world_human_gardener_plant@male@idle_a", "idle_a", 8);
                await Task.Delay(20000);

                lock (player) if (!RX.PlayerExists(player)) return false;

                player.IsTaskAllowed = true;

                await player.SendNotify("Du hast erfolgreich eine Weedpflanze gepflanzt, diese ist in sechs Stunden zur Ernte bereit.");

                await player.StopAnimationAsync();
                await player.disableAllPlayerActions(false);

                await SeedModule.CreatePlant(player, PlantType.Weed);

                return true;
            }
            else
            {
                await player.SendNotify("Hier kannst du nichts anbauen, der Boden ist nicht ausreichend gedüngt!");

                return false;
            }
        }

        [ItemScript("kfz_gt63samg")]
        public async Task<bool> kfz_gt63samg(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;




            using var db = new RXContext();

            var dbVehicle = new DbVehicle
            {
                Id = await db.Vehicles.CountAsync() == 0 ? 1 : (await db.Vehicles.MaxAsync(v => v.Id) + 1),
                ContainerId = 0,
                Distance = 0,
                Fuel = 100,
                GarageId = 2,
                Hash = "rmodgt63",
                ModelId = 107,
                OwnerId = player.Id,
                Plate = "",
                Position = "0,0",
                R = 0,
                G = 0,
                B = 0,
                Rotation = "0,0,0",
                Stored = true,
                Tuning = "{}"
            };

            await db.Vehicles.AddAsync(dbVehicle);

            await db.SaveChangesAsync();

            await player.SendNotify("Dein GT63s AMG wurde dir erfolgreich in die Pillbox Garage gesetzt!", 3500, "orange", "Fahrzeugbrief");

            return true;

        }

        [ItemScript("kfz_teslasplaid")]
        public async Task<bool> kfz_teslasplaid(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;




            using var db = new RXContext();

            var dbVehicle = new DbVehicle
            {
                Id = await db.Vehicles.CountAsync() == 0 ? 1 : (await db.Vehicles.MaxAsync(v => v.Id) + 1),
                ContainerId = 0,
                Distance = 0,
                Fuel = 100,
                GarageId = 2,
                Hash = "models",
                ModelId = 108,
                OwnerId = player.Id,
                Plate = "",
                Position = "0,0",
                R = 0,
                G = 0,
                B = 0,
                Rotation = "0,0,0",
                Stored = true,
                Tuning = "{}"
            };

            await db.Vehicles.AddAsync(dbVehicle);

            await db.SaveChangesAsync();

            await player.SendNotify("Dein Tesla Model S wurde dir erfolgreich in die Pillbox Garage gesetzt!", 3500, "orange", "Fahrzeugbrief");

            return true;

        }

        [ItemScript("kfz_audir8")]
        public async Task<bool> kfz_audir8(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;




            using var db = new RXContext();

            var dbVehicle = new DbVehicle
            {
                Id = await db.Vehicles.CountAsync() == 0 ? 1 : (await db.Vehicles.MaxAsync(v => v.Id) + 1),
                ContainerId = 0,
                Distance = 0,
                Fuel = 100,
                GarageId = 2,
                Hash = "r820",
                ModelId = 109,
                OwnerId = player.Id,
                Plate = "",
                Position = "0,0",
                R = 0,
                G = 0,
                B = 0,
                Rotation = "0,0,0",
                Stored = true,
                Tuning = "{}"
            };

            await db.Vehicles.AddAsync(dbVehicle);

            await db.SaveChangesAsync();

            await player.SendNotify("Dein Audi R8 wurde dir erfolgreich in die Pillbox Garage gesetzt!", 3500, "orange", "Fahrzeugbrief");

            return true;

        }

        [ItemScript("kfz_jugular")]
        public async Task<bool> kfz_jugular(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;




            using var db = new RXContext();

            var dbVehicle = new DbVehicle
            {
                Id = await db.Vehicles.CountAsync() == 0 ? 1 : (await db.Vehicles.MaxAsync(v => v.Id) + 1),
                ContainerId = 0,
                Distance = 0,
                Fuel = 100,
                GarageId = 2,
                Hash = "jugular",
                ModelId = 23,
                OwnerId = player.Id,
                Plate = "",
                Position = "0,0",
                R = 0,
                G = 0,
                B = 0,
                Rotation = "0,0,0",
                Stored = true,
                Tuning = "{}"
            };

            await db.Vehicles.AddAsync(dbVehicle);

            await db.SaveChangesAsync();

            await player.SendNotify("Dein Jugular wurde dir erfolgreich in die Pillbox Garage gesetzt!", 3500, "orange", "Fahrzeugbrief");

            return true;

        }

        [ItemScript("rubbellosluxus")]
        public async Task<bool> RubellosLuxus(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (RubbellosModule.lose.ContainsKey(player))
            {
                RubbellosModule.lose.Remove(player);
            }

            Random random = new Random();
            int[] rubbell = new int[6];

            rubbell[0] = RubbellosModule.losgewinne[random.Next(0, RubbellosModule.losgewinne.Length)];
            rubbell[1] = RubbellosModule.losgewinne[random.Next(0, RubbellosModule.losgewinne.Length)];
            rubbell[2] = RubbellosModule.losgewinne[random.Next(0, RubbellosModule.losgewinne.Length)];
            rubbell[3] = RubbellosModule.losgewinne[random.Next(0, RubbellosModule.losgewinne.Length)];
            rubbell[4] = RubbellosModule.losgewinne[random.Next(0, RubbellosModule.losgewinne.Length)];
            rubbell[5] = RubbellosModule.losgewinne[random.Next(0, RubbellosModule.losgewinne.Length)];

            RubbellosModule.lose.Add(player, new int[] { rubbell[0], rubbell[1], rubbell[2], rubbell[3], rubbell[4], rubbell[5] });

            // rubbell[0], rubbell[1], rubbell[2], rubbell[3], rubbell[4], rubbell[5]);

            RXWindow window = new RXWindow("Scratchcard");


            object losobj = new
            {
                t = 2,
                n = rubbell,
            };

            await window.OpenWindow(player, losobj);

            return true;
        }
        [ItemScript("mysterybox_legendary")]
        public async Task<bool> mysterybox_legendary(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;


            await CaseOpeningModule.OpenCase(player, "Legendary");

            return true;
        }
        [ItemScript("mysterybox_epic")]
        public async Task<bool> mysterybox_epic(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;


            await CaseOpeningModule.OpenCase(player, "Epic");

            return true;
        }
        [ItemScript("mysterybox_normal")]
        public async Task<bool> mysterybox_normal(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;


            await CaseOpeningModule.OpenCase(player, "Normal");

            return true;
        }


        [ItemScript("rubbellos")]
        public async Task<bool> Rubellos(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (RubbellosModule.lose.ContainsKey(player))
            {
                RubbellosModule.lose.Remove(player);
            }

            Random random = new Random();
            int[] rubbell = new int[6];

            rubbell[0] = RubbellosModule.losgewinnebad[random.Next(0, RubbellosModule.losgewinnebad.Length)];
            rubbell[1] = RubbellosModule.losgewinnebad[random.Next(0, RubbellosModule.losgewinnebad.Length)];
            rubbell[2] = RubbellosModule.losgewinnebad[random.Next(0, RubbellosModule.losgewinnebad.Length)];
            rubbell[3] = RubbellosModule.losgewinnebad[random.Next(0, RubbellosModule.losgewinnebad.Length)];
            rubbell[4] = RubbellosModule.losgewinnebad[random.Next(0, RubbellosModule.losgewinnebad.Length)];
            rubbell[5] = RubbellosModule.losgewinnebad[random.Next(0, RubbellosModule.losgewinnebad.Length)];

            RubbellosModule.lose.Add(player, new int[] { rubbell[0], rubbell[1], rubbell[2], rubbell[3], rubbell[4], rubbell[5]});

            // rubbell[0], rubbell[1], rubbell[2], rubbell[3], rubbell[4], rubbell[5]);

            RXWindow window = new RXWindow("Scratchcard");


            object losobj = new
            {
                t = 1,
                n = rubbell,
            };

            await window.OpenWindow(player, losobj);

            return true;
        }

        [ItemScript("drink_5")]
        public async Task<bool> drink_5(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;
            var eat = player.Container.GetItemOnSlot(slot);

            await player.PlayAnimationAsync(49, "mp_player_intdrink", "intro_bottle");
            await player.TriggerEventAsync("attachfoodobj", eat.Model.ItemModel, 8000);
            await player.SendProgressbar(8000);

            player.DraggingTime = 8000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await Task.Delay(8000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;
            await player.StopAnimationAsync();
            player.DraggingItem = false;
            player.IsTaskAllowed = true;
            player.Thirst += 5;

            return true;
        }

        [ItemScript("drink_10")]
        public async Task<bool> drink_10(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;
            var eat = player.Container.GetItemOnSlot(slot);

            await player.PlayAnimationAsync(49, "mp_player_intdrink", "intro_bottle");
            await player.TriggerEventAsync("attachfoodobj", eat.Model.ItemModel, 8000);
            await player.SendProgressbar(8000);

            player.DraggingTime = 8000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await Task.Delay(8000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;
            await player.StopAnimationAsync();
            player.DraggingItem = false;
            player.IsTaskAllowed = true;
            player.Thirst += 10;

            return true;
        }

        [ItemScript("drink_15")]
        public async Task<bool> drink_15(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;
            var eat = player.Container.GetItemOnSlot(slot);

            await player.PlayAnimationAsync(49, "mp_player_intdrink", "intro_bottle");
            await player.TriggerEventAsync("attachdrinkobj", eat.Model.ItemModel, 8000);
            await player.SendProgressbar(8000);

            player.DraggingTime = 8000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await Task.Delay(8000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;
            await player.StopAnimationAsync();
            player.DraggingItem = false;
            player.IsTaskAllowed = true;
            player.Thirst += 15;

            return true;
        }


        [ItemScript("drink_30")]
        public async Task<bool> drink_30(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;
            var eat = player.Container.GetItemOnSlot(slot);

            await player.PlayAnimationAsync(49, "mp_player_intdrink", "intro_bottle");
            await player.TriggerEventAsync("attachdrinkobj", eat.Model.ItemModel, 8000);
            await player.SendProgressbar(8000);

            player.DraggingTime = 8000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await Task.Delay(8000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;
            await player.StopAnimationAsync();
            player.DraggingItem = false;
            player.IsTaskAllowed = true;
            player.Thirst += 30;

            return true;
        }


        [ItemScript("eat_5")]
        public async Task<bool> eat_5(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;
            var eat = player.Container.GetItemOnSlot(slot);

            await player.PlayAnimationAsync(49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
            await player.TriggerEventAsync("attachfoodobj", eat.Model.ItemModel, 8000);
            await player.SendProgressbar(8000);

            player.DraggingTime = 8000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await Task.Delay(8000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;
            await player.StopAnimationAsync();

            player.DraggingItem = false;
            player.IsTaskAllowed = true;
            player.Hunger += 5;

            return true;
        }


        [ItemScript("eat_10")]
        public async Task<bool> eat_10(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;
            var eat = player.Container.GetItemOnSlot(slot);

            await player.PlayAnimationAsync(49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
            await player.TriggerEventAsync("attachfoodobj", eat.Model.ItemModel, 8000);
            await player.SendProgressbar(8000);

            player.DraggingTime = 8000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await Task.Delay(8000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;
            await player.StopAnimationAsync();

            player.DraggingItem = false;
            player.IsTaskAllowed = true;
            player.Hunger += 10;

            return true;
        }

        [ItemScript("eat_15")]
        public async Task<bool> eat_15(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;
            var eat = player.Container.GetItemOnSlot(slot);

            await player.PlayAnimationAsync(49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
            await player.TriggerEventAsync("attachfoodobj", eat.Model.ItemModel, 8000);
            await player.SendProgressbar(8000);

            player.DraggingTime = 8000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await Task.Delay(8000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;
            await player.StopAnimationAsync();

            player.DraggingItem = false;
            player.IsTaskAllowed = true;
            player.Hunger += 15;

            return true;
        }


        [ItemScript("eat_30")]
        public async Task<bool> eat_30(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;
            var eat = player.Container.GetItemOnSlot(slot);

            await player.PlayAnimationAsync(49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
            await player.TriggerEventAsync("attachfoodobj", eat.Model.ItemModel, 8000);
            await player.SendProgressbar(8000);

            player.DraggingTime = 8000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await Task.Delay(8000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;
            await player.StopAnimationAsync();

            player.DraggingItem = false;
            player.IsTaskAllowed = true;
            player.Hunger += 30;

            return true;
        }

        [ItemScript("eat_35")]
        public async Task<bool> eat_35(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;
            var eat = player.Container.GetItemOnSlot(slot);

            await player.PlayAnimationAsync(49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
            await player.TriggerEventAsync("attachfoodobj", eat.Model.ItemModel, 8000);
            await player.SendProgressbar(8000);

            player.DraggingTime = 8000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await Task.Delay(8000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;
            await player.StopAnimationAsync();

            player.DraggingItem = false;
            player.IsTaskAllowed = true;
            player.Hunger += 35;

            return true;
        }


        [ItemScript("eat_40")]
        public async Task<bool> eat_40(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;
            var eat = player.Container.GetItemOnSlot(slot);

            await player.PlayAnimationAsync(49, "mp_player_inteat@burger", "mp_player_int_eat_burger");
            await player.TriggerEventAsync("attachfoodobj", eat.Model.ItemModel, 8000);
            await player.SendProgressbar(8000);

            player.DraggingTime = 8000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await Task.Delay(8000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;
            await player.StopAnimationAsync();

            player.DraggingItem = false;
            player.IsTaskAllowed = true;
            player.Hunger += 40;

            return true;
        }


        [ItemScript("registervehicle")]
        public async Task<bool> registervehicle(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || !player.CanInteract()) return false;

            if (!player.Team.CanRegisterVehicles)
            {
                await player.SendNotify("Diese Aktion kann nur das LSPD/DMV/DPOS durchführen!");
                return false;
            }

            if (!player.InDuty)
            {
                await player.SendNotify("Sie müssen im Dienst sein um Fahrzeuge anzumelden.");
                return false;
            }

            if (player.Teamrank < 3)
            {
                await player.SendNotify("Dafür musst du mindestens Rang 3 sein!");
                return false;
            }

            Vector3 playerpos = await player.GetPositionAsync();
            if (playerpos.DistanceTo(new GTANetworkAPI.Vector3(386.223, -1621.51, 29.292)) > VehicleRegistration.RegistrationRadius)
            {
                await player.SendNotify("Sie müssen am Zulassungsplatz sein.");
                return false;
            }

            RXVehicle sxVehicle = await NAPI.Task.RunReturnAsync(() => VehicleController.GetClosestVehicle(playerpos));
            if (sxVehicle == null)
            {
                await player.SendNotify("Kein Fahrzeug in der Nähe!");
                return false;
            }

            if (sxVehicle.Id == 0) return false;

            if (await VehicleRegistration.IsVehicleRegistered(sxVehicle.Id))
            {
                await player.SendNotify("Dieses Fahrzeug ist bereits angemeldet!");
                return false;
            }

         

            if (sxVehicle != null)
            {
                //driver is available
                if (await NAPI.Task.RunReturnAsync(() => sxVehicle.Occupants.FirstOrDefault(x => ((RXPlayer)x).VehicleSeat == 0)) == null) return false;

                var seat = await NAPI.Task.RunReturnAsync(() => (RXPlayer)sxVehicle.Occupants.FirstOrDefault(x => ((RXPlayer)x).VehicleSeat == 0));

                var driver = await PlayerController.FindPlayerById(seat.Id);

                if (driver == null) return false;

                //check if driver is owner
                if (sxVehicle.OwnerId == driver.Id)
                {
                    //yees driver is owner

                    RXItemModel numberplate = ItemModelModule.ItemModels.Find(x => x.Id == 42);
                    if (numberplate == null)
                    {
                     
                            await player.SendNotify("Sie benötigen ein Kennzeichen");
                            return false;
                    }

                    String plateString = await VehicleRegistration.GetRandomPlate(true);
                    

                    bool successfullyRegistered = await VehicleRegistration.registerVehicle(sxVehicle, driver, player, plateString, false);

                    if (successfullyRegistered)
                    {
                        if (numberplate != null)
                        {
                            player.Container.RemoveItem(numberplate);
                        }
                    }

                    return successfullyRegistered;
                }
            }
            await player.SendNotify("Der Besitzer des Fahrzeugs muss auf dem Fahrersitz sein");        


            return true;
        }

        [ItemScript("blackmoney")]
        public async Task<bool> blackmoney(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || !player.CanInteract()) return false;

            var item = ItemModelModule.ItemModels.Find(x => x.Id == 33);
            int amount = player.Container.GetItemOnSlot(slot).Amount;

            await player.SendProgressbar(3000);

            // Remove
            player.Container.RemoveItemSlotFirst(item, slot, amount);

            player.Freezed = true;

            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

            player.SetData("userCannotInterrupt", true);

            await Task.Delay(3000);

            player.SetData("userCannotInterrupt", false);
            await player.StopAnimationAsync();

            player.Freezed = false;

            player.Blackmoney += amount;
            await player.SendNotify($"Du hast {amount}$ Schwarzgeld entpackt.");
            return true;
        }

        [ItemScript("zig")]
        public async Task<bool> Zig(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_aa_smoke@male@idle_a", "idle_b");
            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(4000);

            player.IsTaskAllowed = false;

            await Task.Delay(4000);
            player.Stress -= 20;
            if (player.Stress < 0)
            {
                player.Stress = 0;
            }
            await player.disableAllPlayerActions(false);
            await player.StopAnimationAsync();

            player.IsTaskAllowed = true;
            await InjuryModule.CheckStressStatus(player);


            return true;
        }


        [ItemScript("graphiccard_1")]
        public async Task<bool> graphiccard_1(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (!player.HasData("ServerId"))
            {
                await player.SendNotify("Du stehst an keinem Server! Was willst du mit diesem Server-Teil?");
                return false;
            }


            using var db = new RXContext();

            uint playerserverid = player.GetData<uint>("ServerId");

            var house = await db.Houses.FirstOrDefaultAsync(x => x.id == playerserverid);

            if (house == null) return false;

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dieser Server gehört nicht dir! Es wäre nicht nett daran rumzuspielen...");
                return false;
            }

            var houseserver = await db.HouseServers.FirstOrDefaultAsync(x => x.houseid == playerserverid);

            if (houseserver == null) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(4000);
            player.IsTaskAllowed = false;
            await Task.Delay(4000);
            await player.disableAllPlayerActions(false);
            player.IsTaskAllowed = true;
            houseserver.GraphicCard = 1;
           
            db.HouseServers.Update(houseserver);

            await db.SaveChangesAsync();



            return true;
        }



        [ItemScript("graphiccard_2")]
        public async Task<bool> graphiccard_2(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (!player.HasData("ServerId"))
            {
                await player.SendNotify("Du stehst an keinem Server! Was willst du mit diesem Server-Teil?");
                return false;
            }


            using var db = new RXContext();

            uint playerserverid = player.GetData<uint>("ServerId");

            var house = await db.Houses.FirstOrDefaultAsync(x => x.id == playerserverid);

            if (house == null) return false;

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dieser Server gehört nicht dir! Es wäre nicht nett daran rumzuspielen...");
                return false;
            }

            var houseserver = await db.HouseServers.FirstOrDefaultAsync(x => x.houseid == playerserverid);

            if (houseserver == null) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(4000);
            player.IsTaskAllowed = false;
            await Task.Delay(4000);
            await player.disableAllPlayerActions(false);
            player.IsTaskAllowed = true;
            houseserver.GraphicCard = 2;

            db.HouseServers.Update(houseserver);

            await db.SaveChangesAsync();



            return true;
        }


        [ItemScript("graphiccard_3")]
        public async Task<bool> graphiccard_3(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (!player.HasData("ServerId"))
            {
                await player.SendNotify("Du stehst an keinem Server! Was willst du mit diesem Server-Teil?");
                return false;
            }


            using var db = new RXContext();

            uint playerserverid = player.GetData<uint>("ServerId");

            var house = await db.Houses.FirstOrDefaultAsync(x => x.id == playerserverid);

            if (house == null) return false;

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dieser Server gehört nicht dir! Es wäre nicht nett daran rumzuspielen...");
                return false;
            }

            var houseserver = await db.HouseServers.FirstOrDefaultAsync(x => x.houseid == playerserverid);

            if (houseserver == null) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(4000);
            player.IsTaskAllowed = false;
            await Task.Delay(4000);
            await player.disableAllPlayerActions(false);
            player.IsTaskAllowed = true;
            houseserver.GraphicCard = 3;

            db.HouseServers.Update(houseserver);

            await db.SaveChangesAsync();



            return true;
        }


        [ItemScript("cpu_1")]
        public async Task<bool> cpu_1(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (!player.HasData("ServerId"))
            {
                await player.SendNotify("Du stehst an keinem Server! Was willst du mit diesem Server-Teil?");
                return false;
            }


            using var db = new RXContext();

            uint playerserverid = player.GetData<uint>("ServerId");

            var house = await db.Houses.FirstOrDefaultAsync(x => x.id == playerserverid);

            if (house == null) return false;

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dieser Server gehört nicht dir! Es wäre nicht nett daran rumzuspielen...");
                return false;
            }

            var houseserver = await db.HouseServers.FirstOrDefaultAsync(x => x.houseid == playerserverid);

            if (houseserver == null) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(4000);
            player.IsTaskAllowed = false;
            await Task.Delay(4000);
            await player.disableAllPlayerActions(false);
            player.IsTaskAllowed = true;
            houseserver.CPU = 1;

            db.HouseServers.Update(houseserver);

            await db.SaveChangesAsync();



            return true;
        }


        [ItemScript("cpu_2")]
        public async Task<bool> cpu_2(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (!player.HasData("ServerId"))
            {
                await player.SendNotify("Du stehst an keinem Server! Was willst du mit diesem Server-Teil?");
                return false;
            }


            using var db = new RXContext();

            uint playerserverid = player.GetData<uint>("ServerId");

            var house = await db.Houses.FirstOrDefaultAsync(x => x.id == playerserverid);

            if (house == null) return false;

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dieser Server gehört nicht dir! Es wäre nicht nett daran rumzuspielen...");
                return false;
            }

            var houseserver = await db.HouseServers.FirstOrDefaultAsync(x => x.houseid == playerserverid);

            if (houseserver == null) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(4000);
            player.IsTaskAllowed = false;
            await Task.Delay(4000);
            await player.disableAllPlayerActions(false);
            player.IsTaskAllowed = true;
            houseserver.CPU = 2;

            db.HouseServers.Update(houseserver);

            await db.SaveChangesAsync();



            return true;
        }

        [ItemScript("memory")]
        public async Task<bool> memory(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (!player.HasData("ServerId"))
            {
                await player.SendNotify("Du stehst an keinem Server! Was willst du mit diesem Server-Teil?");
                return false;
            }


            using var db = new RXContext();

            uint playerserverid = player.GetData<uint>("ServerId");

            var house = await db.Houses.FirstOrDefaultAsync(x => x.id == playerserverid);

            if (house == null) return false;

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dieser Server gehört nicht dir! Es wäre nicht nett daran rumzuspielen...");
                return false;
            }

            var houseserver = await db.HouseServers.FirstOrDefaultAsync(x => x.houseid == playerserverid);

            if (houseserver == null) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(4000);
            player.IsTaskAllowed = false;
            await Task.Delay(4000);
            await player.disableAllPlayerActions(false);
            player.IsTaskAllowed = true;
            houseserver.RAM = 1;

            db.HouseServers.Update(houseserver);

            await db.SaveChangesAsync();



            return true;
        }

        [ItemScript("powersupply")]
        public async Task<bool> powersupply(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (!player.HasData("ServerId"))
            {
                await player.SendNotify("Du stehst an keinem Server! Was willst du mit diesem Server-Teil?");
                return false;
            }


            using var db = new RXContext();

            uint playerserverid = player.GetData<uint>("ServerId");

            var house = await db.Houses.FirstOrDefaultAsync(x => x.id == playerserverid);

            if (house == null) return false;

            if (house.ownerID != player.Id)
            {
                await player.SendNotify("Dieser Server gehört nicht dir! Es wäre nicht nett daran rumzuspielen...");
                return false;
            }

            var houseserver = await db.HouseServers.FirstOrDefaultAsync(x => x.houseid == playerserverid);

            if (houseserver == null) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(4000);
            player.IsTaskAllowed = false;
            await Task.Delay(4000);
            await player.disableAllPlayerActions(false);
            player.IsTaskAllowed = true;
            houseserver.Netzteil = 1;

            db.HouseServers.Update(houseserver);

            await db.SaveChangesAsync();



            return true;
        }

        [ItemScript("hackingkit")]
        public async Task<bool> HackingKit(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;


            if (!player.HasData("blitzerhacking"))
            {
                await player.SendNotify("Was willst du hier hacken?");
                return false;
            }

            DbBlitzer blitzer = player.GetData<DbBlitzer>("blitzerhacking");
            if (blitzer.LastHacked.AddMinutes(30) > DateTime.Now)
            {
                await player.SendNotify("Dieser Blitzer wurde bereits gehackt!");
                return true;
            }

            await player.TriggerEventAsync("CircuitBreakerStart", 2, 2, 2, "hackBlitzer");
            player.Freezed = true;
         
            return true;
        }

        [ItemScript("werkzeugkasten")]
        public async Task<bool> Werkzeugkasten(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;


            if (!player.Team.IsState()) return false;

            if (!player.HasData("blitzerhacking"))
            {
                return false;
            }

            DbBlitzer blitzer = player.GetData<DbBlitzer>("blitzerhacking");
            if (blitzer.Hacked)
            {
                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(20000);

                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missmechanic", "work2_base");
                await Task.Delay(20000);

                await player.SendNotify("Du hast den Blitzer erfolgreich repariert!", 3500, "green");
                await player.StopAnimationAsync();
                await player.disableAllPlayerActions(false);
                blitzer.Hacked = false;
            } else
            {
                return false;
            }
            return true;
        }

        [ItemScript("methcook")]
        public async Task<bool> Methcook(RXPlayer player, int slot)
        {
            if (player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;


            if (!player.HasData("IsInCamper"))
            {
                await player.SendNotify("Du befindest dich in keinem Camper/Wohnmobil!");
                return false;
            }

            if (!player.HasData("cooking"))
            {
                var battery = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Batterie");
                if (battery == null) return false;

                var ephedrin = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Ephedrin");
                if (ephedrin == null) return false;

                var toilet = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Toilettenreiniger");
                if (toilet == null) return false;

                if (player.Container.GetItemAmount("Batterie") < 1 || player.Container.GetItemAmount("Ephedrin") < 1 || player.Container.GetItemAmount("Toilettenreiniger") < 1)
                {
                    await player.SendNotify("Wie willst du ohne Batterien deinen Kocher anschalten? Wie willst du ohne Ephedrin Meth herstellen? Woher willst du ohne Chemikalie arbeiten? Zum Beispiel Toilettenreiniger?");
                    return false;
                }

                await player.SendNotify("Du beginnst nun Meth zu kochen!", 5000, "green");
                player.SetData<bool>("cooking", true);
                if (!MethModule.CookingPlayers.Contains(player))
                {
                    MethModule.CookingPlayers.Add(player);
                }


            }
           

            return true;
        }

        [ItemScript("weed")]
        public async Task<bool> Weed(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            var paper = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Paper");
            if (paper == null) return false;

            var filter = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Filter");
            if (filter == null) return false;

            var joint = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Joint");
            if (joint == null) return false;

            if (player.Container.GetItemAmount("Paper") < 1 || player.Container.GetItemAmount("Filter") < 1) return false;

            player.Container.RemoveItem(paper);
            player.Container.RemoveItem(filter);

            await player.SendNotify("Du fängst nun an einen Joint zu drehen...");

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(40000);

            player.IsTaskAllowed = false;

            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");
            await Task.Delay(40000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            player.IsTaskAllowed = true;

            await player.StopAnimationAsync();
            await player.disableAllPlayerActions(false);

            player.Container.AddItem(joint);

            await player.SendNotify("Du hast einen Joint gedreht!");

            return true;
        }

        [ItemScript("joint")]
        public async Task<bool> Joint(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            player.IsTaskAllowed = false;

            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_smoking_pot@male@base", "base");
            player.Freezed = true;
            await Task.Delay(8000);
            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@incar@male@smoking@exit", "exit");
            await Task.Delay(2500);
            player.Freezed = false;
            await player.StopAnimationAsync();

            await player.SetHealthAsync(200);

            player.IsTaskAllowed = true;

            player.Joints++;

            if (player.Joints >= 1)
            {
                player.LastJoint = DateTime.Now;
                await player.TriggerEventAsync("startScreenEffect", "DrugsMichaelAliensFight", 60000, true);
            }

            return true;
        }

        public static bool GetAllowedAmmo55645MNatoWeapons(WeaponHash weapon)
        {
            switch (weapon)
            {
                case WeaponHash.Specialcarbine:
                case WeaponHash.Advancedrifle:
                case WeaponHash.Bullpuprifle:
                case WeaponHash.Carbinerifle:
                case WeaponHash.Carbinerifle_mk2:
                case WeaponHash.Specialcarbine_mk2:
                    return true;
            }

            return false;
        }

        //[HandleExceptions]
        [ItemScript("ammo_5_5_6_45mm_nato")]
        public async Task<bool> ammo_5_5_6_45mm_nato(RXPlayer player, int slot)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !player.IsTaskAllowed) return false;
            if (player.Dimension != 0 || player.CurrentWeapon == WeaponHash.Unarmed) return false;

            WeaponHash currentWeapon = player.CurrentWeapon;

            if (!GetAllowedAmmo55645MNatoWeapons(currentWeapon)) return false;

            WeaponLoadoutItem playerWeaponsWeapon = player.Weapons.Find(x => x.WeaponHash == currentWeapon.ToString());
            int currentWeaponAmmo = player.GetWeaponAmmo(currentWeapon);

            RXItemModel itemModelWeapon = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == InventoryModule.GetWeaponPackItemIDByWeaponHash(currentWeapon, playerWeaponsWeapon.BWeapon));
            if (itemModelWeapon == null) return false;

            RXItemModel itemModelAmmo = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == InventoryModule.GetWeaponPackAmmoItemIDByWeaponHash(currentWeapon));
            if (itemModelAmmo == null) return false;

            RXItemModel model = player.Container.GetModelOnSlot(slot);
            RXItem item = player.Container.GetItemOnSlot(slot);

            int weaponClipSize = InventoryModule.GetWeaponPackClipSizeByWeaponHash(currentWeapon);
            if (weaponClipSize == 0) return false;

            int timeToLoad = item.Amount * 500;
            int AmmoToAdd = item.Amount * weaponClipSize;

            var dbweapon = player.Weapons.Find(x => x.WeaponHash == currentWeapon.ToString());
            if (dbweapon == null) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(timeToLoad);

            player.IsTaskAllowed = false;

            player.StopAnimation();
            player.PlayAnimation("missheistdockssetup1ig_3@base", "welding_base_dockworker", 1);

            await Task.Delay(timeToLoad);

            player.StopAnimation();
            player.IsTaskAllowed = true;
            await player.disableAllPlayerActions(false);

            dbweapon.Ammo += AmmoToAdd;
            player.SetWeaponAmmo(currentWeapon, dbweapon.Ammo);

            player.Container.RemoveItemSlotFirst(model, slot, item.Amount);

            await player.SendNotify($"Du hast {item.Amount} Magazine in deine {itemModelWeapon.Name} geladen!", 5000, "green");

            return true;
        }

        [ItemScript("ammo")]
        public async Task<bool> ammo(RXPlayer player, int slot)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || player.IsInvDisabled || !player.IsTaskAllowed) return false;
            if (player.Dimension != 0 || player.CurrentWeapon == WeaponHash.Unarmed) return false;

            WeaponHash currentWeapon = player.CurrentWeapon;

            if (GetAllowedAmmo55645MNatoWeapons(currentWeapon)) return false;

            WeaponLoadoutItem playerWeaponsWeapon = player.Weapons.Find(x => x.WeaponHash == currentWeapon.ToString());
            int currentWeaponAmmo = player.GetWeaponAmmo(currentWeapon);

            RXItemModel itemModelWeapon = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == InventoryModule.GetWeaponPackItemIDByWeaponHash(currentWeapon, playerWeaponsWeapon.BWeapon));
            if (itemModelWeapon == null) return false;

            RXItemModel itemModelAmmo = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == InventoryModule.GetWeaponPackAmmoItemIDByWeaponHash(currentWeapon));
            if (itemModelAmmo == null) return false;

            RXItemModel model = player.Container.GetModelOnSlot(slot);
            RXItem item = player.Container.GetItemOnSlot(slot);

            int weaponClipSize = InventoryModule.GetWeaponPackClipSizeByWeaponHash(currentWeapon);
            if (weaponClipSize == 0) return false;

            int timeToLoad = item.Amount * 500;
            int AmmoToAdd = item.Amount * weaponClipSize;

            var dbweapon = player.Weapons.Find(x => x.WeaponHash == currentWeapon.ToString());
            if (dbweapon == null) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(timeToLoad);

            player.IsTaskAllowed = false;

            player.StopAnimation();
            player.PlayAnimation("missheistdockssetup1ig_3@base", "welding_base_dockworker", 1);

            await Task.Delay(timeToLoad);

            player.StopAnimation();
            player.IsTaskAllowed = true;
            await player.disableAllPlayerActions(false);

            dbweapon.Ammo += AmmoToAdd;
            player.SetWeaponAmmo(currentWeapon, dbweapon.Ammo);

            player.Container.RemoveItemSlotFirst(model, slot, item.Amount);

            await player.SendNotify($"Du hast {item.Amount} Magazine in deine {itemModelWeapon.Name} geladen!", 5000, "green");

            return true;
        }

        //[HandleExceptions]
        [ItemScript("state_bulletproof")]
        public async Task<bool> state_bulletproof(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (!player.Team.IsLowestState()) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(2000);

            player.DraggingTime = 2000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await player.PlayAnimationAsync(33, "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 8);
            await Task.Delay(2000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;

            player.DraggingItem = false;
            player.IsTaskAllowed = true;

            await player.SetArmorAsync(100);

            await player.SendNotify("Du hast eine Beamtenschutzweste benutzt!", 3500, "green");
            // await player.StopProgressbar();
            await player.StopAnimationAsync();
            await player.disableAllPlayerActions(false);

            if (player.Team.Id == 1)
            {
                await player.SetClothesAsync(9, 12, 3);
            }
            else if (player.Team.Id == 5)
            {
                await player.SetClothesAsync(9, 3, 0);
            }
            else
            {
                await player.SetClothesAsync(9, 15, 2);
            }

            return true;
        }

        //[HandleExceptions]
        [ItemScript("bulletproof")]
        public async Task<bool> Bulletproof(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(4000);

            player.DraggingTime = 4000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await player.PlayAnimationAsync(33, "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 1);
            await Task.Delay(4000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;

            player.DraggingItem = false;
            player.IsTaskAllowed = true;

            await player.SetArmorAsync(100);

            await player.SendNotify("Du hast eine Schutzweste benutzt!", 3500, "green");
            // await player.StopProgressbar();
            await player.StopAnimationAsync();
            await player.disableAllPlayerActions(false);

            await player.SetClothesAsync(9, 15, 2);

            return true;
        }

        [ItemScript("schweissgeraet")]
        public async Task<bool> Schweissgeraet(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            Vector3 ppos = await player.GetPositionAsync();
            var bank = BankModule.ATM.Where(b => b.Value.DistanceTo(ppos) < 1.0f).FirstOrDefault();

            if (bank.Key != null) // 1 = ATM Only
            {
                if (await player.GetDimensionAsync() != 0)
                {
                    return false;
                }

                if (player.Level < 5)
                {
                    await player.SendNotify("Du musst mindestens Level 5 sein um einen ATM auszurauben!");
                    return false;
                }

                if (!RobberyModule.CanAtmRobbed()) return false; // ehm neeee!

                if (RobberyModule.RobbedAtms.ContainsKey(player.Id) && RobberyModule.RobbedAtms[player.Id] >= 3)
                {
                    await player.SendNotify("Du hast bereits zu viele Automaten ausgeraubt!");
                    return false;
                }

                if (BankModule.robbedatms.Contains(bank.Key))
                {
                    await player.SendNotify("Dieser Automat wurde bereits aufgeschweißt!");
                    return false;
                }

                BankModule.robbedatms.Add(bank.Key);
                await player.SendNotify("Sie beginnen nun damit den Automaten aufzuschweißen!");

                TeamModule.Teams.Find(x => x.Id == 1).SendMessageToDepartmentsInRange("Ein Bankautomat wird in ihrer Nähe aufgebrochen...", bank.Value, 100);

                await player.SendProgressbar(90000);

                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@ornate_bank@thermal_charge", "thermal_charge");
                player.Freezed = true;
                await player.disableAllPlayerActions(true);

                await Task.Delay(90000);
                await player.disableAllPlayerActions(false);

                if (player.IsCuffed || player.IsTied || player.Injured || ppos.DistanceTo(bank.Value) > 2.0f) return false;
                player.Freezed = false;
                await player.StopAnimationAsync();

                if (RobberyModule.RobbedAtms.ContainsKey(player.Id))
                {
                    RobberyModule.RobbedAtms[player.Id]++;
                }
                else RobberyModule.RobbedAtms.Add(player.Id, 1);

                await player.SendNotify("Bankautomat aufgebrochen.");


                Random rnd = new Random();
                var erhalt = rnd.Next(10000, 16000);

                player.Container.AddItem(RobberyModule.MarkierteScheineID, erhalt);
                await player.SendNotify($"${erhalt} markierte Scheine erbeutet!");

                return false;
            }

            return true;
        }

        //[HandleExceptions]
        [ItemScript("medkit")]
        public async Task<bool> Medkit(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            if (await player.GetHealthAsync() > 99) return false;

            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(4000);

            player.DraggingTime = 4000;
            player.IsTaskAllowed = false;
            player.DraggingItem = true;

            await player.PlayAnimationAsync(33, "amb@medic@standing@tendtodead@idle_a", "idle_a", 8);
            await Task.Delay(4000);

            lock (player) if (!RX.PlayerExists(player)) return false;

            if (!player.DraggingItem) return false;

            player.DraggingItem = false;
            player.IsTaskAllowed = true;

            await player.SetHealthAsync(200);

            await player.SendNotify("Du hast einen Verbandskasten benutzt!", 3500, "green");
            // await player.StopProgressbar();
            await player.StopAnimationAsync();
            await player.disableAllPlayerActions(false);

            return true;
        }

        //[HandleExceptions]
        [ItemScript("repairkit")]
        public async Task<bool> Repairkit(RXPlayer player, int slot)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return false;

            var closestVehicle = await NAPI.Task.RunReturnAsync(() => VehicleController.GetClosestVehicle(player.Position, 3));
            if (closestVehicle != null)
            {
                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missmechanic", "work2_base");
                await player.disableAllPlayerActions(true);

                player.DraggingTime = 20000;
                player.IsTaskAllowed = false;
                player.DraggingItem = true;

                await player.SendProgressbar(20000);
                await Task.Delay(20000);

                lock (player) if (!RX.PlayerExists(player)) return false;

                if (!player.DraggingItem) return false;

                player.DraggingItem = false;
                player.IsTaskAllowed = true;

                await player.disableAllPlayerActions(false);
                await player.StopAnimationAsync();

                if (await NAPI.Task.RunReturnAsync(() => closestVehicle.Position.DistanceTo(player.Position)) > 10) return false;

                await NAPI.Task.RunAsync(() =>
                {
                    closestVehicle.Repair();

                    closestVehicle.Health = 1000;
                });
            }

            return true;
        }
    }
}
