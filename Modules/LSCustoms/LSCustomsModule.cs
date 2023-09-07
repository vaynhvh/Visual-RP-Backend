using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Native;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.LSCustoms
{
    public class LSCustoms
    {
        public uint Id { get; set; }
        public string Name { get; set; }
        public uint Type { get; set; }
        public Vector3 Position { get; set; }

    }

    public class Tuning
    {
        public uint ID { get; }
        public string Name { get; }
        public int MaxIndex { get; }
        public int StartIndex { get; }

        public Tuning(uint p_ID, string p_Name, int p_StartIndex = -1, int p_MaxIndex = 20)
        {
            ID = p_ID;
            Name = p_Name;
            MaxIndex = p_MaxIndex;
            StartIndex = p_StartIndex;
        }
    }



    class LSCustomsModule : RXModule
    {
        public LSCustomsModule() : base("LSCustoms") { }

        public static List<LSCustoms> LSCustoms = new List<LSCustoms>();


        public static Dictionary<int, Tuning> TuningParts = new Dictionary<int, Tuning>
        {
            {1, new Tuning(0, "Spoiler", p_MaxIndex: 100)},
            {2, new Tuning(1, "Front Bumper", p_MaxIndex: 100)},
            {3, new Tuning(2, "Rear Bumper", p_MaxIndex: 100)},
            {4, new Tuning(3, "Side Skirt", p_MaxIndex: 100)},
            {5, new Tuning(4, "Exhaust", p_MaxIndex: 100)},
            {6, new Tuning(5, "Frame")},
            {7, new Tuning(6, "Grille")},
            {8, new Tuning(7, "Hood", p_MaxIndex: 100)},
            {9, new Tuning(8, "Fender")},
            {10, new Tuning(9, "Right Fender")},
            {11, new Tuning(10, "Roof")},
            {12, new Tuning(11, "Engine", p_MaxIndex: 3)},
            {13, new Tuning(12, "Brakes", p_MaxIndex: 2)},
            {14, new Tuning(13, "Transmission", p_MaxIndex: 2)},
            {15, new Tuning(14, "Horn", p_MaxIndex: 52)},
            {16, new Tuning(15, "Suspension", p_MaxIndex: 3)},
            {17, new Tuning(16, "Armor", p_MaxIndex: 4)},
            {18, new Tuning(18, "Turbo", p_MaxIndex: 0)},
            {19, new Tuning(22, "Xenon", p_MaxIndex: 0)},
            {20, new Tuning(23, "Front Wheels", p_MaxIndex: 250)},
            {21, new Tuning(24, "Back Wheels", p_MaxIndex: 250)},
            {22, new Tuning(27, "Trim Design")},
            {23, new Tuning(30, "Dials")},
            {24, new Tuning(33, "Steering Wheel")},
            {25, new Tuning(34, "Shift Lever")},
            {26, new Tuning(38, "Hydraulics")},
            {27, new Tuning(48, "Livery", p_MaxIndex: 100)},
            {28, new Tuning(46, "Window Tint", p_StartIndex: 0, p_MaxIndex: 5)},
            {29, new Tuning(80, "Headlight Color", p_MaxIndex: 12)},
            {30, new Tuning(81, "Numberplate")},
            {31, new Tuning(95, "Tire SmokeR")},
            {32, new Tuning(96, "Tire SmokeG")},
            {33, new Tuning(97, "Tire SmokeB")},
            {34, new Tuning(98, "Pearllack")},
            {35, new Tuning(99, "Felgenfarbe")},
        };

        public override async void LoadAsync()
        {
            LSCustoms.Add(new LSCustoms { Name = "Car-Color", Position = new Vector3(-327.18277, -144.41487, 39.060017), Type = 1 });
            LSCustoms.Add(new LSCustoms { Name = "Car-Color", Position = new Vector3(-1276.5485, -2975.232, -48.489796), Type = 1 });
            LSCustoms.Add(new LSCustoms { Name = "Pearl-Color", Position = new Vector3(-326.03162, -138.12791, 39.00965), Type = 2 });
            LSCustoms.Add(new LSCustoms { Name = "Pearl-Color", Position = new Vector3(-1276.7135, -3014.768, -48.49001), Type = 2 });
            LSCustoms.Add(new LSCustoms { Name = "Felgen-Color", Position = new Vector3(-324.3559, -132.60773, 38.96582), Type = 3 });
            LSCustoms.Add(new LSCustoms { Name = "Felgen-Color", Position = new Vector3(-1276.6848, -2995.5867, -48.489807), Type = 3 });
            LSCustoms.Add(new LSCustoms { Name = "Reifenrauch-Color", Position = new Vector3(-1257.5554, - 3034.3135, - 48.490284), Type = 4 });
            LSCustoms.Add(new LSCustoms { Name = "Reifenrauch-Color", Position = new Vector3(-1276.4573, - 3035.2393, - 48.490276), Type = 4 });
            LSCustoms.Add(new LSCustoms { Name = "Komplett-Tuning", Position = new Vector3(-338.11987, -135.52524, 39.00966), Type = 5 });

            var ff = await NAPI.Entity.CreateMCB(new Vector3(-348.22125, -129.79192, 39.009666), new GTANetworkAPI.Color(255, 140, 0), 0u, 5f, 5.4f, false, MarkerType.VerticalCylinder, true, 446, 0, "LS Customs");

            await LSCustoms.forEach(async lsc =>
            {
                var mcb = await NAPI.Entity.CreateMCB(lsc.Position, new GTANetworkAPI.Color(255, 140, 0), 0u, 5f, 5.4f, false, MarkerType.VerticalCylinder, false, 446, 0, "LS Customs");

                mcb.ColShape.Message = new RXMessage
                {
                    Text = $"Drücke E um auf {lsc.Name} zuzugreifen!",
                    Color = "olive",
                    Duration = 3500,
                    Title = "Los Santos Customs",
                };

                mcb.ColShape.Action = async player => await OpenLSCMenu(player, lsc);
            });
        }

        public async Task OpenLSCMenu(RXPlayer player, LSCustoms lsc)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync()) return;

            var nativeMenu = new NativeMenu("LSC Fahrzeugwahl", "", new List<NativeItem>()
            {
                new NativeItem("Schließen", player => player.CloseNativeMenu()),
            });

            RXVehicle vehicle = await player.GetVehicleAsync();
            if (vehicle == null || await player.GetVehicleSeatAsync() != 0) return;
            
            nativeMenu.Items.Add(new NativeItem(vehicle.Id + " | " + await vehicle.GetDisplayNameAsync(), async player =>
            {
                player.CloseNativeMenu();
                if (lsc.Type == 1)
                {
                    var textInputBox = new RXWindow("TextInputBox");

                    await textInputBox.OpenWindow(player, new
                    {
                        textBoxObject = new
                        {
                            Callback = "SetCarColorLSC",
                            Title = "Fahrzeugfarbe",
                            Message = "Gebe zwei Fahrzeugfarben an. (z.B 0 0)",
                        },
                    });
                } else if (lsc.Type == 2)
                {
                    var textInputBox = new RXWindow("TextInputBox");

                    await textInputBox.OpenWindow(player, new
                    {
                        textBoxObject = new
                        {
                            Callback = "SetCarPearlLSC",
                            Title = "Pearleffect",
                            Message = "Gebe die Farbe/Art an.",
                        },
                    });
                }
                else if (lsc.Type == 3)
                {
                    var textInputBox = new RXWindow("TextInputBox");

                    await textInputBox.OpenWindow(player, new
                    {
                        textBoxObject = new
                        {
                            Callback = "SetCarRimColorLSC",
                            Title = "Felgenfarbe",
                            Message = "Gebe die Farbe an.",
                        },
                    });
                } else if (lsc.Type == 4)
                {
                    var textInputBox = new RXWindow("TextInputBox");

                    await textInputBox.OpenWindow(player, new
                    {
                        textBoxObject = new
                        {
                            Callback = "SetCarTyreSmokeColorLSC",
                            Title = "Reifenrauch",
                            Message = "Gebe die Farbe im RGB Format an. (z.B 0 0 0)",
                        },
                    });
                } else if (lsc.Type == 5)
                {
                    await OpenLSCTuningMenu(player);
                }

            }));


            player.ShowNativeMenu(nativeMenu);
        }

        public async Task OpenLSCTuningMenu(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync()) return;

            RXVehicle vehicle = await player.GetVehicleAsync();
            if (vehicle == null || await player.GetVehicleSeatAsync() != 0) return;

            var nativeMenu = new NativeMenu("LSC Tuning", "", new List<NativeItem>()
            {
                new NativeItem("Schließen", player => player.CloseNativeMenu()),
            });
            var l_Tunings = TuningParts;
            foreach (var l_Tuning in l_Tunings)
            {
                if (l_Tuning.Value.ID >= 90)
                    continue;
                nativeMenu.Items.Add(new NativeItem(l_Tuning.Value.Name, async player =>
                {

                    await OpenLSCTuningMenuForCat(player, (int)l_Tuning.Value.ID);

                }));

            }


            player.ShowNativeMenu(nativeMenu);

        }

        public async Task OpenLSCTuningMenuForCat(RXPlayer player, int id)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync()) return;

            RXVehicle vehicle = await player.GetVehicleAsync();
            if (vehicle == null || await player.GetVehicleSeatAsync() != 0) return;

            player.CloseNativeMenu();

            var nativeMenu = new NativeMenu("Tuning", "", new List<NativeItem>()
            {
                new NativeItem("Schließen", player => player.CloseNativeMenu()),
                new NativeItem("Standard", player => player.CloseNativeMenu()),

            });

            Dictionary<int, int> l_Dic = new Dictionary<int, int>();


            Tuning tuning = TuningParts.Values.ToList().Where(tun => tun.ID == id).FirstOrDefault();
            if (tuning == null) return;

            for (var l_Itr = tuning.StartIndex + 1; l_Itr <= tuning.MaxIndex; l_Itr++)
            {
                nativeMenu.Items.Add(new NativeItem($"Anbringen Teil {l_Itr}", async player => await SetTuningPart(player, id, l_Itr)));
            }
            player.ShowNativeMenu(nativeMenu);

        }


        public async Task SetTuningPart(RXPlayer player, int id, int part)
        {


            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !await player.GetIsInVehicleAsync()) return;
            player.CloseNativeMenu();

            RXVehicle vehicle = await player.GetVehicleAsync();
            if (vehicle == null || await player.GetVehicleSeatAsync() != 0) return;

            if (player.HasData("inTuning"))
            {
                await player.SendNotify("Du tunest bereits ein Fahrzeug!");
                return;
            }
            player.SetData("inTuning", true);

            await player.SendProgressbar(10000);
            await Task.Delay(10000);
            await vehicle.ChangeAndSaveMod(id, part);

            await player.SendNotify("Tuning abgeschlossen.");
            player.ResetData("inTuning");
        }

            [RemoteEvent]
        public async Task SetCarColorLSC(RXPlayer player, string returnstring)
        {
            RXVehicle vehicle = await player.GetVehicleAsync();
            if (vehicle == null || await player.GetVehicleSeatAsync() != 0) return;

            if (returnstring.Length < 2 || !returnstring.Contains(" ")) return;

            string[] splittedReturn = returnstring.Split(" ");
            if (splittedReturn.Length != 2) return;

            if (!Int32.TryParse(splittedReturn[0], out int color1)) return;
            if (!Int32.TryParse(splittedReturn[1], out int color2)) return;

            if (player.HasData("inTuning"))
            {
                await player.SendNotify("Du tunest bereits ein Fahrzeug!");
                return;
            }
            player.SetData("inTuning", true);

            await player.SendProgressbar(10000);
            await Task.Delay(10000);

            vehicle.Color1 = color1;
            vehicle.Color2 = color2;

            await NAPI.Task.RunAsync(() => { vehicle.PrimaryColor = color1; vehicle.SecondaryColor = color2; });
            await player.SendNotify("Tuning abgeschlossen.");
            player.ResetData("inTuning");
        }

        [RemoteEvent]
        public async Task SetCarPearlLSC(RXPlayer player, string returnstring)
        {
            RXVehicle vehicle = await player.GetVehicleAsync();
            if (vehicle == null || await player.GetVehicleSeatAsync() != 0) return;

            if (!Int32.TryParse(returnstring, out int color1)) return;

            if (player.HasData("inTuning"))
            {
                await player.SendNotify("Du tunest bereits ein Fahrzeug!");
                return;
            }
            player.SetData("inTuning", true);

            await player.SendProgressbar(10000);
            await Task.Delay(10000);

            await vehicle.ChangeAndSaveMod(98, color1);

            await player.SendNotify("Tuning abgeschlossen.");
            player.ResetData("inTuning");
        }

        [RemoteEvent]
        public async Task SetCarRimColorLSC(RXPlayer player, string returnstring)
        {
            RXVehicle vehicle = await player.GetVehicleAsync();
            if (vehicle == null || await player.GetVehicleSeatAsync() != 0) return;

            if (!Int32.TryParse(returnstring, out int color1)) return;

            if (player.HasData("inTuning"))
            {
                await player.SendNotify("Du tunest bereits ein Fahrzeug!");
                return;
            }
            player.SetData("inTuning", true);

            await player.SendProgressbar(10000);
            await Task.Delay(10000);

            await vehicle.ChangeAndSaveMod(99, color1);

            await player.SendNotify("Tuning abgeschlossen.");
            player.ResetData("inTuning");
        }

        [RemoteEvent]
        public async Task SetCarTyreSmokeColorLSC(RXPlayer player, string returnstring)
        {
            RXVehicle vehicle = await player.GetVehicleAsync();
            if (vehicle == null || await player.GetVehicleSeatAsync() != 0) return;

            if (returnstring.Length < 3 || !returnstring.Contains(" ")) return;

            string[] splittedReturn = returnstring.Split(" ");
            if (splittedReturn.Length != 3) return;

            if (!Int32.TryParse(splittedReturn[0], out int color1)) return;
            if (!Int32.TryParse(splittedReturn[1], out int color2)) return;
            if (!Int32.TryParse(splittedReturn[2], out int color3)) return;

            if (player.HasData("inTuning"))
            {
                await player.SendNotify("Du tunest bereits ein Fahrzeug!");
                return;
            }
            player.SetData("inTuning", true);

            await player.SendProgressbar(10000);
            await Task.Delay(10000);

            if (color1 >= 255)
                color1 = 255;
            if (color2 >= 255)
                color2 = 255;
            if (color3 >= 255)
                color3 = 255;

            await vehicle.ChangeAndSaveMod(95, color1);
            await vehicle.ChangeAndSaveMod(96, color2);
            await vehicle.ChangeAndSaveMod(97, color3);

            await player.SendNotify("Tuning abgeschlossen.");
            player.ResetData("inTuning");
        }

    }
}
