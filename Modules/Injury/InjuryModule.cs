using Backend.Controllers;
using Backend.Models;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Injury
{
    class InjuryModule : RXModule
    {
        public InjuryModule() : base("Injury") { }

        public static List<RXPlayer> injuredPlayers = new List<RXPlayer>();
        public static List<RXInjury> Injuries = new List<RXInjury>();

        public override void LoadAsync()
        {
            injuredPlayers = new List<RXPlayer>();

            Injuries = new List<RXInjury>
            {
                new RXInjury
                {
                    Id = 1,
                    Name = "Leichte Verletzung!",
                    Duration = 5000,
                    DamageScale = 15,
                    AnimDict = "-",
                    AnimName = "-",
                    ScreenEffect = "DeathFailMPDark"
                },
                new RXInjury
                {
                    Id = 2,
                    Name = "Verletzung!",
                    Duration = 7500,
                    DamageScale = 30,
                    AnimDict = "random@dealgonewrong",
                    AnimName = "idle_b",
                    ScreenEffect = "DeathFailMPDark"
                },
                new RXInjury
                {
                    Id = 3,
                    Name = "Schwere Verletzung!",
                    Duration = 10000,
                    DamageScale = 50,
                    AnimDict = "random@dealgonewrong",
                    AnimName = "idle_a",
                    ScreenEffect = "DeathFailMPDark"
                },
            };
        }

        public static async Task CheckStressStatus(RXPlayer player)
        {
            if (player.Stress > 80)
            {
                await player.TriggerEventAsync("startScreenEffect", "ChopVision", 5000, true);
            }
            else
            {
                await player.TriggerEventAsync("stopScreenEffect", "ChopVision");
            }
        }

        public override async Task OnPlayerDamage(RXPlayer player, float healthLoss)
        {
            int injuryid = 0;
            if (healthLoss > 20)
            {
                injuryid = 3;
                player.Stress += 30;

            }
            else if (healthLoss < 10)
            {
                injuryid = 1;
                player.Stress += 10;


            }
            else if (healthLoss > 10)
            {
                injuryid = 2;
                player.Stress += 5;
            }

            var injury = Injuries.Find(x => x.Id == injuryid);

            await player.TriggerEventAsync("startScreenEffect", injury.ScreenEffect, 5000, true);

            if (injury.AnimDict != "-" && injury.AnimDict != "-")
            {
                if (await player.GetIsInVehicleAsync() == false)
                {
                    await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), injury.AnimDict, injury.AnimName);
                }
                player.Freezed = true;
                await Task.Delay(injury.Duration);
                player.Freezed = false;
                await player.TriggerEventAsync("stopScreenEffect", "DeathFailMPDark");
                await player.StopAnimationAsync();

            }
        }

    }
}
