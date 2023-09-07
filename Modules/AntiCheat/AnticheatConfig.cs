using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Modules.AntiCheat
{
    public class AnticheatConfig
    {
        public static List<WeaponHash> blacklistedWeapons = new List<WeaponHash>
        {
            WeaponHash.Minigun,
            WeaponHash.Raypistol,
            WeaponHash.Rayminigun,
            WeaponHash.Raycarbine,
            WeaponHash.Stickybomb,
            WeaponHash.Molotov,
            WeaponHash.Firework,
            WeaponHash.Bzgas,
            WeaponHash.Grenadelauncher,
            WeaponHash.Grenadelauncher_smoke,
            WeaponHash.Grenade,
            WeaponHash.Smokegrenade,
            WeaponHash.HazardCan,
            WeaponHash.Hominglauncher,
            WeaponHash.Rpg
        };

        public static List<int> blacklistedExplosions = new List<int>
        {
            0,
            1,
            2,
            3,
            4,
            6,
            7,
            8,
            11,
            12,
            14,
            15,
            16,
            17,
            18,
            19,
            20,
            21,
            22,
            23,
            25,
            26,
            27,
            28,
            29,
            30,
            32,
            33,
            34,
            35,
            36,
            37,
            38,
            70
        };
    }
}
