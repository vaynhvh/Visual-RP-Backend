using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Faction;
using Backend.Modules.Laptop;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Update;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Staatsfraktionen.STATE
{
    public class TrainingsDutyObject
    {

        public string Name { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public uint TeamId { get; set; }

    }

    class TrainingsDutyModule : RXModule
    {
        public TrainingsDutyModule() : base("TrainingsDutyModule") { }


        public static List<TrainingsDutyObject> TrainingsDuties = new List<TrainingsDutyObject>();

        public override async void LoadAsync()
        {
            RequireModule("Team");

            TrainingsDuties.Add(new TrainingsDutyObject() { Name = "LSPD Trainingsdienst", TeamId = 1, Position = new Vector3(487.37552f, -1015.32306f, 30.679264f), Heading = -0.74443597f });
            TrainingsDuties.Add(new TrainingsDutyObject() { Name = "FIB Trainingsdienst", TeamId = 5, Position = new Vector3(88.97191f, -721.47595f, 33.133278f), Heading = -110.454384f });

            foreach (var training in TrainingsDuties)
            {
                new NPC(PedHash.Cop01SMY, training.Position, training.Heading, 0u);

                var mcb = await NAPI.Entity.CreateMCB(training.Position, new Color(255, 140, 0), 0u, 2.4f);

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um den Trainingsdienst zu betreten!",
                    Color = "green",
                    Duration = 3500,
                    Title = training.Name,
                    RestrictedToTeam = training.TeamId
                };

                mcb.ColShape.Action = async player => await ToggleTrainingDuty(player, training.TeamId);

                var mcbe = await NAPI.Entity.CreateMCB(training.Position.Add(new Vector3(0, 0, 1)), new Color(0, 238, 255, 180), 187000, 1.4f, 1.4f, true, (MarkerType)5);

                mcbe.ColShape.Message = new RXMessage
                {
                    Text = "Benutze E um den Trainingsdienst zu verlassen!",
                    Color = "red",
                    Duration = 3500,
                    Title = training.Name,
                    RestrictedToTeam = training.TeamId
                };

                mcbe.ColShape.Action = async player => await ToggleTrainingDuty(player, training.TeamId);
            }
        }


        public async Task ToggleTrainingDuty(RXPlayer player, uint frak)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || player.TeamId == 0 || player.Team == null || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if (player.TeamId != frak) return;

            if (player.TrainingsDuty)
            {
                player.TrainingsDuty = false;
                await player.SetDimensionAsync(0);
                await player.SendNotify("Du hast den Trainingsdienst verlassen!");
            } else
            {
                player.TrainingsDuty = true;
                await player.SetDimensionAsync(187000);
                await player.SendNotify("Du hast den Trainingsdienst betreten!");
            }


        }



    }
}
