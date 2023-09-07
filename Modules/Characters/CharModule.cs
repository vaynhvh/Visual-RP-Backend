using Backend.Models;
using Backend.Modules.Player;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Characters
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class CharModule : RXModule
    {
        public CharModule() : base("Char", new RXWindow("Char")) { }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task EndChar(RXPlayer player)
        {
            if (!player.IsLoggedIn) return;

            await player.ShowLoader("Abbrechen...", 500);
            await Task.Delay(500);

            using var db = new RXContext();

            DbPlayer dbPlayer = await db.Players.FirstOrDefaultAsync(c => c.Id == player.Id);
            if (dbPlayer == null) return;

            DbCharacter dbCharacter = await db.Characters.FirstOrDefaultAsync(c => c.Id == player.Id);
            if (dbCharacter == null)
            {
                dbCharacter = new DbCharacter
                {
                    Id = player.Id,
                    Clothes = "{}",
                    Accessories = "{}"
                };

                await db.Characters.AddAsync(dbCharacter);
                await db.SaveChangesAsync();
            }

            await player.SendNotify("Deine Charakteränderung wurde abgebrochen.", 3500, "red", "Charaktererstellung");

            await player.SetDimensionAsync(0);
            await player.SetPositionAsync(dbPlayer.Position.ToPos());

            await player.LoadCharacter();

            await player.TriggerEventAsync("skyMover");
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task FinishChar(RXPlayer player, string json)
        {

            await player.ShowLoader("Charakter wird gespeichert...", 500);
            await Task.Delay(500);

            using var db = new RXContext();

            DbPlayer dbPlayer = await db.Players.FirstOrDefaultAsync(c => c.Id == player.Id);
            if (dbPlayer == null) return;

            DbCharacter dbCharacter = await db.Characters.FirstOrDefaultAsync(c => c.Id == player.Id);
            if (dbCharacter == null)
            {
                dbCharacter = new DbCharacter
                {
                    Id = player.Id,
                    Customization = json,
                    Clothes = "{}",
                    Accessories = "{}"
                };

                await db.Characters.AddAsync(dbCharacter);
                await player.SendNotify("Dein Charakter wurde erfolgreich erstellt.", 3500, "green", "Charaktererstellung");

                if (Configuration.PaintballEvent)
                {
                    await player.SpawnAsync(LoginModule.PaintballSpawn.ToPos() + new Vector3(0, 0, 0.52f));
                }
                else
                {

                    dbPlayer.Position = LoginModule.random_spawns[new Random().Next(LoginModule.random_spawns.Count)];
                    await player.SetPositionAsync(dbPlayer.Position.ToPos());
                }
                 
            }
            else
            {

           
                dbCharacter.Customization = json;

                await player.SendNotify("Dein Charakter wurde erfolgreich gespeichert.", 3500, "green", "Charaktererstellung");
                if (Configuration.PaintballEvent)
                {
                    await player.SpawnAsync(LoginModule.PaintballSpawn.ToPos() + new Vector3(0, 0, 0.52f));
                }
                else
                {
                    await player.SetPositionAsync(dbPlayer.Position.ToPos());
                }

            }

            await db.SaveChangesAsync();

            await player.SetDimensionAsync(0);



            await player.LoadCharacter(dbCharacter);

            await player.TriggerEventAsync("skyMover");
        }
    }
}
