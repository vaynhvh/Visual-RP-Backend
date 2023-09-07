using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Inventory;
using Backend.Modules.Jail;
using Backend.Modules.Phone.Apps;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Farming
{
    class FarmingModule : RXModule
    {
        public FarmingModule() : base("Farming") { }

        public static List<RXFarming> farmingspots = new List<RXFarming>();
        public static List<RXPlayer> farmingplayers = new List<RXPlayer>();
        public static List<RXPlayer> farmingplayerstoremove = new List<RXPlayer>();
        public static Random randy = new Random();

        public override async void LoadAsync()
        {
            using var db = new RXContext();

            foreach (var item in await db.Farming.ToListAsync())
            {
                farmingspots.Add(new RXFarming { Id = item.Id, ItemId = item.ItemId, MinCount = item.MinCount, MaxCount = item.MaxCount, Name = item.Name, OnlyBadFaction = item.OnlyBadFaction, Positions = new List<Vector3>(), Range = item.Range, RequiredItemId = item.RequiredItemId, RestrictedToTeam = item.RestrictedToTeam });
            }

            foreach (var farmingpos in await db.FarmingPos.ToListAsync())
            {
                var spot = farmingspots.Find(x => x.Id == farmingpos.FarmingId);


                var mcb = await NAPI.Entity.CreateMCB(farmingpos.Position.ToPos(), new Color(255, 140, 0), 0u, spot.Range, 2.4f, false, MarkerType.VerticleCircle, false);

                mcb.ColShape.Action = async player => await StartFarming(player, spot);

                spot.Positions.Add(farmingpos.Position.ToPos());


            }

            foreach (var farmingpos in await db.FarmingPos.ToListAsync())
            {
                var spot = farmingspots.Find(x => x.Id == farmingpos.FarmingId);

                if (spot.Id != 1)
                {
                    var farminggps = GpsApp.gpsCategories.FirstOrDefault(x => x.Name == "Farming");
                    if (farminggps == null) return;

                    if (GpsApp.gpsCategories.Find(x => x.Name == spot.Name) != null) continue;

                    farminggps.Locations.Add(new GPSPosition(spot.Name, spot.Positions.First()));
                }
            }


            farmingplayers = new List<RXPlayer>();
            farmingplayerstoremove = new List<RXPlayer>();
        }

        public override async Task OnFiveSecond()
        {
            await PlayerController.GetValidPlayers().forEachAlternativeAsync(player =>
            {
                if (farmingplayerstoremove.Contains(player))
                {
                    farmingplayers.Remove(player);
                }
            });

            farmingplayerstoremove.Clear();

            await farmingplayers.forEachAlternativeAsync(async player =>
            {
                if (await NAPI.Task.RunReturnAsync(() => player.HasData("farmingspot")))
                    await PlayerFarming(player, await NAPI.Task.RunReturnAsync(() => player.GetData<RXFarming>("farmingspot")));
            });
        }


        public async Task PlayerFarming(RXPlayer player, RXFarming farmingspot)
        {
            try
            {
                if (farmingspot == null)
                {
                    await player.SendNotify("Farming beendet!", 3500, "red");
                    await player.StopAnimationAsync();
                    player.Freezed = false;
                    farmingplayerstoremove.Add(player);
                    await NAPI.Task.RunAsync(() => player.ResetData("farmingspot"));
                    return;
                }
                if (farmingspot.RequiredItemId != 0 && player.Container.GetItemAmount((uint)farmingspot.RequiredItemId) < 1)
                {
                    await player.SendNotify("Für diese Farmingroute benötigst du ein/en " + ItemModelModule.ItemModels.Find(item => item.Id == farmingspot.RequiredItemId).Name + "!", 3500, "green");
                    return;
                }
                if (await player.GetIsInVehicleAsync() == true)
                {
                    await player.SendNotify("Farming beendet!", 3500, "red");
                    await player.StopAnimationAsync();
                    player.Freezed = false;
                    farmingplayerstoremove.Add(player);
                    await NAPI.Task.RunAsync(() => player.ResetData("farmingspot"));
                    return;
                }
                var farmcount = randy.Next(farmingspot.MinCount, farmingspot.MaxCount);

                if (!player.Container.CanInventoryItemAdded(ItemModelModule.ItemModels.Find(item => item.Id == farmingspot.ItemId), farmcount))
                {
                    await player.SendNotify("Dein Inventar ist zu voll...", 3500, "black");
                    await player.SendNotify("Farming beendet!", 3500, "red");
                    await player.StopAnimationAsync();
                    player.Freezed = false;
                    farmingplayerstoremove.Add(player);
                    await NAPI.Task.RunAsync(() => player.ResetData("farmingspot"));
                    return;
                }
                player.Container.AddItem(ItemModelModule.ItemModels.Find(item => item.Id == farmingspot.ItemId), farmcount);
                await player.SendNotify("Du hast erfolgreich " + farmcount + "x " + ItemModelModule.ItemModels.Find(item => item.Id == farmingspot.ItemId).Name + " gesammelt und baust Stress ab!", 3500, "green");
                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@mp_snowball", "pickup_snowball");

                if (player.Stress > 0)
                {
                    player.Stress--;
                }

            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        public async Task StartFarming(RXPlayer player, RXFarming farmingspot)
        {
            if (await player.GetDimensionAsync() != 0) return;
            if (await player.GetIsInVehicleAsync() == true) return;
            if (farmingplayerstoremove.Contains(player)) return;
            if (farmingspot == null) return;

            if (farmingplayers.Contains(player))
            {
                await player.SendNotify("Farming beendet!", 3500, "red");
                await player.StopAnimationAsync();
                player.Freezed = false;
                farmingplayers.Remove(player);
                await NAPI.Task.RunAsync(() => player.ResetData("farmingspot"));
            }
            else
            {
                if (farmingspot.RequiredItemId != 0 && player.Container.GetItemAmount((uint)farmingspot.RequiredItemId) < 1)
                {
                    await player.SendNotify("Für diese Farmingroute benötigst du ein/en " + ItemModelModule.ItemModels.Find(item => item.Id == farmingspot.RequiredItemId).Name + "!", 3500, "green");
                    return;
                }
                farmingplayers.Add(player);
                await player.SendNotify("Farming gestartet!");
                player.Freezed = true;
                await NAPI.Task.RunAsync(() => player.SetData("farmingspot", farmingspot));
            }
        }
    }
}