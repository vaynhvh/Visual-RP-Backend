using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Bank;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Gas
{
    public class RXGasStation
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public int Price { get; set; } = 2;
        public Vector3 Position { get; set; }
        public float colshapeHeight { get; set; }
        public float colshapeRadius { get; set; }

        public RXGasStation(uint id, string name, Vector3 position, float colshape_height, float colshape_radius, int price = 2)
        {
            Id = id;
            Name = name;
            Price = price;
            Position = position;
            colshapeHeight = colshape_height;
            colshapeRadius = colshape_radius;
        }
    }

    class GasStationModule : RXModule
    {
        public GasStationModule() : base("GasStation") { }

        public static List<RXGasStation> GasStations = new List<RXGasStation>();

        int fueltime = 310;
        string image = "https://i.imgur.com/7Fpz5Ia.png";

        //[HandleExceptions]
        public override async void LoadAsync()
        {
            GasStations = new List<RXGasStation>
            {
                new RXGasStation(1, "Davis", new Vector3(-70.41652, -1761.2568, 29.64248), 10f, 10f),
                new RXGasStation(2, "Cypress", new Vector3(1208.6106, -1402.6562, 35.224197), 8f, 10f),
                new RXGasStation(3, "Mirrorpark", new Vector3(1180.2153, -329.7683, 69.315926), 10f, 10f),
                new RXGasStation(4, "Vinewood", new Vector3(620.35205, 268.64377, 103.08939), 10f, 10f),
                new RXGasStation(5, "Rockford", new Vector3(-1437.239, -276.333, 46.207615), 10f, 10f),
                new RXGasStation(6, "Pacific Bluffs", new Vector3(-2096.6914, -318.8345, 13.168586), 12f, 10f),
                new RXGasStation(7, "Vespucci", new Vector3(-723.3479, -935.2183, 19.211924), 10f, 10f),
                new RXGasStation(8, "Little Soul", new Vector3(-526.2792, -1211.0177, 18.184813), 8f, 10f),
                new RXGasStation(9, "Richman", new Vector3(-1799.6549, 802.682, 138.6446), 10f, 10f),
                new RXGasStation(10, "Lago", new Vector3(-2555.2903, 2334.3816, 33.078045), 10f, 10f),
                new RXGasStation(11, "Grand-Senora", new Vector3(49.418713, 2778.793, 58.03963), 6f, 10f),
                new RXGasStation(12, "Harmony", new Vector3(263.566, 2607.2358, 44.981438), 6f, 10f),
                new RXGasStation(13, "Harmony-East", new Vector3(1207.2678, 2659.8923, 37.89936), 6f, 10f),
                new RXGasStation(14, "Windpark", new Vector3(2539.1255, 2594.771, 37.944813), 6f, 10f),
                new RXGasStation(15, "Grand-Senora-Nord", new Vector3(2680.2976, 3264.509, 55.389294), 8f, 10f),
                new RXGasStation(16, "Alamosee", new Vector3(2005.0895, 3774.1987, 32.403923), 10f, 10f),
                new RXGasStation(17, "Mount Gordo", new Vector3(1702.24, 6415.446, 32.762947), 8f, 10f),
                new RXGasStation(18, "San Andreas", new Vector3(179.62991, 6602.9116, 31.86817), 10f, 10f),
                new RXGasStation(19, "Pillbox", new Vector3(265.12427, -1262.2485, 29.292933), 14f, 10f),
                new RXGasStation(20, "Versace", new Vector3(2580.9702, 361.63037, 108.468834), 10f, 10f),
            };

            GasStations.ForEach(x =>
            {
                NAPI.Task.Run(() =>
                {
                    var blip = NAPI.Blip.CreateBlip(361, x.Position, 1.0f, 64, "Tankstelle", 255, 0, true, 0, 0);
                    var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(x.Position, x.colshapeRadius, x.colshapeHeight, 0);
                   // var marker = NAPI.Marker.CreateMarker(1, x.Position, new Vector3(), new Vector3(), x.colshapeRadius*2, new Color(255, 255, 255));

                    colShape.SetData("GasStation", x.Id);
                });
            });

            await OnHour();
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task DoRefuel(RXPlayer player, uint gasid, uint nu, uint fuel, uint vehicleid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            var vehicle = await NAPI.Task.RunReturnAsync(() => VehicleController.GetClosestVehicle(player.Position, 8.0f));
            if (vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;


            uint stationId = await NAPI.Task.RunReturnAsync(() => gasid);
            if (stationId == 0)
            {
                await player.SendNotify("Du musst an einer Tankstelle sein!");
                return;
            }

            RXGasStation gasStation = GasStations.FirstOrDefault(x => x.Id == stationId);
            if (gasStation == null)
            {
                await player.SendNotify("Du musst an einer Tankstelle sein!");
                return;
            }

            if (int.TryParse(fuel.ToString(), out int liter))
            {
                if (liter < 1 || liter > (vehicle.ModelData == null ? 100 : vehicle.ModelData.Fuel))
                {
                    await player.SendNotify("Falsche Literangabe!");
                    return;
                }

                var fuelToGet = Convert.ToInt32((vehicle.ModelData == null ? 100 : vehicle.ModelData.Fuel) - vehicle.Fuel);

                if (liter > fuelToGet)
                {
                    await player.SendNotify("So viel passt nicht in deinen Tank rein!");
                    return;
                }

                var pricePerLiter = gasStation.Price;

                var price = Convert.ToInt32(pricePerLiter * liter);
                if (price == 0) price = 1;

                if (fuelToGet <= 0 || fuelToGet < 1)
                {
                    await player.SendNotify("Das Fahrzeug ist voll und kann nicht betankt werden.");
                    return;
                }

                PaymentModule.CreatePayment(player, price, async player =>
                {
                    await RX.GiveMoneyToStaatskonto(price, gasStation.Name + " Tankstelle - " + player.Id);

                    vehicle.Fuel += liter;

                    await player.SendNotify("Sie haben " + liter + " Liter für " + price.FormatMoneyNumber() + " getankt! ($" + pricePerLiter + "/Liter)", 3500, "green", "Tankstelle");

                }, gasStation.Name + " Tankstelle");
            }
            else
            {
                await player.SendNotify("Falsche Literangabe!");
                return;
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task Refuel(RXPlayer player, RXVehicle vehicle)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1) || vehicle == null || (vehicle.ModelData == null && vehicle.TeamId == 0)) return;

            var positionPlayer = await NAPI.Task.RunReturnAsync(() => player.Position);
            var positionVehicle = await NAPI.Task.RunReturnAsync(() => vehicle.Position);

            if (positionPlayer.DistanceTo(positionVehicle) > 20f) return;

            if (!await NAPI.Task.RunReturnAsync(() => player.HasData("stationId")))
            {
                await player.SendNotify("Du musst an einer Tankstelle sein!");
                return;
            }

            uint stationId = await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("stationId"));
            if (stationId == 0)
            {
                await player.SendNotify("Du musst an einer Tankstelle sein!");
                return;
            }

            RXGasStation gasStation = GasStations.FirstOrDefault(x => x.Id == stationId);
            if (gasStation == null)
            {
                await player.SendNotify("Du musst an einer Tankstelle sein!");
                return;
            }

            if (await NAPI.Task.RunReturnAsync(() => vehicle.EngineStatus) || await NAPI.Task.RunReturnAsync(() => vehicle.Locked))
            {
                await player.SendNotify("Der Motor muss abgeschaltet und das Fahrzeug aufgeschlossen sein!");
                return;
            }

            int vehicleFuelNeeded = (vehicle.ModelData == null ? 100 : vehicle.ModelData.Fuel) - (int)Math.Ceiling(vehicle.Fuel);
            if (vehicleFuelNeeded <= 0)
            {
                await player.SendNotify("Das Fahrzeug ist bereits vollgetankt!", 3500, "red", "Tankstelle");
                return;
            }

            object fuelstation = new
            {
                i = gasStation.Id,
                n = gasStation.Name,
                g = gasStation.Id,
                v = vehicle.Id,
                f = vehicle.ModelData.Fuel,
                p = gasStation.Price,
            };



            var textInputBox = new RXWindow("FuelStation");

            await textInputBox.OpenWindow(player, fuelstation);
        }

        //[HandleExceptions]
        public override async Task OnColShape(RXColShape shape, RXPlayer player, bool state)
        {
            if (await NAPI.Task.RunReturnAsync(() => !shape.HasData("GasStation")) || !player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead) return;

            uint stationId = await NAPI.Task.RunReturnAsync(() => shape.GetData<uint>("GasStation"));
            if (stationId == 0) return;

            RXGasStation gasStation = GasStations.FirstOrDefault(x => x.Id == stationId);
            if (gasStation == null) return;

            if (state)
            {

                await NAPI.Task.RunAsync(() => player.SetData("stationId", stationId));

                player.SendInfocard(gasStation.Name + " Tankstelle", "red", image, 12000, 0, new List<RXPlayer.InfoCardData>
                {
                    new RXPlayer.InfoCardData{ key = "Preis", value = gasStation.Price + "$/L" }
                });
            }
            else
            {
                await NAPI.Task.RunAsync(() => player.ResetData("stationId"));
            }
        }

        //[HandleExceptions]
        public override async Task OnHour()
        {
            await GasStations.forEachAlternativeAsync(x =>
            {
                x.Price = new Random().Next(1, 4);
            });
        }
    }
}
