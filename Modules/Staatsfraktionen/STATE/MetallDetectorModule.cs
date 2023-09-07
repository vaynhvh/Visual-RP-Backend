using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Laptop;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Staatsfraktionen.STATE
{ 
    
    class MetallDetectorModule : RXModule
    {
        public MetallDetectorModule() : base("MetallDetector") { }

        public static List<DbMetallDetector> MetallDetectors = new List<DbMetallDetector>();


        public override async void LoadAsync()
        {
            using var db = new RXContext();

            foreach (var detector in await db.MetallDetectors.ToListAsync())
            {
                MetallDetectors.Add(detector);
            }

            foreach (var detector in MetallDetectors)
            {
                var mcb = await NAPI.Entity.CreateMCB(detector.Position.ToPos(), new Color(255, 140, 0), 0u, detector.Range, detector.Range, false, MarkerType.UpsideDownCone);


                mcb.ColShape.SetData<DbMetallDetector>("detector", detector);
            }
        }

        [RXCommand("createdetector", 1)]
        public async Task createdetector(RXPlayer player, string[] args)
        {
            using var db = new RXContext();
            var ppos = await player.GetPositionAsync();
            var detector = new DbMetallDetector { LastDetected= DateTime.Now, Position = ppos.FromPos(), Range = float.Parse(args[0]) };
            MetallDetectors.Add(detector);
            db.MetallDetectors.Add(detector);

            await db.SaveChangesAsync();
            var mcb = await NAPI.Entity.CreateMCB(detector.Position.ToPos(), new Color(255, 140, 0), 0u, detector.Range, detector.Range, false, MarkerType.UpsideDownCone);
            mcb.ColShape.SetData<DbMetallDetector>("detector", detector);

            await player.SendNotify("Metalldetector erstellt!");



        }

        public override async Task OnColShape(RXColShape shape, RXPlayer player, bool state)
        {

            if (shape.HasData("detector"))
            {
                if (state)
                {
                    var detector = shape.GetData<DbMetallDetector>("detector");

                    if (detector == null) return;


                    bool validWeapon = false;
                    foreach (var wp in player.Weapons)
                    {
                            validWeapon = true;
                            break;
                    }

                    if (!validWeapon)
                    {
                        foreach (var item in player.Container.Slots.ToList())
                        {
                            if (item != null && item.Model != null && !string.IsNullOrEmpty(item.Model.WeaponHash))
                            {
                                validWeapon = true;
                                break;
                            }
                        }
                    }

                    if (validWeapon && detector.LastDetected.AddSeconds(5) <= DateTime.Now)
                    {
                        detector.LastDetected = DateTime.Now;
                        foreach (RXPlayer xPlayer in await PlayerController.GetPlayersInRange(await player.GetPositionAsync(), 10.0f))
                        {
                            await xPlayer.SendNotify($"1337Allahuakbar$detector", 3000);
                        }
                    }

                }
            }

        }

    }
}
