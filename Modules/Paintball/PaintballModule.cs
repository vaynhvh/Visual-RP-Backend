using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Discord;
using Backend.Modules.Gangwar;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Paintball
{


    public class PaintballLobbies
    {
        [JsonProperty(PropertyName = "d")]
        public List<PaintballPoint> data { get; set; }


    }

    public class PaintballLobby
    {
        [JsonProperty(PropertyName = "d")]
        public PaintballPoint data { get; set; }

        [JsonProperty(PropertyName = "opi")]
        public uint PlayerId { get; set; }

        [JsonProperty(PropertyName = "o")]
        public bool CanControl { get; set; }

    }


    public class PaintballPoint
    {
        [JsonProperty(PropertyName = "i")]
        public uint id { get; set; }

        [JsonProperty(PropertyName = "mp")]
        public uint maxplayers { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "s")]
        public bool isRunning { get; set; }

        [JsonProperty(PropertyName = "pw")]
        public bool isPassword { get; set; }

        [JsonIgnore]
        public string Password { get; set; }

        [JsonIgnore]
        public uint OwnerId { get; set; }

        [JsonProperty(PropertyName = "e")]
        public uint Einsatz { get; set; }

        [JsonProperty(PropertyName = "m")]
        public uint MapId { get; set; }

        [JsonProperty(PropertyName = "t")]
        public uint TypeId { get; set; }

        [JsonProperty(PropertyName = "o")]
        public bool canControl { get; set; }

        [JsonProperty(PropertyName = "p")]
        public List<PaintballParticipants> participants { get; set; }

        [JsonProperty(PropertyName = "maxPoints")]
        public uint MaxPoints { get; set; }

    }

    public enum PaintballType
    {
        FFA = 1,
        TeamDeathmatch = 2,
        Gungame = 3,
        Revolver = 4,
        Sniper = 5,
        OneVsOne = 6,
        MKII = 7,
    }


    public class PaintballTeams
    {
        [JsonProperty(PropertyName = "end")]
        public bool isEnding { get; set; }

        [JsonProperty(PropertyName = "t")]
        public uint Team { get; set; }

        [JsonProperty(PropertyName = "t1")]
        public List<PaintballParticipants> TeamOne { get; set; }

        [JsonProperty(PropertyName = "t2")]
        public List<PaintballParticipants> TeamTwo { get; set; }
    }

    public class PaintballParticipants
    {
        [JsonProperty(PropertyName = "i")]
        public uint PlayerId { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string PlayerName { get; set; }

        [JsonProperty(PropertyName = "s")]
        public uint PlayerTeam { get; set; }

        [JsonProperty(PropertyName = "k")]
        public uint PlayerKills { get; set; }

        [JsonProperty(PropertyName = "d")]
        public uint PlayerDeaths { get; set; }
    }

    class PaintballModule : RXModule
    {
        public PaintballModule() : base("Paintball", new RXWindow("PaintballPoint")) { }

        public static List<PaintballPoint> paintballLobbies = new List<PaintballPoint>();   
        public override async void LoadAsync()
        {
            //-252.23201 -2001.5149 30.146017 Heading: 170,75328

            await NAPI.Task.RunAsync(() =>
            {
                //new NPC(PedHash.Armymech01SMY, new GTANetworkAPI.Vector3(-253.8116f, -2027.6268f, 29.94603f), -127.08734f, 0u);
                new NPC(PedHash.Armymech01SMY, new GTANetworkAPI.Vector3(-552.9228f, 284.8437f, 82.17633f), 81.40625f, 0u);
            });

            var mcb = await NAPI.Entity.CreateMCB(new Vector3(-116.22954f, -1772.4661f, 29.823433f), new Color(255, 140, 0), 0u, 2f, 2.4f, false, MarkerType.VerticalCylinder, true, 647, 2, "Paintball");

            mcb.ColShape.Message = new RXMessage
            {
                Text = "Drücke E um die Paintball Lobbies anzuzeigen!",
                Color = "lightblue",
                Duration = 3500,
                Title = "Paintball"
            };

            mcb.ColShape.Action = async player => await OpenPaintball(player);

        }

        public async Task OpenPaintball(RXPlayer player)
        {
            if (player == null) return;
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || player.Freezed) return;


            var lobby = paintballLobbies.FirstOrDefault(x => x.OwnerId == player.Id);

            if (lobby == null)
            {
                await this.Window.OpenWindow(player, new PaintballLobbies() { data = paintballLobbies });
            } else
            {
                RXWindow window = new RXWindow("PaintballLobby");

                await window.OpenWindow(player, new PaintballLobby() { data = lobby, CanControl = true, PlayerId = player.Id });
            }
        }


        [RemoteEvent]
        public async Task CreatePaintballLobby(RXPlayer player, string name, string password, uint maxplayer, uint entry, uint maxscore)
        {
            bool pw = false;

            if (!string.IsNullOrEmpty(name)) pw = true;

            var players = new PaintballParticipants() { PlayerId = player.Id, PlayerName = await player.GetNameAsync(), PlayerTeam = 1, PlayerKills = 0, PlayerDeaths = 0 };

            var playerlist = new List<PaintballParticipants>();

            playerlist.Add(players);

            var plobby = new PaintballPoint() { id = (uint)paintballLobbies.Count + 1, OwnerId = player.Id, Name = name, Einsatz = entry, canControl = false, isPassword = pw, isRunning = false, MapId = 1, TypeId = 1, maxplayers = maxplayer, Password = password, MaxPoints = maxscore, participants = playerlist };

            DiscordModule.Logs.Add(new DiscordLog("FFA", (await player.GetNameAsync()) + " hat eine FFA Lobby erstellt! (Name: " + name + " | Password: " + password + ")", DiscordModule.FFACreate));

            paintballLobbies.Add(plobby);

            await player.SendNotify("Lobby erstellt!");

            RXWindow window = new RXWindow("PaintballLobby");

            await window.OpenWindow(player, new PaintballLobby() { data = plobby, CanControl = true, PlayerId = player.Id });
        
        }


        [RemoteEvent]
        public async Task JoinPaintballLobby(RXPlayer player, uint id, string password)
        {
          
            var plobby = paintballLobbies.Find(x => x.id == id);

            if (plobby == null) return;

            if (password != plobby.Password && plobby.isPassword)
            {
                await player.SendNotify("Das eingegebene Passwort ist falsch!");
                return;
            }

            var pmember = plobby.participants.Find(x => x.PlayerId == player.Id);

            if (pmember != null)
            {
                await player.SendNotify("Du bist bereits in dieser Lobby!");
                return;
            }
            else
            {
                pmember = new PaintballParticipants() { PlayerId = player.Id, PlayerName = await player.GetNameAsync(), PlayerTeam = 1, PlayerKills = 0, PlayerDeaths = 0 };
                plobby.participants.Add(pmember);
                await player.SendNotify("Du bist der Lobby beigetreten!");
                player.inPaintball = true;

                foreach (PaintballParticipants member in plobby.participants)
                {
                    var pl = await PlayerController.FindPlayerById(member.PlayerId);
                    if (pl == null) continue;

                    await pl.TriggerEventAsync("JoinPaintballLobby", player.Id, await player.GetNameAsync());
                }

                RXWindow window = new RXWindow("PaintballLobby");

                await window.OpenWindow(player, new PaintballLobby() { data = plobby, CanControl = false, PlayerId = player.Id });

            }
        }

        [RemoteEvent]
        public async Task showPaintballStats(RXPlayer player)
        {
            if (player == null || !player.inPaintball) return;

            var lobby = getPlayerLobby(player);

            if (lobby == null) return;

            RXWindow window = new RXWindow("PaintballStatistic");

            var teamone = lobby.participants.FindAll(x => x.PlayerTeam == 1);

            if (teamone == null)
            {
                teamone = new List<PaintballParticipants>();
            }

            var teamtwo = lobby.participants.FindAll(x => x.PlayerTeam == 2);

            if (teamtwo == null)
            {
                teamtwo = new List<PaintballParticipants>();
            }

            int teamcount = 1;

            if (lobby.TypeId == 2)
            {
                teamcount = 2;
            }

            await window.OpenWindow(player, new PaintballTeams() { isEnding = false, Team = (uint)teamcount, TeamOne = teamone, TeamTwo = teamtwo });
        }


        [RemoteEvent]
        public async Task ChangePaintballConfig(RXPlayer player, uint id, uint map, uint type)
        {

            var plobby = paintballLobbies.Find(x => x.id == id);

            if (plobby == null) return;


                if (plobby.OwnerId != player.Id)
                {
                    await player.SendNotify("Dafür hast du keine Rechte!");
                    return;
                }

            plobby.MapId = map;
            plobby.TypeId = type;

            await player.SendNotify("Lobby-Config wurde gespeichert.");
            foreach (PaintballParticipants member in plobby.participants)
            {
                var pl = await PlayerController.FindPlayerById(member.PlayerId);
                if (pl == null) continue;

                await pl.TriggerEventAsync("UpdatePaintballConfig", map, type, plobby.maxplayers);
            }
        }

        [RemoteEvent]
        public async Task CancelPaintball(RXPlayer player)
        {

            var plobby = getPlayerLobby(player);

            if (plobby == null) return;


            var pmember = plobby.participants.Find(x => x.PlayerId == player.Id);

            if (pmember == null)
            {
                await player.SendNotify("Du bist nicht in dieser Lobby!");
                return;
            }
            else
            {
                if (plobby.OwnerId != player.Id)
                {
                    plobby.participants.Remove(pmember);
                    await player.SendNotify("Du hast die Lobby verlassen!");


                    foreach (PaintballParticipants member in plobby.participants)
                    {
                        var pl = await PlayerController.FindPlayerById(member.PlayerId);
                        if (pl == null) continue;

                        await pl.TriggerEventAsync("RemovePaintballParticipant", pl.Id);
                    }
                    await PlayerLeaveArena(player);

                }
                else
                {

                    foreach (PaintballParticipants member in plobby.participants)
                    {
                        var pl = await PlayerController.FindPlayerById(member.PlayerId);
                        if (pl == null) continue;

                        await pl.TriggerEventAsync("closeAllWindow");
                        await pl.SendNotify("Die Paintball-Lobby wurde aufgelöst");
                        await PlayerLeaveArena(pl);

                    }

                    plobby.participants.Clear();

                    paintballLobbies.Remove(plobby);
                }
            }
        }

        [RemoteEvent]
        public async Task KickPaintballParticipant(RXPlayer player, uint id, uint pid)
        {

            var plobby = paintballLobbies.Find(x => x.id == id);

            if (plobby == null) return;


            var pmember = plobby.participants.Find(x => x.PlayerId == pid);

            if (pmember == null)
            {
                await player.SendNotify("Spieler ist nicht in dieser Lobby!");
                return;
            }
            else
            {
                if (plobby.OwnerId != player.Id)
                {

                    return;

                }
                else
                {

                    var target = await PlayerController.FindPlayerById(pmember.PlayerId);

                    if (target == null) return;

                    plobby.participants.Remove(pmember);
                    await target.SendNotify("Du wurdest aus der Lobby geworfen!");
                    player.inPaintball = false;


                    foreach (PaintballParticipants member in plobby.participants)
                    {
                        var pl = await PlayerController.FindPlayerById(member.PlayerId);
                        if (pl == null) continue;

                        await pl.TriggerEventAsync("RemovePaintballParticipant", target.Id);
                    }
                }
            }
        }

        [RemoteEvent]
        public async Task LeavePaintballLobby(RXPlayer player, uint id)
        {

            var plobby = paintballLobbies.Find(x => x.id == id);

            if (plobby == null) return;


            var pmember = plobby.participants.Find(x => x.PlayerId == player.Id);

            if (pmember == null)
            {
                await player.SendNotify("Du bist nicht in dieser Lobby!");
                return;
            }
            else
            {
                if (plobby.OwnerId != player.Id)
                {
                    plobby.participants.Remove(pmember);
                    await player.SendNotify("Du hast die Lobby verlassen!");
                    player.inPaintball = false;


                    foreach (PaintballParticipants member in plobby.participants)
                    {
                        var pl = await PlayerController.FindPlayerById(member.PlayerId);
                        if (pl == null) continue;

                        await pl.TriggerEventAsync("RemovePaintballParticipant", player.Id);
                    }

                    } else
                {

                    foreach (PaintballParticipants member in plobby.participants) {
                        var pl = await PlayerController.FindPlayerById(member.PlayerId);
                        if (pl == null) continue;

                        await pl.TriggerEventAsync("closeAllWindow");
                        await pl.SendNotify("Die Paintball-Lobby wurde aufgelöst");
                    
                    }

                    plobby.participants.Clear();

                    paintballLobbies.Remove(plobby);
                }
            }
        }


        [RemoteEvent]
        public async Task StartPaintballLobby(RXPlayer player, uint id)
        {

            var plobby = paintballLobbies.Find(x => x.id == id);

            if (plobby == null) return;


            var pmember = plobby.participants.Find(x => x.PlayerId == player.Id);

            if (pmember == null)
            {
                await player.SendNotify("Du bist nicht in dieser Lobby!");
                return;
            }
            else
            {
                if (plobby.OwnerId != player.Id)
                {
                    await player.SendNotify("Dazu hast du keine Rechte!");
                    return;
                }
                var type = (PaintballType)plobby.TypeId;


                foreach (PaintballParticipants member in plobby.participants)
                    {
                        var pl = await PlayerController.FindPlayerById(member.PlayerId);
                        if (pl == null) continue;

                        await pl.TriggerEventAsync("closeAllWindow");
                        await pl.SendNotify("Die Paintball-Runde startet nun");
                        await SpawnInArena(pl, true);
                    pl.Paintballkills = 0;
                    pl.Paintballdeaths = 0;



                }

                    plobby.isRunning = true;    

                
            }
        }


        [RemoteEvent]
        public async Task JoinPaintballLobbyAfterwards(RXPlayer player, uint id, uint playerid)
        {

            var plobby = paintballLobbies.Find(x => x.id == id);

            if (plobby == null) return;


            var pmember = plobby.participants.Find(x => x.PlayerId == player.Id);

            if (pmember == null)
            {
                await player.SendNotify("Du bist nicht in dieser Lobby!");
                return;
            }
            else
            {
      
      
                    await player.TriggerEventAsync("closeAllWindow");
                    await player.SendNotify("Du bist der Runde beigetreten!");
                    await SpawnInArena(player, true);
                player.Paintballkills = 0;
                player.Paintballdeaths = 0;



              

            }
        }


        [RemoteEvent]
        public async Task SelectPaintballTeam(RXPlayer player, uint pid, uint team)
        {
            var plobby = getPlayerLobby(player);

            if (plobby == null) return;


            var pmember = plobby.participants.Find(x => x.PlayerId == player.Id);


            if (pmember == null)
            {
                await player.SendNotify("Spieler ist nicht in dieser Lobby!");
                return;
            }
            else
            {

                pmember.PlayerTeam = team;

                await player.SendNotify("Team wurde erfolgreich gewechselt!");


                foreach (PaintballParticipants member in plobby.participants)
                {
                    var pl = await PlayerController.FindPlayerById(member.PlayerId);
                    if (pl == null) continue;

                    await pl.TriggerEventAsync("SelectPaintballTeam", player.Id, team);
                }
            }
        }

        public static PaintballPoint getPlayerLobby(RXPlayer player)
        {
            foreach (var lobby in paintballLobbies)
            {

                var found = lobby.participants.Find(x => x.PlayerId == player.Id);

                if (found == null) continue;

                return lobby;

            }

            return null;
        }


        public async static Task SpawnInArena(RXPlayer player, bool first = false)
        {

            using var db = new RXContext();

            var lobby = getPlayerLobby(player);



                            if (lobby == null) return;


            PaintballType type = (PaintballType)lobby.TypeId;

            if (type == PaintballType.FFA || type == PaintballType.TeamDeathmatch || type == PaintballType.Sniper || type == PaintballType.MKII || type == PaintballType.Gungame || type == PaintballType.Revolver)
            {

                var spawnpoints = await db.PaintballSpawnpoints.Where(x => x.MapId == lobby.MapId).ToListAsync();

                if (spawnpoints == null) return;
                Random rnd = new Random();
                var spawnPoint = spawnpoints[rnd.Next(spawnpoints.Count)];
                await player.SetDimensionAsync(spawnPoint.MapId + 6000);
                await player.SetPositionAsync(spawnPoint.Position.ToPos());
            }

            player.inPaintball = true;

            if (type == PaintballType.TeamDeathmatch)
            {
                player.PaintballTeam = (int)lobby.participants.Find(x => x.PlayerId == player.Id).PlayerTeam;
            } else
            {
                player.PaintballTeam = 0;
            }

            if (first)
            {
                player.FFAKillStreak = 0;
            }
            await GivePlayerArenaWeapons(player);



        }


        public override async Task OnPlayerDisconnect(RXPlayer player, DisconnectionType type, string reason)
        {

            var lobby = getPlayerLobby(player);



            if (lobby == null || !lobby.isRunning) return;

            await CancelPaintball(player);
         

        }

            public async static Task PlayerLeaveArena(RXPlayer player)
        {
            await player.RemoveAllWeaponsAsync();

            await player.LoadCharacter();
            player.inPaintball = false;
            player.FFAKillStreak = 0;
            player.Paintballkills = 0;
            player.Paintballdeaths = 0;
            player.PaintballTeam = 0;

            await player.SetPositionAsync(new Vector3(-116.22954, -1772.4661, 29.823433));
            await player.SetDimensionAsync(0);

            if (player.DeathData.IsDead)
            {
                await player.RevivePlayer();
            }

        }



        public static async Task GivePlayerArenaWeapons(RXPlayer player)
        {
            if (player == null) return;
            var lobby = getPlayerLobby(player);

            if (lobby == null) return;

            await player.RemoveAllWeaponsAsync();

            await player.SetHealthAsync(100);
            await player.SetArmorAsync(100);
            PaintballType type = (PaintballType)lobby.TypeId;


            if (type == PaintballType.FFA || type == PaintballType.TeamDeathmatch)
            {
                await player.GiveWeaponAsync(WeaponHash.Knife, 600);
                await player.GiveWeaponAsync(WeaponHash.Heavypistol, 600);
                await player.GiveWeaponAsync(WeaponHash.Assaultrifle, 0);
                await player.GiveWeaponAsync(WeaponHash.Advancedrifle, 0);
                await player.GiveWeaponAsync(WeaponHash.Specialcarbine, 0);
                await player.GiveWeaponAsync(WeaponHash.Bullpuprifle, 600);
                await player.GiveWeaponAsync(WeaponHash.Gusenberg, 600);
            }
            else if (type == PaintballType.Sniper)
            {
                await player.GiveWeaponAsync(WeaponHash.Marksmanrifle, 600);
                await player.GiveWeaponAsync(WeaponHash.Sniperrifle, 600);
                await player.GiveWeaponAsync(WeaponHash.Marksmanpistol, 600);
                await player.GiveWeaponAsync(WeaponHash.Knife, 600);
            }
            else if (type == PaintballType.Revolver)
            {
                await player.GiveWeaponAsync(WeaponHash.Revolver, 600);
                await player.GiveWeaponAsync(WeaponHash.Revolver_mk2, 600);
                await player.GiveWeaponAsync(WeaponHash.Knife, 600);
            }
            else if (type == PaintballType.MKII)
            {
                await player.GiveWeaponAsync(WeaponHash.Knife, 600);
                await player.GiveWeaponAsync(WeaponHash.Pistol_mk2, 600);
                await player.GiveWeaponAsync(WeaponHash.Assaultrifle_mk2, 0);
                await player.GiveWeaponAsync(WeaponHash.Specialcarbine_mk2, 0);
            }
            else if (type == PaintballType.Gungame)
            {
                await player.GiveWeaponAsync(WeaponHash.Hammer, 600);
                if (player.FFAKillStreak == 0)
                {
                    await player.GiveWeaponAsync(WeaponHash.NavyRevolver, 600);
                }
                else if (player.FFAKillStreak == 1)
                {
                    await player.GiveWeaponAsync(WeaponHash.Smg, 600);
                }
                else if (player.FFAKillStreak == 2)
                {
                    await player.GiveWeaponAsync(WeaponHash.Advancedrifle, 600);
                }
                else if (player.FFAKillStreak == 3)
                {
                    await player.GiveWeaponAsync(WeaponHash.Assaultrifle, 600);
                }
                else if (player.FFAKillStreak == 4)
                {
                    await player.GiveWeaponAsync(WeaponHash.Carbinerifle, 600);
                }
                else if (player.FFAKillStreak == 5)
                {
                    await player.GiveWeaponAsync(WeaponHash.Carbinerifle_mk2, 600);
                }
                else if (player.FFAKillStreak == 6)
                {
                    await player.GiveWeaponAsync(WeaponHash.Pumpshotgun, 600);
                }
                else if (player.FFAKillStreak == 7)
                {
                    await player.GiveWeaponAsync(WeaponHash.Musket, 600);
                }
                else if (player.FFAKillStreak == 8)
                {
                    await player.GiveWeaponAsync(WeaponHash.Combatmg, 600);
                }
                else if (player.FFAKillStreak == 9)
                {
                    await player.GiveWeaponAsync(WeaponHash.Doubleaction, 600);
                }
                else if (player.FFAKillStreak == 10)
                {
                    await player.GiveWeaponAsync(WeaponHash.Minigun, 600);
                } else
                {
                    await player.GiveWeaponAsync(WeaponHash.Specialcarbine_mk2, 600);
                }


            }
        }
    }
    }
