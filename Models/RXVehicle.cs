using Backend.Controllers;
using Backend.Modules.Inventory;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static Backend.Models.RXContainer;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Models
{
    public class RXVehicle : Vehicle
    {
        public NetHandle VehicleHandle { get; set; }

        public RXVehicle(NetHandle handle) : base(handle)
        {
            VehicleHandle = handle;
        }

        public uint Id { get; set; } = 0;
        public RXVehicleModel ModelData { get; set; } = null;
        public double Fuel { get; set; } = 100;
        public int Color1 { get; set; }
        public int Color2 { get; set; }
        public uint OwnerId { get; set; } = 0;
        public uint TeamId { get; set; } = 0;
        public string TeamVehicleModel { get; set; } = "";
        public uint ContainerId { get; set; } = 0;
        public string Plate { get; set; } = "";
        public double Distance { get; set; } = 0;
        public double JobId { get; set; } = 0;
        public string LastDriver { get; set; }
        public bool TrunkOpen { get; set; } = false;



        //[HandleExceptions]
        public RXContainerObj Container
        {
            get
            {
                return ContainerModule.Containers.FirstOrDefault(x => x.Id == ContainerId);
            }
        }

        public Dictionary<int, int> Mods { get; set; } = new Dictionary<int, int>();
        public ConcurrentDictionary<uint, bool> DoorStates = new ConcurrentDictionary<uint, bool>();



        public async Task ChangeAndSaveMod(int slot, int mod)
        {
           if (!this.Mods.ContainsKey(slot)) this.Mods.TryAdd(slot, -1);
           this.Mods[slot] = mod;

                var l_NearPlayers = await PlayerController.GetPlayersInRange(await NAPI.Task.RunReturnAsync(() => this.Position), 50.0f);
                foreach (var l_Player in l_NearPlayers)
                {
                    await l_Player.TriggerEventAsync("syncTuning", this, slot, mod);
                }



        }
        public bool Registered { get; set; } = false;
        //public Dictionary<int, uint> Passengers { get; set; } = new Dictionary<int, uint>();

        //[HandleExceptions]
        public async void SetEngineStatus(bool status)
        {
            await NAPI.Task.RunAsync(() =>
            {
                NAPI.Vehicle.SetVehicleEngineStatus(this, status);

                this.EngineStatus = status;

                this.SetSharedData("engineStatus", status);
            });
        }
        public async Task<int> GetNextFreeSeat(int offset = 0)
        {
            var seats = new bool[(int)Math.Round((double)NAPI.Task.RunReturn(() => this.MaxOccupants))];

            var unavailableSeats = new HashSet<int>();

            foreach (var player in await this.GetOccupantsAsync())
            {
                unavailableSeats.Add(await player.GetVehicleSeatAsync());
            }

            for (int i = offset, length = (int)Math.Round((double)NAPI.Task.RunReturn(() => this.MaxOccupants)); i < length; i++)
            {
                if (!unavailableSeats.Contains(i))
                {
                    return i;
                }
            }

            return -2;
        }

        public async Task<bool> IsSeatFree(int seat)
        {
            if (this == null) return false;
            var occupants = await this.GetOccupantsAsync();

            if (occupants == null) return false;

            foreach (var occ in occupants)
            {
                if (await occ.GetVehicleSeatAsync() == seat)
                {
                    return false;
                }
            }

            return this.IsValidSeat(seat);
        }

        public bool IsValidSeat(int seat)
        {
            return seat > -2 && seat < NAPI.Task.RunReturn(() => this.MaxOccupants) - 1;
        }
        public int RXLivery
        {
            get
            {
                return NAPI.Task.RunReturn(() => NAPI.Vehicle.GetVehicleLivery(this));
            }
            set
            {
                NAPI.Task.Run(() => NAPI.Vehicle.SetVehicleLivery(this, value));
            }
        }
        public bool Handbrake
        {
            get
            {
                if (NAPI.Task.RunReturn(() => !this.HasSharedData("handbrakeStatus"))) return false;

                return NAPI.Task.RunReturn(() => this.GetSharedData<bool>("handbrakeStatus"));
            }
            set
            {
                NAPI.Task.Run(() => this.SetSharedData("handbrakeStatus", value));
            }
        }



        //[HandleExceptions]
        public async void SetLocked(bool status)
        {
            await NAPI.Task.RunAsync(() =>
            {
                NAPI.Vehicle.SetVehicleLocked(this, status);

                this.Locked = status;

                this.SetSharedData("lockedStatus", status);
            });
        }
        //[HandleExceptions]
        public async Task<uint> GetDimensionAsync() => await NAPI.Task.RunReturnAsync(() => this.Dimension);

        //[HandleExceptions]
        public async Task SetDimensionAsync(uint dimension) => await NAPI.Task.RunAsync(() => this.Dimension = dimension);

        //[HandleExceptions]
        public async Task<Vector3> GetPositionAsync() => await NAPI.Task.RunReturnAsync(() => this.Position);

        public async Task<Vector3> GetVelocityAsync() => await NAPI.Task.RunReturnAsync(() => NAPI.Entity.GetEntityVelocity(this));
        public async Task<int> GetClassAsync() => await NAPI.Task.RunReturnAsync(() => this.Class);
        public async Task<string> GetDisplayNameAsync() => await NAPI.Task.RunReturnAsync(() => this.DisplayName);
        public async Task<bool> IsSirenActiveAsync() => await NAPI.Task.RunReturnAsync(() => this.Siren);
        public async Task<float> GetHealthAsync() => await NAPI.Task.RunReturnAsync(() => this.Health);
        public async Task<float> GetBodyHealthAsync() => await NAPI.Task.RunReturnAsync(() => NAPI.Vehicle.GetVehicleBodyHealth(this));
        public async Task<uint> GetModelAsync() => await NAPI.Task.RunReturnAsync(() => this.Model);
        public async Task DeleteAsync() => await NAPI.Task.RunAsync(() => this.Delete());
        public async Task<List<RXPlayer>> GetOccupantsAsync() => await NAPI.Task.RunReturnAsync(() => this.Occupants.Cast<RXPlayer>().ToList());

        //[HandleExceptions]
        public async Task SetPositionAsync(Vector3 position) => await NAPI.Task.RunAsync(() => this.Position = position);
        public async Task SetNumberPlateAsync(string plate) => await NAPI.Task.RunAsync(() => this.NumberPlate = plate);
        public List<uint> VehicleKeys { get; set; } = new List<uint>();


        public async Task<int> GetSpeed(RXPlayer player)
        {
            if (!await player.GetIsInVehicleAsync())
            {
                return 0;
            }
            var velocity = await GetVelocityAsync();
            var speed = Math.Sqrt(
                velocity.X * velocity.X +
                velocity.Y * velocity.Y +
                velocity.Z * velocity.Z
            );

            return Convert.ToInt32(speed * 3.6); // from m/s to km/h
        }

        /*public void RemovePlayerFromOccupants(RXPlayer player)
        {
            try
            {
                if (Passengers.ContainsValue(player.Id))
                {
                    Passengers.Remove(Passengers.FirstOrDefault(x => x.Value == player.Id).Key);
                }
            }
            catch (Exception e)
            {
                Logger.Print(e.ToString());
            }
        }

        public void AddPlayerToVehicleOccupants(RXPlayer player, int seat)
        {
            try
            {
                var occupants = this.Passengers;

                if (occupants.ContainsValue(player.Id))
                {
                    occupants.Remove(occupants.FirstOrDefault(x => x.Value == player.Id).Key);
                }
                if (occupants.ContainsKey(seat))
                {
                    occupants.Remove(seat);
                }

                occupants.TryAdd(seat, player.Id);
                this.Passengers = occupants;
            }
            catch (Exception e)
            {
                Logger.Print(e.ToString());
            }
        }
        */
        //[HandleExceptions]
        public bool HasPerm(RXPlayer player)
        {
            return this.OwnerId == player.Id || VehicleKeys.Contains(player.Id) || (this.TeamId == player.TeamId && this.TeamId > 0);
        }
    }
}
