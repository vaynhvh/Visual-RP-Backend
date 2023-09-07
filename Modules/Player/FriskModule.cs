using Backend.Controllers;
using Backend.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Player
{
    class FriskModule : RXModule
    {
        public FriskModule() : base("Frisk") { }

        [RemoteEvent]
        public async Task closedWeaponFrisk(RXPlayer player, string friskedPersonName, bool wantsToDrop)
        {
            if (!player.IsLoggedIn || await player.GetNameAsync() == friskedPersonName || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            var target = await PlayerController.FindPlayerByName(friskedPersonName);
            if (target == null) return;

            await player.TriggerEventAsync("closeFriskWindow");

            //COP check

            player.resetFriskInventoryFlags();
            player.resetDisabledInventoryFlag();

            target.Container.ShowFriskInventory(player, target, "Spieler", target.Cash + target.Blackmoney);
        }

        [RemoteEvent]
        public async Task resetDisabledInventoryFlag(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            player.resetFriskInventoryFlags();
            player.resetDisabledInventoryFlag();
        }
    }
}
