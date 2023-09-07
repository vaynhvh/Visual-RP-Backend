using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Crime;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Jail
{
    public class JailSpawn
    {
        public uint Id { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public int Group { get; set; }

    }

    class JailModule : RXModule
    {
        public JailModule() : base("Jail") { }

        public static Vector3 PrisonZone = new Vector3(1681, 2604, 44);
        public static float Range = 200.0f;

        public static Vector3 PrisonSpawn = new Vector3(1836.71, 2587.8, 45.891);

        public static List<JailSpawn> jailSpawns= new List<JailSpawn>();

        public override async void LoadAsync()
        {

            jailSpawns.Add(new JailSpawn { Id = 1, Position = new Vector3(1690.5906, 2578.7983, 45.911495), Group = 5, Heading = -179.19212f });   

            foreach (var jailspawn in jailSpawns)
            {

                var mcb = await NAPI.Entity.CreateMCB(jailspawn.Position, new Color(255, 140, 0), 0u, 2f, 2.4f, false, MarkerType.VerticalCylinder, false);

                mcb.ColShape.SetData<int>("jailGroup", jailspawn.Group);

            }

        }

        public override async Task OnColShape(RXColShape shape, RXPlayer player, bool enter)
        {
            if (await player.GetIsInVehicleAsync()) return;

            if (shape == null || !shape.HasData("jailGroup")) return;

            if (enter)
            {
                if (player.Team.IsState() && player.InDuty) return;

                var wanteds = CrimeModule.CalcJailTime(player);
                if (wanteds < 30) wanteds = 30;

                if (player.Jailtime > 0)
                {
                    // already inhaftiert
                    return;
                }

                int jailtime = CrimeModule.CalcJailTime(player);
                int jailcosts = CrimeModule.CalcJailCosts(player);

                // Checke auf Jailtime
                if (jailtime > 0 && jailtime <= 29 && shape.GetData<int>("jailGroup") != 5)
                {
                    player.Jailtime = (uint)jailtime;
                    await CrimeModule.ArrestPlayer(player, null, false);
                    //     dbPlayer.ApplyCharacter();
                    player.SetData("inJailGroup", shape.GetData<int>("jailGroup"));
                } // group 5 == sg
                else if (shape.GetData<int>("jailGroup") == 5 && jailtime >= 30)
                {
                    player.Jailtime = (uint)jailtime;
                    //     dbPlayer.ArrestPlayer(null, false);
                    await CrimeModule.ArrestPlayer(player, null, false);
              //      dbPlayer.ApplyCharacter();
                    player.SetData("inJailGroup", shape.GetData<int>("jailGroup"));
                }

            }
            else
            {
                player.ResetData("inJailGroup");
            }
        }

    }
}
