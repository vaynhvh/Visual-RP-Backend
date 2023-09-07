
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

namespace Backend.Modules.Jumppoints
{
    class JumppointModule : RXModule
    {
        public JumppointModule() : base("Jumppoint") { }

        public static List<DbJumppoint> Jumppoints = new List<DbJumppoint>();

        public override async void LoadAsync()
        {
            await RefreshJumppoints();
        }

        public async Task RefreshJumppoints()
        {
            using var db = new RXContext();

            Jumppoints = await db.Jumppoints.ToListAsync();

            foreach (var jumppoint in Jumppoints)
            {

                var mcb = await NAPI.Entity.CreateMCB(jumppoint.Position.ToPos(), new Color(255, 140, 0), jumppoint.Dimension, jumppoint.Range, jumppoint.Range, false, MarkerType.CheckeredFlagRect);

                mcb.ColShape.Message = new RXMessage
                {
                    Text = $"Benutze E um von {jumppoint.Name} nach {jumppoint.SecondName} zu gehen.",
                    Duration = 3500,
                };

                mcb.ColShape.Action = async player => await UseJumppoint(player, jumppoint);

                var mcb2 = await NAPI.Entity.CreateMCB(jumppoint.SecondPosition.ToPos(), new Color(255, 140, 0), jumppoint.SecondDimension, jumppoint.SecondRange, jumppoint.SecondRange, false, MarkerType.CheckeredFlagRect);

                mcb2.ColShape.Message = new RXMessage
                {
                    Text = $"Benutze E um von {jumppoint.SecondName} nach {jumppoint.Name} zu gehen.",
                    Duration = 3500,
                };

                mcb2.ColShape.Action = async player => await UseSecondJumppoint(player, jumppoint);

                var jumpenterlock = await NAPI.Entity.CreateMCB(jumppoint.Position.ToPos(), new Color(255, 140, 0), jumppoint.Dimension, jumppoint.Range, jumppoint.Range, false, MarkerType.CheckeredFlagRect);

                jumpenterlock.ColShape.ColShapeKeyType = ColShape.ColShapeKeyType.L;
                jumpenterlock.ColShape.Action = async player => await ToggleJumpLock(player, jumppoint);

                var jumpexitlock = await NAPI.Entity.CreateMCB(jumppoint.SecondPosition.ToPos(), new Color(255, 140, 0), jumppoint.SecondDimension, jumppoint.SecondRange, jumppoint.SecondRange, false, MarkerType.CheckeredFlagRect);

                jumpexitlock.ColShape.ColShapeKeyType = ColShape.ColShapeKeyType.L;
                jumpexitlock.ColShape.Action = async player => await ToggleJumpLock(player, jumppoint);
            }
        }

        [RXCommand("savefirstjumppoint")]
        public async Task savefirstjumppoint(RXPlayer player, string[] args)
        {
            if (player == null) return;


            player.SetData("firstjumppos", await player.GetPositionAsync());
            player.SetData("firstjumpdim", await player.GetDimensionAsync());

            await player.SendNotify("First Jumppoint saved!");
        }

        [RXCommand("savesecondjumppoint")]
        public async Task savesecondjumppoint(RXPlayer player, string[] args)
        {
            if (player == null) return;


            player.SetData("secondjumppos", await player.GetPositionAsync());
            player.SetData("secondjumpdim", await player.GetDimensionAsync());

            await player.SendNotify("Second Jumppoint saved!");
        }

        [RXCommand("createjumppoint")]
        public async Task createjumppoint(RXPlayer player, string[] args)
        {
            if (player == null) return;

            using var db = new RXContext();

            var jumppoint = new DbJumppoint { Position = player.GetData<Vector3>("firstjumppos").FromPos(), Dimension = player.GetData<uint>("firstjumpdim"), Locked = true, Name = args[0], SecondName = args[1], Teams = args[2], FloorObject = false, WithVehicle = bool.Parse(args[3]), SecondPosition = player.GetData<Vector3>("secondjumppos").FromPos(), SecondDimension = player.GetData<uint>("secondjumpdim"), Range = float.Parse(args[4]), SecondRange = float.Parse(args[4]) };

            db.Jumppoints.Add(jumppoint);

            await db.SaveChangesAsync();
            await player.SendNotify("Jumppoint created!");
            await RefreshJumppoints();

        }

        public async Task ToggleJumpLock(RXPlayer player, DbJumppoint jumppoint)
        {
            if (player == null) return;

            if (jumppoint.Locked)
            {
                await player.SendNotify("Du hast die Tür aufgeschlossen!", 3500, "green");
                jumppoint.Locked = false;
            }
            else
            {
                await player.SendNotify("Du hast die Tür abgeschlossen!", 3500, "red");
                jumppoint.Locked = true;
            }
        }

        public async Task UseJumppoint(RXPlayer player, DbJumppoint jumppoint)
        {

            if (player == null) return;

            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !jumppoint.WithVehicle && await player.GetIsInVehicleAsync()) return;

            if (jumppoint.Locked)
            {
                await player.SendNotify("Die Tür ist abgeschlossen!", 3500, "red");
                return;
            }
            RXVehicle sxVeh = await player.GetVehicleAsync();
            if (jumppoint.WithVehicle && sxVeh != null)
            {


                await sxVeh.SetDimensionAsync(jumppoint.SecondDimension);

                foreach (RXPlayer occupant in await NAPI.Task.RunReturnAsync(() => sxVeh.Occupants))
                {
                    if (occupant == null) continue;
                    await occupant.SetDimensionAsync(jumppoint.SecondDimension);
                    await occupant.ShowLoader("Position wird geladen!", 1000);
                }
                player.Freezed = true;
                await sxVeh.SetPositionAsync(jumppoint.SecondPosition.ToPos());
                await Task.Delay(1000);
                player.Freezed = false;
                await player.StopAnimationAsync();
            } else
            {
                await player.ShowLoader("Position wird geladen!", 1000);
                await player.SetDimensionAsync(jumppoint.SecondDimension);
                player.Freezed = true;
                await player.SetPositionAsync(jumppoint.SecondPosition.ToPos());
                await Task.Delay(1000);
                player.Freezed = false;
                await player.StopAnimationAsync();
            }
        }

        public async Task UseSecondJumppoint(RXPlayer player, DbJumppoint jumppoint)
        {

            if (player == null) return;

            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || !jumppoint.WithVehicle && await player.GetIsInVehicleAsync()) return;

            if (jumppoint.Locked)
            {
                await player.SendNotify("Die Tür ist abgeschlossen!", 3500, "red");
                return;
            }

            RXVehicle sxVeh = await player.GetVehicleAsync();
            if (jumppoint.WithVehicle && sxVeh != null)
            {
                await sxVeh.SetDimensionAsync(jumppoint.Dimension);

                foreach (RXPlayer occupant in await NAPI.Task.RunReturnAsync(() => sxVeh.Occupants))
                {
                    if (occupant == null) continue;
                    await occupant.SetDimensionAsync(jumppoint.Dimension);
                    await occupant.ShowLoader("Position wird geladen!", 1000);
                }
                player.Freezed = true;
                await sxVeh.SetPositionAsync(jumppoint.Position.ToPos());
                await Task.Delay(1000);
                player.Freezed = false;
                await player.StopAnimationAsync();
            }
            else
            {
                await player.ShowLoader("Position wird geladen!", 1000);
                await player.SetDimensionAsync(jumppoint.Dimension);
                player.Freezed = true;
                await player.SetPositionAsync(jumppoint.Position.ToPos());
                await Task.Delay(1000);
                player.Freezed = false;
                await player.StopAnimationAsync();
            }
        }

    }
}
