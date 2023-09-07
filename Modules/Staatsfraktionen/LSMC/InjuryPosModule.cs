using Backend.Models;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Staatsfraktionen.LSMC
{
    class InjuryPosModule : RXModule
    {
        public InjuryPosModule() : base("InjuryPos") { }

        public static List<DbInjury> InjuriesPos = new List<DbInjury>();

        public override async void LoadAsync()
        {
            using var db = new RXContext();

            InjuriesPos = await db.Injuries.ToListAsync();

            foreach (var injury in InjuriesPos)
            {
                var mcb = await NAPI.Entity.CreateMCB(injury.Position.ToPos(), new Color(255, 140, 0), 0u, 5f, 5f, false, MarkerType.UpsideDownCone, true, 61, 1);

                mcb.ColShape.SetData("injurypoint", injury);

                var mcb2 = await NAPI.Entity.CreateMCB(injury.KHPosition.ToPos(), new Color(255, 140, 0), injury.KHDimension, 5f, 5f, false, MarkerType.UpsideDownCone);

                mcb2.ColShape.SetData("injuryrevivepoint", injury);
            }
        }

        public static async Task<bool> TrySetPlayerIntoVehicleOccupants(RXVehicle sxVehicle, RXPlayer player)
        {
            if (sxVehicle == null || player == null) return false;
            if (sxVehicle.ModelData.Seats > 1)
            {
                int key = 1;

                var Occupants = await sxVehicle.GetOccupantsAsync();
                
                while (key < sxVehicle.ModelData.Seats - 1)
                {
                    var seatplayer = Occupants.Find(x => NAPI.Task.RunReturn(() => x.VehicleSeat) == key);
                    if (seatplayer == null)
                    {
                        await NAPI.Task.RunAsync(() => player.SetIntoVehicle(sxVehicle, key));
                        return true;
                    }
                    key++;
                }

                return false;
            }
            return false;
        }

        [RXCommand("savefirstinjurypos")]
        public async Task savefirstinjurypos(RXPlayer player, string[] args)
        {
            if (player == null) return;


            player.SetData("firstinjurypos", await player.GetPositionAsync());

            await player.SendNotify("First Injurypoint saved!");
        }

        [RXCommand("savesecondinjurypos")]
        public async Task savesecondinjurypos(RXPlayer player, string[] args)
        {
            if (player == null) return;


            player.SetData("secondinjurypos", await player.GetPositionAsync());
            player.SetData("secondinjurydim", await player.GetDimensionAsync());

            await player.SendNotify("Second Jumppoint saved!");
        }

        [RXCommand("createinjurypos")]
        public async Task createinjurypos(RXPlayer player, string[] args)
        {
            if (player == null) return;

            using var db = new RXContext();

            var injurypos = new DbInjury { Name = args[0], IsBadFrak = bool.Parse(args[1]), Position = player.GetData<Vector3>("firstinjurypos").FromPos(), KHDimension = player.GetData<uint>("secondinjurydim"), KHPosition = player.GetData<Vector3>("secondinjurypos").FromPos(), };

            db.Injuries.Add(injurypos);

            await db.SaveChangesAsync();
            await player.SendNotify("Injurypos created!");

        }


        public override async Task OnColShape(RXColShape shape, RXPlayer player, bool state)
        {

            if (shape.HasData("injuryrevivepoint"))
            {
                var injurypos = shape.GetData<DbInjury>("injurypoint");
                if (state)
                {
                    player.SetData("InInjuryRevivePoint", injurypos);
                } else
                {
                    player.ResetData("InInjuryRevivePoint");
                }
            }

                if (shape.HasData("injurypoint"))
            {
                var injurypos = shape.GetData<DbInjury>("injurypoint");

                if (state)
                {
                    if (player.Injured || player.DeathData.IsDead)
                    {

                        await player.SetPositionAsync(injurypos.KHPosition.ToPos());
                        await player.SetDimensionAsync(injurypos.KHDimension);
                    }
                }
            }
        }
    }
}
