using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Inventory;
using Backend.Modules.Vehicle;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Meth
{
    class MethModule : RXModule
    {
        public MethModule() : base("Meth") { }


        public static Vector3 CamperInteriorPosition = new Vector3(1973.07, 3816.15, 33.4287);
        public static Vector3 CamperPassiveCookPosition = new Vector3(1976.8535, 3819.4421, 33.450047);

        public static float CamperDrugAirRange = 60.0f;

        public static float DrugLabIncreaseRange = 20.0f;

        public static List<RXPlayer> CookingPlayers = new List<RXPlayer>();

        public static List<RXVehicle> CookingVehicles = new List<RXVehicle>();
        public static List<RXVehicle> PassiveCookingVehicles = new List<RXVehicle>();

        public override void LoadAsync()
        {
            CookingPlayers = new List<RXPlayer>();
            CookingVehicles = new List<RXVehicle>();
            PassiveCookingVehicles = new List<RXVehicle>();
        }


        public override async Task OnPlayerDisconnect(RXPlayer player, DisconnectionType type, string reason)
        {

            if (player.HasData("IsInCamper"))
            {
                uint playerdim = await player.GetDimensionAsync();

                var vehicle = VehicleController.FindVehicleById(playerdim);
                await player.SetDimensionAsync(0);
                if (vehicle != null)
                {
                    Vector3 vehpos = await vehicle.GetPositionAsync();

                    await player.SetPositionAsync(new Vector3(vehpos.X + 2, vehpos.Y,
                    vehpos.Z));
                }
                else
                {
                    if (!player.HasData("CamperEnterPos")) return;
                    Vector3 enterPosition = player.GetData<Vector3>("CamperEnterPos");
                    await player.SetPositionAsync(new Vector3(enterPosition.X + 2, enterPosition.Y,
                        enterPosition.Z));
                }
            }
        }

        public override async Task OnTenMinute()
        {
            Dictionary<Vector3, string> Messages = new Dictionary<Vector3, string>();
            var sendMethVehs = new List<uint>();
            foreach (var iVehicle in PassiveCookingVehicles.ToList())
            {
                if (iVehicle == null)
                    continue;
                var random = new Random();

                //Meth Cooking
                if (iVehicle.HasData("passivecooking"))
                {
                    Vector3 playerpos = await iVehicle.GetPositionAsync();


                    if (iVehicle.Container.GetItemAmount("Batterie") > 0 || iVehicle.Container.GetItemAmount("Gehäutete Köten") > 0 || iVehicle.Container.GetItemAmount("Toilettenreiniger") > 0)
                    {
                        var explode = random.Next(1, 30);
                        if (explode < 5)
                        {
                            iVehicle.Container.RemoveItem(28, 1);


                            iVehicle.ResetData("passivecooking");

                            if (PassiveCookingVehicles.Contains(iVehicle)) PassiveCookingVehicles.Remove(iVehicle);

                            var journeyDbId = await iVehicle.GetDimensionAsync();
                            if (!sendMethVehs.Contains(journeyDbId))
                            {
                                sendMethVehs.Add(journeyDbId);
                                var sxveh = VehicleController.FindVehicleById(journeyDbId);
                                if (sxveh != null)
                                {
                                    Messages.Add(await sxveh.GetPositionAsync(), "1337Allahuakbar$explode");
                                }
                            }
                        }
                        else
                        {
                            var meth = random.Next(8, 15); //3 included, 7 excluded

                            iVehicle.Container.RemoveItem(27, 1);
                            iVehicle.Container.RemoveItem(45, 1);
                            iVehicle.Container.RemoveItem(25, 1);


                            if (!iVehicle.Container.CanInventoryItemAdded(ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == 74), meth))
                            {
                                iVehicle.ResetData("cooking");
                                if (PassiveCookingVehicles.Contains(iVehicle)) PassiveCookingVehicles.Remove(iVehicle);
                            }
                            else
                            {
                                iVehicle.Container.AddItem(ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == 74), meth);

                                var journeyDbId = await iVehicle.GetDimensionAsync();
                                if (!sendMethVehs.Contains(journeyDbId))
                                {
                                    sendMethVehs.Add(journeyDbId);
                                    var sxveh = VehicleController.FindVehicleById(journeyDbId);

                                    if (sxveh != null)
                                    {
                                        if (!CookingVehicles.Contains(sxveh)) CookingVehicles.Add(sxveh);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        iVehicle.ResetData("passivecooking");
                        if (PassiveCookingVehicles.Contains(iVehicle)) PassiveCookingVehicles.Remove(iVehicle);
                    }
                }
            }

            // Send Messages together....
            foreach (var xPlayer in PlayerController.GetValidPlayers())
            {
                if (xPlayer == null) continue;
                foreach (KeyValuePair<Vector3, string> kvp in Messages)
                {
                    Vector3 ppos = await xPlayer.GetPositionAsync();
                    if (ppos.DistanceTo(kvp.Key) < 30)
                    {
                        await xPlayer.SendNotify(kvp.Value, 4000, "orange", "Leistungskurs Chemie");
                    }
                }
            }
        }

        public override async Task OnMinute()
        {
            CookingVehicles.Clear();
            Dictionary<Vector3, string> Messages = new Dictionary<Vector3, string>();
            var sendMethVehs = new List<uint>();
            foreach (var iPlayer in CookingPlayers.ToList())
            {
                if (iPlayer == null)
                    continue;
                var random = new Random();

                //Meth Cooking
                if (iPlayer.HasData("cooking") && iPlayer.HasData("IsInCamper"))
                {
                    Vector3 playerpos = await iPlayer.GetPositionAsync();

                    if (playerpos.DistanceTo(CamperInteriorPosition) > 40.0f)
                    {
                        iPlayer.ResetData("cooking");
                        if (CookingPlayers.Contains(iPlayer)) CookingPlayers.Remove(iPlayer);
                        iPlayer.Kick("Cheating. c:");
                        continue;
                    }
                    if (iPlayer.Container.GetItemAmount("Batterie") > 0 || iPlayer.Container.GetItemAmount("Gehäutete Köten") > 0 || iPlayer.Container.GetItemAmount("Toilettenreiniger") > 0)
                    {
                        var explode = random.Next(1, 30);
                        if (explode < 5)
                        {
                            await iPlayer.SendNotify("1337Allahuakbar$explode", 5000);
                            iPlayer.Container.RemoveItem(28, 1);

                            await iPlayer.SetHealthAsync(await iPlayer.GetHealthAsync() - 40);

                            iPlayer.ResetData("cooking");

                            await ModuleController.getClientDamage(iPlayer, 30);
                            if (CookingPlayers.Contains(iPlayer)) CookingPlayers.Remove(iPlayer);

                            var journeyDbId = await iPlayer.GetDimensionAsync();
                            if (!sendMethVehs.Contains(journeyDbId))
                            {
                                sendMethVehs.Add(journeyDbId);
                                var sxveh = VehicleController.FindVehicleById(journeyDbId);
                                if (sxveh != null)
                                {
                                    Messages.Add(await sxveh.GetPositionAsync(), "1337Allahuakbar$explode");
                                }
                            }
                        }
                        else
                        {
                            var meth = random.Next(8, 15); //3 included, 7 excluded

                            iPlayer.Container.RemoveItem(27, 1);
                            iPlayer.Container.RemoveItem(45, 1);
                            iPlayer.Container.RemoveItem(25, 1);

                            await iPlayer.SendNotify("Du hast die Chemikalien gut gemischt und du konntest erfolgreich " + meth + " Ephidrinkonzentrat herstellen!", 5000, "orange");

                            if (!iPlayer.Container.CanInventoryItemAdded(ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == 74), meth))
                            {
                                await iPlayer.SendNotify("Dein Inventar ist voll! Die Ephidrinkonzentrat werden in den Kofferraum deines Fahrzeugs gelegt...");
                                iPlayer.ResetData("cooking");
                                if (CookingPlayers.Contains(iPlayer)) CookingPlayers.Remove(iPlayer);
                            }
                            else
                            {
                                iPlayer.Container.AddItem(ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == 74), meth);

                                var journeyDbId = await iPlayer.GetDimensionAsync();
                                if (!sendMethVehs.Contains(journeyDbId))
                                {
                                    sendMethVehs.Add(journeyDbId);
                                    var sxveh = VehicleController.FindVehicleById(journeyDbId);

                                    if (sxveh != null)
                                    {
                                        if (!CookingVehicles.Contains(sxveh)) CookingVehicles.Add(sxveh);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        await iPlayer.SendNotify("Dein Kocher ist ausgegangen... Woran hats gelegen?");
                        iPlayer.ResetData("cooking");
                        if (CookingPlayers.Contains(iPlayer)) CookingPlayers.Remove(iPlayer);
                    }
                }
            }

            // Send Messages together....
            foreach (var xPlayer in PlayerController.GetValidPlayers())
            {
                if (xPlayer == null) continue;
                foreach (KeyValuePair<Vector3, string> kvp in Messages)
                {
                    Vector3 ppos = await xPlayer.GetPositionAsync();
                    if (ppos.DistanceTo(kvp.Key) < 30)
                    {
                        await xPlayer.SendNotify(kvp.Value, 4000, "orange", "Leistungskurs Chemie");
                    }
                }
            }
        }


        public override async Task PressedE(RXPlayer player)
        {

            uint playerdim = await player.GetDimensionAsync();


            if (player.HasData("IsInCamper") && playerdim != 0)
            {
                Vector3 playerpos = await player.GetPositionAsync();

                if (playerpos.DistanceTo(CamperPassiveCookPosition) < 2.5f)
                {
                    /*    var vehicle = VehicleController.FindVehicleById(playerdim);

                        if (vehicle.HasData("passivecooking"))
                        {
                            vehicle.ResetData("passivecooking");
                            PassiveCookingVehicles.Remove(vehicle);
                            await player.SendNotify("Du stoppst mit deinem Camper passiv zu kochen!", 5000, "red");

                        }
                        else
                        {
                            if (vehicle.Container.GetItemAmount("Kocher") < 1 || vehicle.Container.GetItemAmount("Batterie") < 1 || vehicle.Container.GetItemAmount("Ephedrinkonzentrat") < 1 || vehicle.Container.GetItemAmount("Toilettenreiniger") < 1)
                            {
                                await player.SendNotify("Du hast nicht genug Materialien im Kofferraum!", 5000, "green");
                                return;
                            }
                            vehicle.SetData<uint>("passivecooking", vehicle.Id);
                            PassiveCookingVehicles.Add(vehicle);
                            await player.SendNotify("Du beginnst nun mit deinem Camper passiv zu kochen!", 5000, "green");
                        }*/

                    var window = new RXWindow("Camper");

                    await window.OpenWindow(player);
                }
            }
        
    


            if (player.HasData("IsInCamper") && playerdim != 0)
            {
                Vector3 playerpos = await player.GetPositionAsync();

                if (playerpos.DistanceTo(CamperInteriorPosition) > 2.5f) return;
                var vehicle = VehicleController.FindVehicleById(playerdim);


                await player.SetDimensionAsync(0);

                if (vehicle != null)
                {
                    Vector3 vehpos = await vehicle.GetPositionAsync();


                    await player.SetPositionAsync(new Vector3(vehpos.X + 2, vehpos.Y,
                        vehpos.Z + 0.5f));

                    player.ResetData("CamperEnterPos");
                    player.ResetData("IsInCamper");
                } else
                {
                    Vector3 enterPosition = player.GetData<Vector3>("CamperEnterPos");
                    await player.SetPositionAsync(new Vector3(enterPosition.X + 2, enterPosition.Y,
                        enterPosition.Z));
                }


                return;
            }
            Vector3 ppos = await player.GetPositionAsync();


                var xVeh = await NAPI.Task.RunReturnAsync(() => VehicleController.GetClosestVehicle(player.Position));

                if (xVeh == null || xVeh.Id == 0) return;

                if (await NAPI.Task.RunReturnAsync(() => xVeh.Model != (uint)VehicleHash.Journey && xVeh.Model != (uint)VehicleHash.Camper)) return;

                if (await NAPI.Task.RunReturnAsync(() => xVeh.Locked)) return;

                player.SetData<bool>("IsInCamper", true);
                player.SetData<Vector3>("CamperEnterPos", ppos);
                await player.SetDimensionAsync(xVeh.Id);

               

                await player.SetPositionAsync(CamperInteriorPosition);

        }
        public override async Task PressedL(RXPlayer player)
        {
            uint playerdim = await player.GetDimensionAsync();

            if (player.HasData("IsInCamper") && playerdim != 0)
            {
                var vehicle = VehicleController.FindVehicleById(playerdim);


                if (vehicle == null) return;

                if (await NAPI.Task.RunReturnAsync(() => vehicle.Locked))
                {
                    await player.SendNotify("Fahrzeug aufgeschlossen!", 3500, "green");
                    vehicle.SetLocked(false);

                } else
                {
                    await player.SendNotify("Fahrzeug zugeschlossen!", 3500, "red");
                    vehicle.SetLocked(true);
                }


                return;
            }

        }

    }
}
