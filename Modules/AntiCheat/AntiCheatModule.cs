using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Discord;
using Backend.Modules.Gangwar;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Crypto.Tls;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.AntiCheat
{
    class AntiCheatModule : RXModule
    {
        public AntiCheatModule() : base("Anticheat") { }

        [RemoteEvent]
        public async void antinametags(RXPlayer player)
        {
            if (!player.IsLoggedIn) return;

            if(player.HasData("acnametags")) { return; }

            if (Configuration.ACDevMode == true)
            {
                await player.SendNotify("Anti-Cheat [Nametags]");
                return;
            }
            await player.BanPlayer("Anti-Cheat [Nametags]");
        }

        [RemoteEvent]
        public async void ExplosionCheck(RXPlayer player, int explosionid)
        {
            if (player == null) { return; }

            if (AnticheatConfig.blacklistedExplosions.Contains(explosionid))
            {
                if (player.HasData("acexplosion")) { return; }
                if (player.Flags != 3) { player.Flags += 1; return; }
                if (Configuration.ACDevMode == true)
                {
                    await player.SendNotify("Anti-Cheat [BLACKLISTED EXPLOSION]");
                    return;
                }
                await player.BanPlayer("Anti-Cheat [BLACKLISTED EXPLOSION]");
                return;
            }
        }

        [ServerEvent(Event.PlayerWeaponSwitch)]
        public async Task OnPlayerWeaponSwitch(RXPlayer player, WeaponHash oldWeapon, WeaponHash newWeapon)
        {
            if (!player.IsLoggedIn) return;

            if (newWeapon != WeaponHash.Unarmed)
            {
                if (AnticheatConfig.blacklistedWeapons.Contains(newWeapon))
                {
                    if (player.HasData("acblacklistweapon")) { return; }
                    if (Configuration.ACDevMode == true)
                    {
                        await player.SendNotify("Anti-Cheat [BLACKLISTED WEAPON]");
                        return;
                    }
                    await player.RemoveWeaponAsync(newWeapon);
                    await player.BanPlayer("Anti-Cheat [BLACKLISTED WEAPON]");
                    return;
                }
                if (!player.inPaintball && !GangwarModule.gangwarPlayers.ContainsKey(player) && player.Weapons.Find(x => x.WeaponHash == newWeapon.ToString()) == null)
                {
                    if (player.HasData("acgiveweapon")) { return; }
                    if (Configuration.ACDevMode == true)
                    {
                        await player.SendNotify("Anti-Cheat [GIVE WEAPON]");
                        return;
                    }
                    await player.RemoveWeaponAsync(newWeapon);
                    await player.BanPlayer("Anti-Cheat [GIVE WEAPON]");
                    return;
                }
            }
        }
    }
}
