using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Jail;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Doors
{
    class DoorsModule : RXModule
    {
        public DoorsModule() : base("Doors") { }

        public static List<DbDoor> Doors = new List<DbDoor>();   

        public override async void LoadAsync()
        {

            using var db = new RXContext();

            Doors = await db.Doors.ToListAsync();

            foreach(var door in Doors)
            {

                await SetLocked(door, door.Locked);

                var mcb = await NAPI.Entity.CreateMCB(door.Position.ToPos(), new Color(255, 140, 0), 0u, door.Range, 2.4f, false, MarkerType.VerticalCylinder, false);

                mcb.ColShape.ColShapeKeyType = ColShape.ColShapeKeyType.L;

                mcb.ColShape.SetData("isdoor", true);
                mcb.ColShape.SetData("doorget", door);

                mcb.ColShape.Action = async player => await LockDoor(player, door);
            }



        }

        public override async Task OnColShape(RXColShape shape, RXPlayer player, bool enter)
        {

            if (shape == null || !shape.HasData("isdoor")) return;

            if (enter)
            {

                DbDoor door = shape.GetData<DbDoor>("doorget");

                if (door != null)
                {
                    await RefreshForPlayer(player, door);
                }

            }
        }

        [RXCommand("createpdoor", 96)]
        public async Task createpdoor(RXPlayer player, string[] args)
        {

            using var db = new RXContext();

            if (!player.HasData("doorhash"))
            {
                await player.SendNotify("Keine Türdaten zwischengespeichert!");
                return;
            }

            bool locked = bool.Parse(args[1]);
            long hash = player.GetData<long>("doorhash");
            Vector3 coord = player.GetData<Vector3>("doorcoord");

            var door = new DbDoor { Model = hash, Locked = locked, OpenWithHacking = false, OpenWithWelding = false, Position = coord.FromPos(), Range = 1.4f, RangRestriction = 0, Teams = "", PlayerIds = args[0] };

            await db.Doors.AddAsync(door);

            await db.SaveChangesAsync();

            Doors.Add(door);


            var mcb = await NAPI.Entity.CreateMCB(door.Position.ToPos(), new Color(255, 140, 0), 0u, door.Range, 2.4f, false, MarkerType.VerticalCylinder, false);

            mcb.ColShape.ColShapeKeyType = ColShape.ColShapeKeyType.L;

            mcb.ColShape.SetData("isdoor", true);
            mcb.ColShape.SetData("doorget", door);

            mcb.ColShape.Action = async player => await LockDoor(player, door);

            await Refresh(door);
            await player.SendNotify("Tür gespeichert!");
            player.ResetData("doorhash");
            player.ResetData("doorcoord");

        }

        [RXCommand("createdoor", 96)]
        public async Task createdoor(RXPlayer player, string[] args)
        {

            using var db = new RXContext();

            if (!player.HasData("doorhash"))
            {
                await player.SendNotify("Keine Türdaten zwischengespeichert!");
                return;
            }

            bool locked = bool.Parse(args[1]);
            long hash = player.GetData<long>("doorhash");
            Vector3 coord = player.GetData<Vector3>("doorcoord");

            var door = new DbDoor { Model = hash, PlayerIds = "", Locked = locked, OpenWithHacking = false, OpenWithWelding = false, Position = coord.FromPos(), Range = 1.4f, RangRestriction = 0, Teams = args[0] };

                  await db.Doors.AddAsync(door);

               await db.SaveChangesAsync();

              Doors.Add(door);


            var mcb = await NAPI.Entity.CreateMCB(door.Position.ToPos(), new Color(255, 140, 0), 0u, door.Range, 2.4f, false, MarkerType.VerticalCylinder, false);

            mcb.ColShape.ColShapeKeyType = ColShape.ColShapeKeyType.L;

            mcb.ColShape.SetData("isdoor", true);
            mcb.ColShape.SetData("doorget", door);

            mcb.ColShape.Action = async player => await LockDoor(player, door);

            await Refresh(door);
            await player.SendNotify("Tür gespeichert!");
            player.ResetData("doorhash");
            player.ResetData("doorcoord");

        }

        [RemoteEvent]
        public async Task TryToCreateDoor(RXPlayer player, long hash, Vector3 pos)
        {
            if (!player.InGduty) return;
            //  using var db = new RXContext();


            player.SetData("doorhash", hash);
            player.SetData("doorcoord", pos);


           //      var door = new DbDoor { Model = hash, Locked = true, OpenWithHacking = false, OpenWithWelding = false, Position = pos.FromPos(), Range = 2.5f, RangRestriction = 0, Teams = "" };

         //       await db.Doors.AddAsync(door);

         //     await db.SaveChangesAsync();

         //     Doors.Add(door);


        //    await Refresh(door);
            await player.SendNotify("Tür-Daten zwischengespeichert!");




        }

        public async Task LockDoor(RXPlayer player, DbDoor door)
        {

            HashSet<RXTeam> teams = door.Teams.ToTeam();
            HashSet<uint> playerids = door.PlayerIds.ToUINT();

            if (teams.Contains(player.Team))
            {

                if (door.Locked)
                {
                    door.Locked = false;

                    await player.SendNotify("Tür aufgeschlossen!", 4000, "green");
                    await Refresh(door);
                } else
                {
                    door.Locked = true;

                    await player.SendNotify("Tür abgeschlossen!", 4000, "red");
                    await Refresh(door);
                }
            } else if (playerids.Contains(player.Id))
            {
                if (door.Locked)
                {
                    door.Locked = false;

                    await player.SendNotify("Tür aufgeschlossen!", 4000, "green");
                    await Refresh(door);
                }
                else
                {
                    door.Locked = true;

                    await player.SendNotify("Tür abgeschlossen!", 4000, "red");
                    await Refresh(door);
                }
            }
          
        }

        public static async Task SetLocked(DbDoor door, bool locked = true)
        {
            door.Locked = locked;
      
                var pair = Doors.Find(x => x.Id == door.Id);
                if (pair != null)
                {
                    pair.Locked = locked;
                    await Refresh(door);
                }
                return;
        }

        

        public static async Task RefreshForPlayer(RXPlayer player, DbDoor door)
        {


    
                    if (door.Model >= 0)
                        await player.TriggerEventAsync("setStateOfClosestDoorOfType", (uint)door.Model, door.Position.ToPos().X, door.Position.ToPos().Y, door.Position.ToPos().Z, door.Locked, 0, false);
                    else
                        await player.TriggerEventAsync("setStateOfClosestDoorOfType", (int)door.Model, door.Position.ToPos().X, door.Position.ToPos().Y, door.Position.ToPos().Z, door.Locked, 0, false);
                
            
        }

        public static async Task Refresh(DbDoor door)
        {

            
            foreach (RXPlayer player in PlayerController.GetValidPlayers())
            {
                Vector3 ppos = await player.GetPositionAsync();
                if (ppos.DistanceTo(door.Position.ToPos()) < door.Range * 5)
                {
                    if (door.Model >= 0)
                        await player.TriggerEventAsync("setStateOfClosestDoorOfType", (uint)door.Model, door.Position.ToPos().X, door.Position.ToPos().Y, door.Position.ToPos().Z, door.Locked, 0, false);
                    else
                        await player.TriggerEventAsync("setStateOfClosestDoorOfType", (int)door.Model, door.Position.ToPos().X, door.Position.ToPos().Y, door.Position.ToPos().Z, door.Locked, 0, false);
                }
            }
        }
    }
}
