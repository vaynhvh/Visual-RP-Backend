using Backend.Models;
using Backend.Models.Appearance;
using Backend.Modules.Native;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Schoenheitsklinik
{
     class Schoenheitsklinik : RXModule
    {
        public Schoenheitsklinik() : base("Schoenheitsklinik") { }

        public override async void LoadAsync()
        {

            var mcb = await NAPI.Entity.CreateMCB(new Vector3(350.89212f, -588.022f, 28.796839f), new Color(255, 140, 0), 0u, 2f, 2.4f, false, MarkerType.VerticalCylinder, false);

            mcb.ColShape.Message = new RXMessage
            {
                Text = "Drücke E um die Schönheitsklinik zu öffnen!",
                Color = "lightblue",
                Duration = 3500,
            };

            mcb.ColShape.Action = async player => await OpenKlinikMenu(player);

        }

        public async Task OpenKlinikMenu(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

          
                RXWindow CharacterCreator = new RXWindow("Char");

                await player.SetDimensionAsync((uint)new Random().Next(2500, 1000000));
                await player.SetPositionAsync(new Vector3(-1832.6901f, -1240.9187f, 13.00293f));

                await player.EvalAsync("mp.players.local.setHeading(-185);");


                using var db = new RXContext();

            DbCharacter dbCharacter = await db.Characters.FirstOrDefaultAsync(c => c.Id == player.Id);
                if (dbCharacter != null)
                {

                await CharacterCreator.OpenWindow(player, dbCharacter.Customization, true);

                }


       
        }

    }
}
