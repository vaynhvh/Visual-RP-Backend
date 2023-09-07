using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Inventory;
using Backend.Modules.Phone.Apps;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Workstation
{

    public class WorkstationObject
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "r")]
        public bool Running { get; set; }

        [JsonProperty(PropertyName = "rot")]
        public List<Workstationitem> sourceItems { get; set; }

        [JsonProperty(PropertyName = "prod")]
        public List<WorkstationitemRot> prodItems { get; set; }
        [JsonProperty(PropertyName = "p")]
        public uint Price { get; set; }

    }

    public class Workstation
    {

    }

    public class Workstationitem
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get;set; }
        [JsonProperty(PropertyName = "a")]
        public uint Amount { get; set; }

        [JsonProperty(PropertyName = "p")]
        public uint Price { get; set; }

        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }

        [JsonProperty(PropertyName = "m")]
        public uint Minutes { get; set; } = 15;
        [JsonProperty(PropertyName = "req")]
        public List<WorkstationitemRot> endItems { get; set; }
    }

    public class WorkstationitemRot
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }
        [JsonProperty(PropertyName = "a")]
        public uint Amount { get; set; }

        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }
        [JsonProperty(PropertyName = "m")]
        public uint Minutes { get; set; } = 15;

        [JsonProperty(PropertyName = "p")]
        public uint Price { get; set; }
    }

    class WorkstationModule : RXModule
    {
        public WorkstationModule() : base("Workstation", new RXWindow("WeaponFactory")) { }

        public static List<DbWorkstation> Workstations = new List<DbWorkstation>();

        public override async void LoadAsync()
        {
            using var db = new RXContext();
            foreach (var workstation in await db.Workstations.ToListAsync())
            {
                Workstations.Add(workstation);  
            }

            foreach (var workstation in await db.Workstations.ToListAsync())
            {
                var mcb = await NAPI.Entity.CreateMCB(workstation.RentPosition.ToPos(), new Color(255, 140, 0), 0u, 1.4f, 2.4f, false, MarkerType.HorizontalCircleSkinny, false);

                mcb.ColShape.Message = new RXMessage
                {
                    Text = $"Drücke E um die Farmingmaschine zu öffnen!",
                    Color = "green",
                    Duration = 3500,
                    Title = "Farmingmaschine (" + workstation.Name + ")",
                };

                mcb.ColShape.Action = async player => await OpenWorkStationRent(player, workstation);

                await NAPI.Task.RunAsync(() =>
                {
                    new NPC((PedHash)NAPI.Util.GetHashKey(workstation.RentPed), workstation.RentPosition.ToPos(), workstation.RentPedHeading, 0u);
                });

                var farminggps = GpsApp.gpsCategories.FirstOrDefault(x => x.Name == "Workstations");
                if (farminggps == null) return;

                if (GpsApp.gpsCategories.Find(x => x.Name == workstation.Name) != null) continue;

                farminggps.Locations.Add(new GPSPosition(workstation.Name, workstation.RentPosition.ToPos()));


            }
        }

        public async Task OpenWorkStationRent(RXPlayer player, DbWorkstation workstation)
        {
            using var db = new RXContext();

            var playerprocesses = await db.WorkstationsProcess.Where(x => x.PlayerId == player.Id && !x.Finished).ToListAsync();

            var prod = new List<WorkstationitemRot>();

            foreach (var workstationitem in playerprocesses)
            {
                if (workstationitem == null) continue;
                var time = (uint)15 - (uint)(DateTime.Now - workstationitem.StartTime).TotalMinutes;

                if (time < 0 || time > 15)
                {
                    time = 0;
                }

                prod.Add(new WorkstationitemRot()
                {
                    Id = workstationitem.Id,
                    Amount = workstationitem.OutputItemAmount,
                    Image = ItemModelModule.ItemModels.Find(x => x.Id == workstation.InputItemId).ImagePath,
                    Minutes = time,
                    Name = ItemModelModule.ItemModels.Find(x => x.Id == workstation.OutputItemId).Name,
                    Price = 0

                });
            }


            WorkstationObject workstationObject = new WorkstationObject { Price = (uint)workstation.RentPrice, Id = workstation.Id, Name = workstation.Name, prodItems = prod, Running = player.WorkstationRunning, sourceItems = new List<Workstationitem> { new Workstationitem { Price = (uint)workstation.RentPrice, endItems = new List<WorkstationitemRot> { new WorkstationitemRot { Id = workstation.InputItemId, Price = (uint)workstation.RentPrice, Image = ItemModelModule.ItemModels.Find(x => x.Id == workstation.InputItemId).ImagePath, Name = ItemModelModule.ItemModels.Find(x => x.Id == workstation.InputItemId).Name, Amount = workstation.InputItemAmount, Minutes = 15 } }, Id = workstation.OutputItemId, Amount = workstation.OutputItemAmount, Image = ItemModelModule.ItemModels.Find(x => x.Id == workstation.OutputItemId).ImagePath, Minutes = 15, Name = ItemModelModule.ItemModels.Find(x => x.Id == workstation.OutputItemId).Name } } };

            await this.Window.OpenWindow(player, workstationObject);

        }

        public override async Task OnMinute()
        {
            using var db = new RXContext();

            var dbPlayers = PlayerController.GetValidPlayers();

            foreach (var dbPlayer in dbPlayers)
            {
                if (dbPlayer.WorkstationId != 0)
                {
                    var playerprocesses = await db.WorkstationsProcess.Where(x => x.PlayerId == dbPlayer.Id && !x.Finished).ToListAsync();

                    foreach (var process in playerprocesses)
                    {
                        if (process == null) continue;
                        if ((DateTime.Now - process.StartTime).TotalMinutes > 15)
                        {
                            process.Finished = true;
                        }
                    }

                }
            }
     

        }

        public static async Task LoadPlayerWorkstationPoints(RXPlayer player, uint oldworkstation)
        {

            if (player.WorkstationId == 0) return;

            var workstation = Workstations.Find(x => x.Id == player.WorkstationId);
            if (workstation == null) return;

        }

        [RemoteEvent]
        public async Task BuildWeapon(RXPlayer player, int id, int itemid)
        {
            var workstation = Workstations.Find(x => x.Id == id);
            if (workstation == null) return;

            using var db = new RXContext();


            if (player.Container.GetItemAmount(workstation.InputItemId) <= workstation.InputItemAmount) {
                await player.SendNotify("Du hast davon nicht genug dabei!");
                return;
            }

            player.Container.RemoveItem((uint)workstation.InputItemId, (int)workstation.InputItemAmount);

            var newprocess = new DbWorkstationProcess() { Finished = false, StartTime = DateTime.Now, InputItemId = workstation.InputItemId, InputItemAmount = workstation.InputItemAmount, OutputItemAmount = workstation.OutputItemAmount, OutputItemId = workstation.OutputItemId, PlayerId = player.Id, WorkstationId = workstation.Id };

            await db.WorkstationsProcess.AddAsync(newprocess);

            await player.SendNotify("Vorgang wurde gestartet");


            var prod = new List<WorkstationitemRot>();
            var playerprocesses = await db.WorkstationsProcess.Where(x => x.PlayerId == player.Id && !x.Finished).ToListAsync();

            foreach (var workstationitem in playerprocesses)
            {
                if (workstationitem == null) continue;

                var time = (uint)15 - (uint)(DateTime.Now - workstationitem.StartTime).TotalMinutes;

                if (time < 0 || time > 15)
                {
                    time = 0;
                }

                prod.Add(new WorkstationitemRot()
                {
                    Id = workstationitem.Id,
                    Amount = workstationitem.OutputItemAmount,
                    Image = ItemModelModule.ItemModels.Find(x => x.Id == workstation.InputItemId).ImagePath,
                    Minutes = time,
                    Name = ItemModelModule.ItemModels.Find(x => x.Id == workstation.OutputItemId).Name,
                    Price = 0

                }) ;
            }


            await player.TriggerEventAsync("UpdateFactoryQueue", NAPI.Util.ToJson(prod));
            await db.SaveChangesAsync();


        }


        [RemoteEvent]
        public async Task TakeWeapon(RXPlayer player, int id, int itemid)
        {
            var workstation = Workstations.Find(x => x.Id == id);
            if (workstation == null) return;

            using var db = new RXContext();
            var playerprocesses = await db.WorkstationsProcess.FirstOrDefaultAsync(x => x.PlayerId == player.Id && x.Id == itemid);

            if (playerprocesses == null)
            {
                await player.SendNotify("Da ist wohl etwas schief gelaufen!");
                return;
            }

            var time = (uint)15 - (uint)(DateTime.Now - playerprocesses.StartTime).TotalMinutes;

            if (time < 0 || time > 15)
            {
                time = 0;
            }


            if (time != 0)
            {
                await player.SendNotify("Du kannst dies noch nicht abholen!");
                return;
            }

            player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Id == playerprocesses.OutputItemId), (int)playerprocesses.OutputItemAmount);

            db.WorkstationsProcess.Remove(playerprocesses);

            await db.SaveChangesAsync();
            await player.SendNotify("Du hast deine Ware entnommen!");


        }

    }
}
