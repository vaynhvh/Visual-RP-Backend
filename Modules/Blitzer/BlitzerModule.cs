using Backend.Models;
using Backend.Modules.Crime;
using Backend.Modules.Discord;
using Backend.Modules.Faction;
using Backend.Modules.Jail;
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

namespace Backend.Modules.Blitzer
{
    class BlitzerModule : RXModule
    {
        public BlitzerModule() : base("Blitzer") { }

        public static List<DbBlitzer> Blitzers = new List<DbBlitzer>();

        public override async void LoadAsync()
        {
            using var db = new RXContext();

            Blitzers = await db.Blitzer.ToListAsync();

            foreach (var blitzer in Blitzers)
            {

                blitzer.Tolleranz = Convert.ToInt32(blitzer.SpeedLimit / 5); // 20%
                if (blitzer.Tolleranz < 15) blitzer.Tolleranz = 15;

                blitzer.Position = blitzer.RawPosition.ToPos();
                blitzer.ObjectPosition = blitzer.RawObjectPosition.ToPos();
                var mcb = await NAPI.Entity.CreateMCB(blitzer.Position, new Color(255, 140, 0), 0u, blitzer.Range, 7f, false, MarkerType.VerticalCylinder, false);


                mcb.ColShape.SetData("blitzer", blitzer);

                var mcb2 = await NAPI.Entity.CreateMCB(blitzer.ObjectPosition, new Color(255, 140, 0), 0u, 1.2f, 1.2f, false, MarkerType.VerticalCylinder, false);
                mcb2.ColShape.SetData("blitzerhacking", blitzer);

                if (blitzer.Active)
                {
                    await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(-6978462, blitzer.ObjectPosition.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, blitzer.Heading)));
                }
            }
        }

      

        [RXCommand("saveblitzer", 1)]
        public async Task saveblitzer(RXPlayer iPlayer, string[] args)
        {
            if (iPlayer == null) return;

            iPlayer.SetData("blitzercreate_pos", await iPlayer.GetPositionAsync());
        }


        [RXCommand("createblitzer", 1)]
        public async Task createblitzer(RXPlayer iPlayer, string[] args)
        {
            
            if (iPlayer == null) return;


            if (!Int32.TryParse(args[0], out int kmh)) return;
            if (!Int32.TryParse(args[1], out int range)) return;

            if (!iPlayer.HasData("blitzercreate_pos")) return;

            Vector3 colShapePos = iPlayer.GetData<Vector3>("blitzercreate_pos");
            int colShapeRange = range;

            string x = colShapePos.X.ToString().Replace(",", ".");
            string y = colShapePos.Y.ToString().Replace(",", ".");
            string z = colShapePos.Z.ToString().Replace(",", ".");

            var playerPos = await iPlayer.GetPositionAsync();
            var playerHeading = await iPlayer.GetHeadingAsync();


            string objx = playerPos.X.ToString().Replace(",", ".");
            string objy = playerPos.Y.ToString().Replace(",", ".");
            string objz = playerPos.Z.ToString().Replace(",", ".");

            using var db = new RXContext();


            await db.Blitzer.AddAsync(new DbBlitzer { Active = true, Heading = playerHeading, SpeedLimit = kmh, RawPosition = $"{x},{y},{z}", RawObjectPosition = $"{objx},{objy},{objz}", Range = colShapeRange });


            await db.SaveChangesAsync();


            await iPlayer.SendNotify("Blitzer angelegt!");
            return;
        }

        public override async Task OnMinute()
        {

            foreach (DbBlitzer blitzer in Blitzers)
            {
                if (blitzer.Hacked)
                {
                    if (blitzer.LastHacked.AddMinutes(30) < DateTime.Now)
                    {
                        blitzer.Hacked = false;

                        TeamModule.Teams.Find(x => x.IsState() == true).SendMessageToAllState("Gehackter Blitzer (" + blitzer.Id + ") ist nun wieder online!");
                        

                       
                    }
                }
            }
        }

        [RemoteEvent]
        public async Task hackBlitzer(RXPlayer player, bool success)
        {
            if (player == null) return;

            if (!player.HasData("blitzerhacking"))
            {
                return;
            }
            var blitzer = player.GetData<DbBlitzer>("blitzerhacking");

            player.Freezed = false;
            if (!success)
            {
                await player.SendNotify("Hacken fehlgeschlagen! Ein Signal könnte an das LSPD gesendet worden sein!", 5000, "red");
                TeamModule.Teams.Find(x => x.IsState() == true).SendMessageToAllState("Blitzer (" + blitzer.Id + ") sendet ein Notsignal! Blitzer wird manipuliert!");

            }
            else
            {
                await player.SendNotify("Hacken erfolgreich! Der Blitzer wird für 30 Minuten nicht mehr aktiv sein!", 5000, "green");
                blitzer.Hacked = true;
                blitzer.LastHacked = DateTime.Now;
            }
        }

        public override async Task OnColShape(RXColShape shape, RXPlayer player, bool enter)
        {


            if (shape.HasData("blitzerhacking"))
            {
                if (enter)
                {
                    if (player.Team.IsState() && player.InDuty) return;

                    DbBlitzer blitzer = shape.GetData<DbBlitzer>("blitzerhacking");
                    player.SetData("blitzerhacking", blitzer);
                }
                else
                {
                    if (player.HasData("blitzerhacking"))
                    {
                        player.ResetData("blitzerhacking");
                    }
                }
            }
            if (!enter)
            {
                if (shape.HasData("inBlitzerRange"))
                {
                    shape.ResetData("inBlitzerRange");
                }
            }


            if (enter)
            {
                if (shape.HasData("blitzer"))
                {
                    if (player.HasData("inBlitzerRange"))
                    {
                        player.ResetData("inBlitzerRange");
                        return;
                    }
                    player.SetData("inBlitzerRange", shape.GetData<DbBlitzer>("blitzer"));

                    DbBlitzer xBlitzer = Blitzers.Find(b => b.Id == shape.GetData<DbBlitzer>("blitzer").Id);
                    if (xBlitzer == null || !xBlitzer.Active && xBlitzer.Hacked)
                    {
                        player.ResetData("inBlitzerRange");
                        return;
                    }

                    // in Fahrzeug, kein cop medic oder regierung
                    if (await player.GetIsInVehicleAsync())
                    {
                        if (player.InAduty || await player.GetDimensionAsync() != 0)
                        {
                            player.ResetData("inBlitzerRange");
                            return;
                        }

                        // Nur wenn fahrer
                        if (await player.GetVehicleSeatAsync() == 0)
                        {
                            var sxVeh = (RXVehicle)await NAPI.Task.RunReturnAsync(() => player.Vehicle);
                            var sxVehPos = await sxVeh.GetPositionAsync();
                            // Z Koordinate < -3Blitzer (wegen Tunnel etc) oder > +10 (flugzeug bla)
                            if (sxVeh == null ||
                                sxVehPos.Z < (xBlitzer.Position.Z - 5.0f) || sxVehPos.Z > (xBlitzer.Position.Z + 5.0f))
                            {

                                player.ResetData("inBlitzerRange");
                                return;
                            }

                            if (player.InDuty) return;

                            if (await sxVeh.IsSirenActiveAsync())
                            {
                                player.ResetData("inBlitzerRange");
                                return;
                            }

                            if (player.HasData("BlitzerTimestamp"))
                            {
                                DateTime date = (DateTime)player.GetData<DateTime>("BlitzerTimestamp");
                                if (date.AddMinutes(1) >= DateTime.Now)
                                {
                                    player.ResetData("inBlitzerRange");
                                    player.ResetData("BlitzerTimestamp");
                                    return;
                                }
                            }

                            int speed = await sxVeh.GetSpeed(player) - 10;
                            if ((speed - xBlitzer.Tolleranz) > xBlitzer.SpeedLimit)
                            {
                                int differenz = speed - xBlitzer.SpeedLimit;
                                int wantedReasonId = 446; // Standard-Fall (0 - 20)

                                // 20-50 Überschreitung Strafe
                                if (differenz > 20 && differenz < 50)
                                {
                                    wantedReasonId = 447;
                                }
                                else if (differenz >= 50 && differenz < 100) // 50-100 Überschreitung Strafe
                                {
                                    wantedReasonId = 448;
                                }
                                else if (differenz > 100) // 100+ Überschreitung Strafe
                                {
                                    wantedReasonId = 449;
                                }

                                try
                                {
                                    player.SetData("BlitzerTimestamp", DateTime.Now);
                                    string wantedstring = $"{await sxVeh.GetDisplayNameAsync()} ({sxVeh.Id}) mit {speed}/{xBlitzer.SpeedLimit} geblitzt - {DateTime.Now.Hour}:{DateTime.Now.Minute} {DateTime.Now.Day}/{DateTime.Now.Month}/{DateTime.Now.Year}";
                                    using var db = new RXContext();

                                    var newcrimes = await db.NewCrimes.ToListAsync();
                                        var crimeModule = newcrimes.FirstOrDefault(x => x.i == wantedReasonId);
                                    if (crimeModule == null)
                                    {
                                        await player.SendNotify("There is a fehla! -_-");
                                        return;
                                    }

                                        var newCrime = new DbPlayerCrimes { CrimeId = crimeModule.i, PlayerId = player.Id, Uhrzeit = DateTime.Now.ToString("dd:MM:yyyy mm:HH"), OfficerId = 0 };
                                        await db.PlayerCrimes.AddAsync(newCrime);

                                    player.PlayerCrimes.Add(newCrime);
                                   
                                    await db.SaveChangesAsync();

                                    await player.SendNotify($"Dein Fahrzeug {await sxVeh.GetDisplayNameAsync()} | ({sxVeh.Id}) wurde mit {speed}/{xBlitzer.SpeedLimit} km/h geblitzt! (Tolleranz: {xBlitzer.Tolleranz} km/h einberechnet)", 10000, "white", "Blitzer");

                                    DiscordModule.Logs.Add(new DiscordLog("Bank", (await player.GetNameAsync()) + " wurde geblitzt! (km/h: " + speed + "/" + xBlitzer.SpeedLimit + ")", DiscordModule.Blitzer));

                                    await player.TriggerEventAsync("startScreenEffect", "MP_SmugglerCheckpoint", 3000, false);
                                    await player.TriggerEventAsync("startsoundplay", "Camera_Shoot", "Phone_Soundset_Franklin");
                                }
                                catch (Exception e)
                                {
                                    RXLogger.Print(e.Message);
                                    player.ResetData("inBlitzerRange");
                                    return;
                                }
                            }

                            player.ResetData("inBlitzerRange");
                            return;
                        }
                    }
                    else
                    {
                        player.ResetData("BlitzerTimestamp");
                        player.ResetData("inBlitzerRange");
                    }
                }
            }
            return;
        }

    }
}
