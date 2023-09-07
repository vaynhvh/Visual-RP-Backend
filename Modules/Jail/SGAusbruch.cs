using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Doors;
using Backend.Modules.Faction;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Jail
{

    public class SGVoltage
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public bool Breaked { get; set; }

    }
    class SGAusbruch : RXModule
    {

        public SGAusbruch() : base("SGAusbruch") { }

        public static int ManipulateToCrashElectircal = 6;

        public static Vector3 Hauptverteiler = new Vector3(1605.83, 2621.01, 45.5649);

        public static List<Vector3> Generatoren = new List<Vector3>();

        public bool IsOnMainGen = false;

        public static List<SGVoltage> ManipuliereStromkaesten = new List<SGVoltage>();
        public static List<SGVoltage> Stromkaesten = new List<SGVoltage>();

        public DateTime lastBreaked = DateTime.Now.AddMinutes(-30);

        public override async void LoadAsync()
        {
            var mcbe = await NAPI.Entity.CreateMCB(Hauptverteiler, new Color(255, 140, 0), 0u, 1.4f, 1.4f, false, MarkerType.VerticalCylinder, false); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);


            mcbe.ColShape.Action = async player => await BreakMainGenerator(player);

            Generatoren.Add(new Vector3(1652.571, 2564.1611, 45.56487));
            Generatoren.Add(new Vector3(1629.9292, 2564.312, 45.56487));
            Generatoren.Add(new Vector3(1624.0934, 2577.388, 45.56487));
            Generatoren.Add(new Vector3(1609.0557, 2567.0269, 45.56487));
            Generatoren.Add(new Vector3(1609.8286, 2539.6746, 45.56487));
            Generatoren.Add(new Vector3(1622.3922, 2507.76, 45.56489));
            Generatoren.Add(new Vector3(1643.8221, 2490.7837, 45.56489));
            Generatoren.Add(new Vector3(1679.6198, 2480.3552, 45.56492));
            Generatoren.Add(new Vector3(1700.1199, 2474.8274, 45.56493));
            Generatoren.Add(new Vector3(1706.9099, 2481.0305, 45.56492));
            Generatoren.Add(new Vector3(1737.3776, 2504.6443, 45.564922));
            Generatoren.Add(new Vector3(1760.6213, 2519.0967, 45.564922));
            Generatoren.Add(new Vector3(1761.5166, 2540.4658, 45.564922));

            uint id = 0;

            foreach (var coord in Generatoren)
            {
                id++;
                SGVoltage sg = new SGVoltage { Id = id, Breaked = false, Position = coord };

                Stromkaesten.Add(sg);

                var mcb = await NAPI.Entity.CreateMCB(sg.Position, new Color(255, 140, 0), 0u, 1.4f, 1.4f, false, MarkerType.VerticalCylinder, false); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

                mcb.ColShape.Action = async player => await BreakGenerator(player, Stromkaesten.Find(x => x.Id == sg.Id).Id);
            }
        }

        public async Task BreakMainGenerator(RXPlayer player)
        {

            if (!player.CanInteract() || player.Freezed) return;
            if (!IsAbleToManipulate()) return;

            if (IsOnMainGen) return;


            if (ManipuliereStromkaesten.Count < ManipulateToCrashElectircal)
            {
                return;
            }


            IsOnMainGen = true;
            player.Freezed = true;
            await player.disableAllPlayerActions(true);

            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@world_human_hammering@male@base", "base");

            await player.SendProgressbar(10000);
            await Task.Delay(10000);
            IsOnMainGen = false;

            player.Freezed = false;
            await player.disableAllPlayerActions(false);

            await player.StopAnimationAsync();
            lastBreaked = DateTime.Now;

            var doormodels = new List<long>();

            doormodels.Add(-1156020871);
            doormodels.Add(320433149);
            doormodels.Add(631614199);
            doormodels.Add(741314661);

            foreach (DbDoor door in DoorsModule.Doors)
            {
                if (doormodels.Contains(door.Model))
                {
                    door.Locked = false;
                }
            }

            foreach (SGVoltage sGVoltage in Stromkaesten)
            {
                sGVoltage.Breaked = false;
            }

            ManipuliereStromkaesten.Clear();
            TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState($"Es wurde ein Stromausfall am Staatsgefängnis gemeldet!");

            for (int i = 0; i < 4; i++)
            {
                foreach (RXPlayer dbPlayer1 in await PlayerController.GetPlayersInRange(StaatsSG.sgBellPosition, 300.0f))
                {
                    await dbPlayer1.SendNotify($"1337Allahuakbar$sgalarm", 31000);
                }
                await Task.Delay(30000);
            }
     
        }

        public async Task BreakGenerator(RXPlayer player, uint sgid)
        {
            var sg = Stromkaesten.Find(x => x.Id == sgid);

            if (sg == null) return;
            if (player.Team.IsState())
            {
                await player.ShowReactionGame("BreakGeneratorFinish", sgid, ReactionGameLevel.Easy);
                return;
            }
            else
            {

                if (lastBreaked.AddMinutes(30) > DateTime.Now)
                {
                    await player.SendNotify("Das Stromnetz wurde vor kurzem erst manipuliert!");
                    return;
                }

                if (sg.Breaked)
                {
                    await player.SendNotify("Hmm, die Kabel wurden bereits vertauscht...");
                    return;
                }
                await player.ShowReactionGame("BreakGeneratorFinish", sgid, ReactionGameLevel.Normal);


            }
        }




        [RemoteEvent]
        public async Task BreakGeneratorFinish(RXPlayer player, bool success, uint sgid)
        {
            if (!player.CanInteract() || player.Freezed) return;
            if (!success) return;
            var sg = Stromkaesten.Find(x => x.Id == sgid);

            if (sg == null) return;
            if (player.Team.IsState())
            {
                player.Freezed = true;
                await player.disableAllPlayerActions(true);

                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                await player.SendProgressbar(1000);
                await Task.Delay(1000);

                player.Freezed = false;
                await player.disableAllPlayerActions(false);
                await player.StopAnimationAsync();


                if (sg.Breaked) sg.Breaked = false;
                await player.SendNotify("Alle Kabel sitzen nun wieder!");
                return;
            }
            else
            {

                if (lastBreaked.AddMinutes(30) > DateTime.Now)
                {
                    await player.SendNotify("Das Stromnetz wurde vor kurzem erst manipuliert!");
                    return;
                }

                if (sg.Breaked)
                {
                    await player.SendNotify("Hmm, die Kabel wurden bereits vertauscht...");
                    return;
                }


                sg.Breaked = true;

                ManipuliereStromkaesten.Add(sg);
                await player.SendNotify("Du hast die Kabel vertauscht! Lass dich nicht erwischen!");
                await player.SendNotify($"Manipuliere mindestens {ManipulateToCrashElectircal} Generatoren! ({ManipuliereStromkaesten.Count}/6)", 8000);

            }
        }

        public bool IsAbleToManipulate()
        {

            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (Configuration.DevMode) return true;


            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 30)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 15)
                    {
                        return false;
                    }

                    break;
            }


            return true;
        }
    }
}
