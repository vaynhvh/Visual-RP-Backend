using Backend.Controllers;
using Backend.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Training
{
    class TrainingModule : RXModule
    {
        public TrainingModule() : base("Training") { }

        public static List<RXTraining> trainingspots = new List<RXTraining>();
        public static Dictionary<RXPlayer, RXTrainingType> trainingplayers = new Dictionary<RXPlayer, RXTrainingType>();
        public static Dictionary<RXPlayer, DateTime> trainingcooldown = new Dictionary<RXPlayer, DateTime>();
        public static Random randy = new Random();

        public async override void LoadAsync()
        {

            trainingspots = new List<RXTraining>
            {
                new RXTraining
                {

                    Id = 1,
                    Name = "Vespucci Training",
                    Range = 2,
                    Positions = new Dictionary<Vector4, RXTrainingType>(){
                        { new Vector4(-1197.8866f, -1568.1659f, 5.0169697f, 127.21f), RXTrainingType.Situps },
                        { new Vector4(-1199.0787f, -1565.065f, 5.020294f, 127.21f), RXTrainingType.Liegestütze },
                        { new Vector4(-1200.5065f, -1561.9626f, 5.0096755f, 127.21f), RXTrainingType.Situps },
                        { new Vector4( -1207.2408f, -1560.6904f, 5.0177827f, -147.28f), RXTrainingType.Situps },
                        { new Vector4(-1203.255f, -1567.938f, 5.009251f, -147.28f), RXTrainingType.Liegestütze },
                        { new Vector4(-1201.2404f, -1566.525f, 5.015813f, -147.28f), RXTrainingType.Liegestütze },
                        { new Vector4(-1202.4188f, -1572.992f, 4.607892f, 127.21f), RXTrainingType.Arme },
                        { new Vector4(-1209.492f, -1559.0248f, 4.6078935f, -126.06748f), RXTrainingType.Arme },
                    },
                },
                                new RXTraining
                {

                    Id = 2,
                    Name = "Würfelpark Training",
                    Range = 2,
                    Positions = new Dictionary<Vector4, RXTrainingType>(){
                        { new Vector4( 194.6544f, -997.13324f, 30.493526f, -17.898783f), RXTrainingType.Situps },
                        { new Vector4( 192.06714f, -996.09753f, 30.492619f, 160.31973f), RXTrainingType.Situps },
                        { new Vector4( 202.38661f, -989.76953f, 30.49898f, -40.69104f), RXTrainingType.Situps },
                        { new Vector4( 197.73793f, -997.89435f, 30.501686f, 74.74848f), RXTrainingType.Situps },
                        { new Vector4(199.63895f, -998.8268f, 30.501877f, -111.78364f), RXTrainingType.Situps },
                        { new Vector4( 202.43614f, -999.9402f, 30.499294f, 26.860674f), RXTrainingType.Situps },
                        { new Vector4( 199.7426f, -994.7295f, 30.11191f, 75.38448f), RXTrainingType.Liegestütze },
                        { new Vector4( 191.2447f, -993.0774f, 30.09189f, 35.758717f), RXTrainingType.Arme },
                        { new Vector4(203.61899f, -991.0852f, 30.09189f, -135.43533f), RXTrainingType.Arme },
                    },
                }
            };
            trainingplayers = new Dictionary<RXPlayer, RXTrainingType>();
            trainingcooldown = new Dictionary<RXPlayer, DateTime>();

            await trainingspots.forEach(async trainingspot =>
            {
                foreach (var trainingpos in trainingspot.Positions.ToList())
                {

                    var mcb = await NAPI.Entity.CreateMCB(new GTANetworkAPI.Vector3(trainingpos.Key.X, trainingpos.Key.Y, trainingpos.Key.Z), new Color(255, 140, 0), 0u, 2f, 2.4f, false, MarkerType.VerticalCylinder);

                    mcb.ColShape.Message = new RXMessage
                    {
                        Text = "Drücke E um " + trainingpos.Value.ToString() + " zu trainieren!",
                        Color = "green",
                        Duration = 3500,
                        Title = trainingspot.Name
                    };

                    mcb.ColShape.Action = async player => await StartTraining(player, trainingspot.Id);
                }

            });


        }

        public override async Task OnTenSecond()
        {

            List<RXPlayer> removegleich = new List<RXPlayer>();
            foreach(var cooldownplayer in trainingcooldown)
            {

                if (cooldownplayer.Value.AddMinutes(2) < DateTime.Now)
                {
                    removegleich.Add(cooldownplayer.Key);
                }
            }

            foreach (RXPlayer player in removegleich)
            {
                await player.SendNotify("Du fühlst dich wieder fit genug um zu trainieren!");
                trainingcooldown.Remove(player);
            }

            removegleich.Clear();
        }


        public KeyValuePair<Vector4, RXTrainingType> GetTrainingSpotAndTypeByPos(RXTraining training, GTANetworkAPI.Vector3 playerpos)
        {
            var l_Positions = training;

            foreach (var trainingpos in l_Positions.Positions.ToList())
            {
                var l_Range = l_Positions.Range;


                GTANetworkAPI.Vector3 l_Vector = new GTANetworkAPI.Vector3(trainingpos.Key.X, trainingpos.Key.Y, trainingpos.Key.Z);

                    if (playerpos.DistanceTo(l_Vector) > l_Range)
                        continue;
                    return trainingpos;
         
            }
            return new KeyValuePair<Vector4, RXTrainingType>();
        }

        [RemoteEvent]
        public async Task DoFitness(RXPlayer player, int id)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if (id == 1)
            {

                await player.SendNotify("Du beginnst zu joggen...");
                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "move_m@jogger", "run_turn_r2");
                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(60000);
                
                player.IsTaskAllowed = false;
                player.DraggingItem = true;

                await Task.Delay(60000);

                if (!player.DraggingItem) return;

                player.DraggingItem = false;

                await player.disableAllPlayerActions(false);
                player.IsTaskAllowed = true;
                await player.StopAnimationAsync();
                await player.SendNotify("Du bist erfolgreich ein wenig gejoggt und konntest so Stress abbauen & Muskeln aufbauen!");
                player.Stress -= randy.Next(1, 8);
                player.Sport += randy.Next(1, 8);

            }
            else
            if (id == 2)
            {

                await player.SendNotify("Du beginnst Liegestütze zu machen...");
                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_push_ups@male@idle_a", "idle_d");
                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(30000);

                player.IsTaskAllowed = false;
                player.DraggingItem = true;

                await Task.Delay(30000);


                if (!player.DraggingItem) return;

                player.DraggingItem = false;

                await player.disableAllPlayerActions(false);
                player.IsTaskAllowed = true;
                await player.StopAnimationAsync();
                await player.SendNotify("Du hast erfolgreich ein paar Liegestütze gemacht und konntest so Stress abbauen & Muskeln aufbauen!");
                player.Stress -= randy.Next(1, 4);
                player.Sport += randy.Next(1, 4);

            }
            else
            if (id == 3)
            {

                await player.SendNotify("Du beginnst Situps zu machen...");
                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_sit_ups@male@idle_a", "idle_b");
                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(45000);

                player.IsTaskAllowed = false;
                player.DraggingItem = true;

                await Task.Delay(45000);



                if (!player.DraggingItem) return;

                player.DraggingItem = false;

                await player.disableAllPlayerActions(false);
                player.IsTaskAllowed = true;
                await player.StopAnimationAsync();
                await player.SendNotify("Du hast erfolgreich ein paar Situps gemacht und konntest so Stress abbauen & Muskeln aufbauen!");
                player.Stress -= randy.Next(1, 6);
                player.Sport += randy.Next(1, 6);

            }
            else
            if (id == 4)
            {

                await player.SendNotify("Du beginnst Yoga zu machen...");
                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_yoga@male@base", "base_b");
                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(15000);

                player.IsTaskAllowed = false;
                player.DraggingItem = true;

                await Task.Delay(15000);

                if (!player.DraggingItem) return;

                player.DraggingItem = false;

                await player.disableAllPlayerActions(false);
                player.IsTaskAllowed = true;
                await player.StopAnimationAsync();
                await player.SendNotify("Du hast ein bisschen Yoga gemacht und konntest so Stress abbauen & Muskeln aufbauen!");
                player.Stress -= randy.Next(1, 2);
                player.Sport += randy.Next(1, 2);

            }

            }



            public async Task StartTraining(RXPlayer player, int trainingid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            try
            {

                var training = trainingspots.Find(x => x.Id == trainingid);

                if (training == null) return;

                if (trainingcooldown.ContainsKey(player))
                {
                    await player.SendNotify("Du musst dich noch ein wenig entspannen! Nicht so hastig Sohnemann!", 3500);
                    return;
                }

                if (trainingplayers.ContainsKey(player))
                {
                    await player.SendNotify("Du trainierst bereits!", 3500, "red");
                    return;
                }


                var trainingspot = GetTrainingSpotAndTypeByPos(training, await player.GetPositionAsync());

                player.Freezed = true;

                await player.SetPositionAsync(new GTANetworkAPI.Vector3(trainingspot.Key.X, trainingspot.Key.Y, trainingspot.Key.Z + 0.1f));
                await player.SetHeadingAsync(trainingspot.Key.W);
                await player.SendNotify("Du beginnst " + trainingspot.Value.ToString() + " zu trainieren!");

                trainingplayers.Add(player, trainingspot.Value);

                if (trainingspot.Value == RXTrainingType.Situps)
                {
                    await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_sit_ups@male@idle_a", "idle_b");
                }
                else if (trainingspot.Value == RXTrainingType.Liegestütze)
                {
                    await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_push_ups@male@idle_a", "idle_d");
                }
                else if (trainingspot.Value == RXTrainingType.Arme)
                {
                    await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_muscle_free_weights@male@barbell@base", "base");
                }

                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(30000);

                player.IsTaskAllowed = false;

                await Task.Delay(30000);
                await player.disableAllPlayerActions(false);
                player.IsTaskAllowed = true;
                player.Freezed = false;
                await player.StopAnimationAsync();
                await player.SendNotify("Du hast erfolgreich " + trainingspot.Value.ToString() + " trainiert und konntest so Stress abbauen!");
                player.Stress -= randy.Next(1, 4);
                player.Sport += randy.Next(1, 4);

                trainingplayers.Remove(player);
                trainingcooldown.Add(player, DateTime.Now);



            } catch (Exception e)
            {
                RXLogger.Print("TRAINING: " + e.Message);
            }

        }
    }
}
