using Backend.Models;
using Backend.Modules.Gangwar;
using Backend.Utils.Extensions;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Weapons
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class WeaponModule : RXModule
    {
        public WeaponModule() : base("Weapon") { }

        //[HandleExceptions]
        [ServerEvent(Event.PlayerWeaponSwitch)]
        public async Task OnPlayerWeaponSwitch(RXPlayer player, WeaponHash oldWeapon, WeaponHash newWeapon)
        {
            if (!player.IsLoggedIn) return;

            await NAPI.Task.RunAsync(async () =>
            {
                if (newWeapon != WeaponHash.Unarmed)
                {
                    NAPI.Player.SetPlayerCurrentWeapon(player, newWeapon);
                    //int ammo = 1000;
                    //NAPI.Player.SetPlayerCurrentWeaponAmmo(player, ammo);
                    //await player.EvalAsync($"mp.game.invoke('0xDCD2A934D65CB497', mp.game.player.getPed(), {NAPI.Util.GetHashKey(newWeapon.ToString())}, {ammo});");
                }
            });
        }
    }
}
