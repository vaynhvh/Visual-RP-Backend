using Backend.Models;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Casino
{

    public class SlotMachineEvent
    {
        [JsonProperty(PropertyName = "machineId")] public uint Id { get; set; }
        [JsonProperty(PropertyName = "price")] public int Price { get; set; }
        [JsonProperty(PropertyName = "minprice")] public int MinPrice { get; set; }
        [JsonProperty(PropertyName = "maxprice")] public int MaxPrice { get; set; }
        [JsonProperty(PropertyName = "pricestep")] public int PriceStep { get; set; }
        [JsonProperty(PropertyName = "maxmultiple")] public int MaxMultiple { get; set; }

    }

    public enum CasinoAutomatType
    {
        SlotMachineKoksal,
        SlotMachineDiamond,
        Roulett,
        Wheel
    }
    public class CasinoAutomat
    {
        public uint Id { get; set; }
        public CasinoAutomatType Type { get; set; }
        public int Price { get; set; }
        public int MinPrice { get; set; }
        public int MaxPrice { get; set; }
        public int PriceStep { get; set; }
        public int MaxMultiple { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public int Radius { get; set; }
        public bool IsInUse { get; set; } = false;

    }
    public enum Status
    {
        WIN,
        LOSE
    }

    public class SlotMachineGame
    {
        [JsonProperty(PropertyName = "id")]
        public int Id { get; set; }
        [JsonIgnore]
        public int Einsatz { get; set; }
        [JsonProperty(PropertyName = "slot1")]
        public int Slot1 { get; set; }
        [JsonProperty(PropertyName = "slot2")]
        public int Slot2 { get; set; }
        [JsonProperty(PropertyName = "slot3")]
        public int Slot3 { get; set; }
        [JsonProperty(PropertyName = "winsum")]
        public int WinSum { get; set; }
        [JsonProperty(PropertyName = "status")]
        public Status Status { get; set; }
        [JsonIgnore]
        public uint KasinoDeviceId { get; set; }
        [JsonIgnore]
        public int Multiple { get; set; }
    }


    class CasinoModule : RXModule
    {
        public CasinoModule() : base("Casino", new RXWindow("SlotMachine")) { }

        public static List<CasinoAutomat> Automaten = new List<CasinoAutomat>();
        public static int CasinoGameId = 0;
        public static Dictionary<int, SlotMachineGame> SlotMachineGames = new Dictionary<int, SlotMachineGame>();


        private static Random random = new Random();

        public static float[,] Factors = new float[,]
        {
            {1.5f,  3,  250},
            {1,  1.5f,   30},
            {1,  1.5f,   15},
            {0,  1.5f,   10},
            {0,  1,       5},
            {0,  1,       2},
            {0,  0,       1},
            {0,  0,       0},
        };

        public override async void LoadAsync()
        {

            await NAPI.Entity.CreateMCB(new Vector3(923.9907, 47.09761, 81.10641), new Color(255, 140, 0), 0u, 2f, 2.4f, false, MarkerType.VerticalCylinder, true, 680, 4, "Casino");


            Automaten = new List<CasinoAutomat>
            {
                new CasinoAutomat
                {
                    Id = 1,
                    Type = CasinoAutomatType.SlotMachineKoksal,
                    Price = 100,
                    MinPrice = 100,
                    MaxPrice = 5000,
                    PriceStep = 500,
                    MaxMultiple = 2,
                    Position = new Vector3(976.46454, 27.438978, 75.741234),
                    Heading = 39.32f,
                    Radius = 2
                },
                new CasinoAutomat
                {
                    Id = 2,
                    Type = CasinoAutomatType.SlotMachineKoksal,
                    Price = 100,
                    MinPrice = 100,
                    MaxPrice = 5000,
                    PriceStep = 500,
                    MaxMultiple = 2,
                    Position = new Vector3(978.26294, 29.909403, 75.741234),
                    Heading = 84.49f,
                    Radius = 2
                },
                new CasinoAutomat
                {
                    Id = 3,
                    Type = CasinoAutomatType.SlotMachineKoksal,
                    Price = 100,
                    MinPrice = 100,
                    MaxPrice = 5000,
                    PriceStep = 500,
                    MaxMultiple = 2,
                    Position = new Vector3(978.5474, 32.336845, 75.741234),
                    Heading = 98.44f,
                    Radius = 2
                },
                  new CasinoAutomat
                {
                    Id = 4,
                    Type = CasinoAutomatType.SlotMachineDiamond,
                    Price = 2000,
                    MinPrice = 2000,
                    MaxPrice = 15000,
                    PriceStep = 1000,
                    MaxMultiple = 4,
                    Position = new Vector3(972.9581, 38.125114, 75.74132),
                    Heading = 170.52412f,
                    Radius = 2
                },
                      new CasinoAutomat
                {
                    Id = 5,
                    Type = CasinoAutomatType.SlotMachineDiamond,
                    Price = 2000,
                    MinPrice = 2000,
                    MaxPrice = 15000,
                    PriceStep = 1000,
                    MaxMultiple = 4,
                    Position = new Vector3(970.5692, 37.95828, 75.74132),
                    Heading = -155.40291f,
                    Radius = 2
                },

        new CasinoAutomat
                {
                    Id = 6,
                    Type = CasinoAutomatType.SlotMachineDiamond,
                    Price = 2000,
                    MinPrice = 2000,
                    MaxPrice = 15000,
                    PriceStep = 1000,
                    MaxMultiple = 4,
                    Position = new Vector3(968.7263, 36.966187, 75.74132),
                    Heading = -134.98843f,
                    Radius = 2
                },
        new CasinoAutomat
                {
                    Id = 6,
                    Type = CasinoAutomatType.SlotMachineDiamond,
                    Price = 2000,
                    MinPrice = 2000,
                    MaxPrice = 15000,
                    PriceStep = 1000,
                    MaxMultiple = 4,
                    Position = new Vector3(968.10016, 27.013466, 75.7413),
                    Heading = -44.019924f,
                    Radius = 2
                },
          new CasinoAutomat
                {
                    Id = 7,
                    Type = CasinoAutomatType.SlotMachineKoksal,
                    Price = 100,
                    MinPrice = 100,
                    MaxPrice = 5000,
                    PriceStep = 500,
                    MaxMultiple = 2,
                    Position = new Vector3(970.1829, 25.822664, 75.7413),
                    Heading = -1.6f,
                    Radius = 2
                },
            new CasinoAutomat
                {
                    Id = 8,
                    Type = CasinoAutomatType.SlotMachineDiamond,
                    Price = 2000,
                    MinPrice = 2000,
                    MaxPrice = 15000,
                    PriceStep = 1000,
                    MaxMultiple = 4,
                    Position = new Vector3(966.4471, 29.10723, 75.741264),
                    Heading = -57.27f,
                    Radius = 2
                },
              new CasinoAutomat
                {
                    Id = 9,
                    Type = CasinoAutomatType.SlotMachineKoksal,
                    Price = 100,
                    MinPrice = 100,
                    MaxPrice = 5000,
                    PriceStep = 500,
                    MaxMultiple = 2,
                    Position = new Vector3(972.4523, 25.406317, 75.74132),
                    Heading = 8.8f,
                    Radius = 2
                },
                new CasinoAutomat
                {
                    Id = 10,
                    Type = CasinoAutomatType.SlotMachineDiamond,
                    Price = 2000,
                    MinPrice = 2000,
                    MaxPrice = 15000,
                    PriceStep = 1000,
                    MaxMultiple = 4,
                    Position = new Vector3( 974.12177, 25.75843, 75.74132),
                    Heading = 22.41f,
                    Radius = 2
                },
            };

            foreach (CasinoAutomat automat in Automaten)
            {
                if (automat.Type == CasinoAutomatType.SlotMachineKoksal)
                {
                    await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("ch_prop_casino_slot_08a"), automat.Position.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, automat.Heading - 180f)));

                    var mcb = await NAPI.Entity.CreateMCB(automat.Position, new Color(255, 140, 0), 0u, 1.4f, 1.4f, false, MarkerType.VerticalCylinder, false); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

                    mcb.ColShape.Message = new RXMessage
                    {
                        Text = "Benutze E um den Slotautomaten zu benutzen!",
                        Color = "orange",
                        Duration = 3500,
                        Title = "Casino - Slots",
                    };

                    mcb.ColShape.Action = async player => await UseAutomat(player, automat);
                }
                if (automat.Type == CasinoAutomatType.SlotMachineDiamond)
                {
                    await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("ch_prop_casino_slot_04a"), automat.Position.Subtract(new Vector3(0, 0, 1)), new Vector3(0, 0, automat.Heading - 180f)));

                    var mcb = await NAPI.Entity.CreateMCB(automat.Position, new Color(255, 140, 0), 0u, 1.4f, 1.4f, false, MarkerType.VerticalCylinder, false); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

                    mcb.ColShape.Message = new RXMessage
                    {
                        Text = "Benutze E um den Slotautomaten zu benutzen!",
                        Color = "#b9f2ff",
                        Duration = 3500,
                        Title = "Casino - Slots",
                    };

                    mcb.ColShape.Action = async player => await UseAutomat(player, automat);
                }
            }
            
        }

        public async Task UseAutomat(RXPlayer player, CasinoAutomat automat)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if (automat.IsInUse)
            {
                await player.SendNotify("Dieser Automat wird bereits benutzt!");
            }
            else
            {
                if (player.Cash < automat.MinPrice * 5)
                {
                    await player.SendNotify($"Sie benötigen mindestens ${automat.MinPrice * 5} um hier zu spielen!");
                }

                var obj = new SlotMachineEvent { Id = automat.Id, Price = automat.Price, MaxMultiple = automat.MaxMultiple, MaxPrice = automat.MaxPrice, MinPrice = automat.MinPrice, PriceStep = automat.PriceStep};
                await this.Window.OpenWindow(player, obj);
                automat.IsInUse = true;
            }
        }

        [RemoteEvent]
        public async Task requestSlotInfo(RXPlayer dbPlayer)
        {
            if (dbPlayer == null) return;
            await this.Window.TriggerEvent(dbPlayer, "responseSlotInfo", JsonConvert.SerializeObject(Factors));
        }


        [RemoteEvent]
        public async Task newSlotRoll(RXPlayer dbPlayer, int moneyUsed)
        {
            if (dbPlayer == null) return;
            if (dbPlayer.Container.GetItemAmount(36) > 0)
            {
                dbPlayer.Container.RemoveItem(36, 1);
                SlotMachineGame slotMachineGame = GenerateSlotMachineGame(dbPlayer, moneyUsed);
                await SendGameResultToPlayer(dbPlayer, slotMachineGame);
                return;
            } else if (await dbPlayer.TakeMoney(moneyUsed))
                {
                    SlotMachineGame slotMachineGame = GenerateSlotMachineGame(dbPlayer, moneyUsed);
                    await SendGameResultToPlayer(dbPlayer, slotMachineGame);
                    return;
                }
                

                await dbPlayer.SendNotify("Du hast nicht genug Geld?");

        }

        [RemoteEvent]
        public void leaveSlotMachine(RXPlayer dbPlayer, int deviceId)
        {
            if (dbPlayer == null) return;

            CasinoAutomat kasinoDevice = Automaten.Find(x => x.Id == deviceId);
            if (kasinoDevice == null) return;
            kasinoDevice.IsInUse = false;
        }

        [RemoteEvent]
        public async Task cashoutSlotRoll(RXPlayer dbPlayer, int id)
        {
            if (dbPlayer == null) return;

            if (SlotMachineGames.TryGetValue(id, out SlotMachineGame slotMachineGame))
            {
                if (slotMachineGame.Status == Status.LOSE) return;
                slotMachineGame.WinSum = slotMachineGame.WinSum * slotMachineGame.Multiple;
                await dbPlayer.GiveMoney(slotMachineGame.WinSum);
                await dbPlayer.SendNotify($"Du hast gewonnen! Du erhälst {slotMachineGame.WinSum}$");
                SlotMachineGames.Remove(id);
            }
        }

        [RemoteEvent]
        public async Task risikoCard(RXPlayer dbPlayer, int number, int id)
        {
            if (dbPlayer == null) return;

            int rndnumber = random.Next(1, 3);
            if (SlotMachineGames.TryGetValue(id, out SlotMachineGame slotMachineGame))
            {
                if (rndnumber == number)
                {
                    slotMachineGame.Multiple++;
                    await this.Window.TriggerEvent(dbPlayer, "responseRisiko", rndnumber.ToString(), "1");
                    return;
                }
                else
                {
                    slotMachineGame.Multiple = 0;
                    slotMachineGame.WinSum = 0;
                    slotMachineGame.Status = Status.LOSE;
                    await this.Window.TriggerEvent(dbPlayer, "responseRisiko", rndnumber.ToString(), "0");

                }
            }
        }

        public async Task SendGameResultToPlayer(RXPlayer dbPlayer, SlotMachineGame slotMachineGame)
        {
            await this.Window.TriggerEvent(dbPlayer, "rollSlot", JsonConvert.SerializeObject(slotMachineGame));

        }

        public SlotMachineGame GenerateSlotMachineGame(RXPlayer dbPlayer, int moneyUsed)
        {
            int multiple = 1;
            int slot1 = random.Next(1, 9);
            int slot2 = random.Next(1, 101);
            if (slot2 <= 27)
            {
                slot2 = slot1;
            }
            else
            {
                slot2 = random.Next(1, 9);
                while (slot2 == slot1)
                {
                    slot2 = random.Next(1, 9);
                }
            }
            int slot3 = random.Next(1, 101);
            if (slot3 <= 2)
            {
                slot3 = slot2;
            }
            else
            {
                slot3 = random.Next(1, 9);
                while (slot3 == slot2)
                {
                    slot3 = random.Next(1, 9);
                }
            }

            Status status = Status.LOSE;
            float winSum = calculateProfit(slot1, slot2, slot3, moneyUsed);

            if (winSum != 0.0f)
            {
                status = Status.WIN;
            }

            SlotMachineGame slotMachineGame = new SlotMachineGame()
            {
                Id = CasinoGameId,
                Einsatz = moneyUsed,
                KasinoDeviceId = 1,
                Slot1 = slot1,
                Slot2 = slot2,
                Slot3 = slot3,
                Status = status,
                WinSum = (int)winSum,
                Multiple = multiple
            };

            SlotMachineGames.Add(CasinoGameId, slotMachineGame);
            if (status == Status.LOSE)
            {
                //log if want

            }
            CasinoGameId++;

            return slotMachineGame;
        }

        public float calculateProfit(int slot1, int slot2, int slot3, int einsatz)
        {
            float profit = 0f;
            float profitFactor = 0f;


            if (slot1 == slot2)
            {
                if (slot1 == slot3)
                {
                    profitFactor = Factors[slot1 - 1, 2];
                }
                else
                {
                    profitFactor = Factors[slot1 - 1, 1];
                }
            }
            else
            {
                profitFactor = Factors[slot1 - 1, 0];
            }
            profit = profitFactor * einsatz;
            return profit;
        }
    }
}
