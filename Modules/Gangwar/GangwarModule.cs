using Backend.Controllers;
using Backend.Models;
using Backend.Models.Appearance;
using Backend.Modules.Faction;
using Backend.Modules.Garage;
using Backend.Modules.Inventory;
using Backend.Modules.Native;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Gangwar
{

    public class GangwarStats
    {
        [JsonProperty(PropertyName = "d")]
        public List<RXTeam> Teams { get; set; }

        [JsonProperty(PropertyName = "member")]
        public List<GangwarPlayer> Member { get; set; }

        [JsonProperty(PropertyName = "ap")]
        public uint AttackerPoints { get; set; }

        [JsonProperty(PropertyName = "dp")]
        public uint DefenderPoints { get; set; }

        [JsonProperty(PropertyName = "ai")]
        public uint AttackerId { get; set; }

        [JsonProperty(PropertyName = "di")]
        public uint DefenderId { get; set; }

    }

    public class GangwarPlayer
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "k")]
        public uint Kills { get; set; }

        [JsonProperty(PropertyName = "d")]
        public uint Deaths { get; set; }
    }

    public class GangwarStart
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "image")]
        public string Image { get; set; }

        [JsonProperty(PropertyName = "cba")]
        public bool canBeAttacked { get; set; }

        [JsonProperty(PropertyName = "la")]
        public string LastAttack { get; set; }

        [JsonProperty(PropertyName = "cbaa")]
        public string CanBeAttackedDatum { get; set; }
    }
    public class GangwarWeaponPack
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "t")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "w1")]
        public uint WeaponId1 { get; set; }

        [JsonProperty(PropertyName = "w2")]
        public uint WeaponId2 { get; set; }

        [JsonProperty(PropertyName = "w3")]
        public uint WeaponId3 { get; set; }

        [JsonProperty(PropertyName = "w1_name")]
        public string WeaponName1 { get; set; }

        [JsonProperty(PropertyName = "w2_name")]
        public string WeaponName2 { get; set; }

        [JsonProperty(PropertyName = "w3_name")]
        public string WeaponName3 { get; set; }
    }


    class GangwarModule : RXModule
    {
        public GangwarModule() : base("Gangwar", new RXWindow("GangwarStart")) { }

        public static List<DbGangwar> Gangwars = new List<DbGangwar>();
        public static Dictionary<RXPlayer, DbGangwar> gangwarPlayers = new Dictionary<RXPlayer, DbGangwar>();

        public override async void LoadAsync()
        {
            RequireModule("Team");

            await Task.Delay(8000);

            using var db = new RXContext();

            Gangwars = await db.Gangwar.ToListAsync();

            await Gangwars.forEachAlternativeAsync(async dbGangwar =>
            {
                var team = TeamModule.Teams.FirstOrDefault(x => x.Id == dbGangwar.TeamId);
                if (team == null) return;
                var mcb = await NAPI.Entity.CreateMCB(dbGangwar.Position.ToPos(), new Color(0, 238, 255, 180), 0u, 2.4f, 2.4f, false, MarkerType.VerticalCylinder, true, 543, (byte)team.BlipColor, dbGangwar.Name);

                dbGangwar.GangwarBlip = mcb.Blip;

                mcb.ColShape.Message = new RXMessage
                {
                    Text = "Drücke E um das Gebiet " + dbGangwar.Name + " anzugreifen!",
                    Color = team.RGB.ConvertHTML(),
                    Duration = 3500,
                    Title = dbGangwar.Name
                };

                mcb.ColShape.Action = async player => await StartGangwarZone(player, dbGangwar.Id);

                var mcb2 = await NAPI.Entity.CreateMCB(dbGangwar.Flag1.ToPos().Add(new Vector3(0, 0, 1)), new Color(0, 238, 255, 180), dbGangwar.Dimension, 1.4f, 1.4f, true, MarkerType.CheckeredFlagRect, false, 543, (byte)team.BlipColor, dbGangwar.Name);

                mcb2.ColShape.Message = new RXMessage
                {
                    Text = "Drücke E um die Flagge einzunehmen!",
                    Color = team.RGB.ConvertHTML(),
                    Duration = 3500,
                    Title = dbGangwar.Name
                };


                mcb2.ColShape.Action = async player => await EnterFlag(player, 1, dbGangwar.Id);


                var mcb3 = await NAPI.Entity.CreateMCB(dbGangwar.Flag2.ToPos().Add(new Vector3(0, 0, 1)), new Color(0, 238, 255, 180), dbGangwar.Dimension, 1.4f, 1.4f, true, MarkerType.CheckeredFlagRect, false, 543, (byte)team.BlipColor, dbGangwar.Name);

                mcb3.ColShape.Message = new RXMessage
                {
                    Text = "Drücke E um die Flagge einzunehmen!",
                    Color = team.RGB.ConvertHTML(),
                    Duration = 3500,
                    Title = dbGangwar.Name
                };
                mcb3.ColShape.Action = async player => await EnterFlag(player, 2, dbGangwar.Id);

                var mcb4 = await NAPI.Entity.CreateMCB(dbGangwar.Flag2.ToPos().Add(new Vector3(0, 0, 1)), new Color(0, 238, 255, 180), dbGangwar.Dimension, 1.4f, 1.4f, true, MarkerType.CheckeredFlagRect, false, 543, (byte)team.BlipColor, dbGangwar.Name);

                mcb4.ColShape.Message = new RXMessage
                {
                    Text = "Drücke E um die Flagge einzunehmen!",
                    Color = team.RGB.ConvertHTML(),
                    Duration = 3500,
                    Title = dbGangwar.Name
                };
                mcb4.ColShape.Action = async player => await EnterFlag(player, 3, dbGangwar.Id);

                var attacker = await NAPI.Entity.CreateMCB(dbGangwar.AttackerAusparker.ToPos().Add(new Vector3(0, 0, 1)), new Color(0, 238, 255, 180), dbGangwar.Dimension, 1.4f, 1.4f, true, (MarkerType)36, false, 543, (byte)team.BlipColor, dbGangwar.Name);

                attacker.ColShape.Message = new RXMessage
                {
                    Text = "Drücke E um auf den Ausparker zuzugreifen!",
                    Color = team.RGB.ConvertHTML(),
                    Duration = 3500,
                    Title = dbGangwar.Name
                };
                attacker.ColShape.Action = async player => await OpenConfirmParkOutVeh(player, "attacker", dbGangwar.Id);


                var defender = await NAPI.Entity.CreateMCB(dbGangwar.DefenderAusparker.ToPos().Add(new Vector3(0, 0, 1)), new Color(0, 238, 255, 180), dbGangwar.Dimension, 1.4f, 1.4f, true, (MarkerType)36, false, 543, (byte)team.BlipColor, dbGangwar.Name);

                defender.ColShape.Message = new RXMessage
                {
                    Text = "Drücke E um auf den Ausparker zuzugreifen!",
                    Color = team.RGB.ConvertHTML(),
                    Duration = 3500,
                    Title = dbGangwar.Name
                };
                defender.ColShape.Action = async player => await OpenConfirmParkOutVeh(player, "defender", dbGangwar.Id);

            });
        }

        public async Task OpenConfirmParkOutVeh(RXPlayer player, string state, uint gwid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            var gangwar = Gangwars.Find(x => x.Id == gwid);
            if (gangwar == null) return;

            object confirmationBoxObject = new
            {
                t = "Willst du ein Fahrzeug ausparken?",
                ft = "Ja",
                st = "Nein",
                fe = "ParkOutVehGangwar",
                se = "Close",
                d = 0,
            };

            await new RXWindow("Confirm").OpenWindow(player, confirmationBoxObject);
        }

        [RemoteEvent]
        public async Task ParkOutVehGangwar(RXPlayer player)
        {
            foreach (var gangwar in Gangwars)
            {
                if (gangwar.AttackerId == player.Team.Id)
                {
                    await NAPI.Task.RunAsync(() =>
                    {
                        RXVehicle vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey("jackal"), gangwar.AttackerVehSpawn.ToPos(), gangwar.AttackerVehSpawnRotation, player.Team.ColorId, player.Team.ColorId, "", 255, false, true, gangwar.Dimension);
                        vehicle.NumberPlate = player.Team.ShortName;
                        vehicle.SetSharedData("engineStatus", true);
                        vehicle.SetSharedData("lockedStatus", false);
                    });
                }
                else
                {
                    await NAPI.Task.RunAsync(() =>
                    {
                        RXVehicle vehicle = (RXVehicle)NAPI.Vehicle.CreateVehicle(NAPI.Util.GetHashKey("jackal"), gangwar.DefenderVehSpawn.ToPos(), gangwar.DefenderVehSpawnRotation, player.Team.ColorId, player.Team.ColorId, "", 255, false, true, gangwar.Dimension);
                        vehicle.NumberPlate = player.Team.ShortName;
                        vehicle.SetSharedData("engineStatus", true);
                        vehicle.SetSharedData("lockedStatus", false);
                    });
                }
            }
        }

        public async Task EnterFlag(RXPlayer player, uint flagid, uint gwid)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            var gangwar = Gangwars.Find(x => x.Id == gwid);

            if (gangwar == null) return;

            if (flagid == 1)
            {

                if (gangwar.Flag1Team == (int)player.Team.Id)
                {
                    return;
                }


                if (gangwar.Flag1Team == gangwar.AttackerId)
                {
                    gangwar.AttackerFlags--;
                    gangwar.DefenderFlags++;
                }
                else
                {
                    gangwar.DefenderFlags--;
                    gangwar.AttackerFlags++;
                }

                gangwar.Flag1Team = (int)player.Team.Id;

            }
            else if (flagid == 2)
            {
                if (gangwar.Flag2Team == (int)player.Team.Id)
                {
                    return;
                }



                if (gangwar.Flag2Team == gangwar.AttackerId)
                {
                    gangwar.AttackerFlags--;
                    gangwar.DefenderFlags++;
                }
                else
                {
                    gangwar.DefenderFlags--;
                    gangwar.AttackerFlags++;
                }

                gangwar.Flag2Team = (int)player.Team.Id;
            }
            else
            {
                if (gangwar.Flag3Team == (int)player.Team.Id)
                {
                    return;
                }




                if (gangwar.Flag3Team == gangwar.AttackerId)
                {
                    gangwar.AttackerFlags--;
                    gangwar.DefenderFlags++;
                }
                else
                {
                    gangwar.DefenderFlags--;
                    gangwar.AttackerFlags++;
                }


                gangwar.Flag3Team = (int)player.Team.Id;

            }
            if (gangwar.AttackerFlags < 0)
            {
                gangwar.AttackerFlags = 0;
            }
            if (gangwar.DefenderFlags < 0)
            {
                gangwar.DefenderFlags = 0;
            }

            await gangwar.UpdateGangwarHud();
        }

        public async override Task OnSecond()
        {
            var gws = Gangwars.FindAll(x => x.IsGettingAttacked).ToList();

            foreach (var gangwar in gws)
            {
                DateTime t1 = DateTime.Now;
                double seconds = 0;

                if (Configuration.DevMode)
                {
                    t1 = gangwar.LastAttacked.AddMinutes(1);
                    seconds = t1.Subtract(DateTime.Now).TotalSeconds;
                }
                else
                {
                    t1 = gangwar.LastAttacked.AddMinutes(20);
                    seconds = t1.Subtract(DateTime.Now).TotalSeconds;
                }

                if (t1.Subtract(DateTime.Now).TotalMinutes <= 0 && seconds <= 15)
                {
                    using var db = new RXContext();

                    var dbgangwar = await db.Gangwar.FirstOrDefaultAsync(x => x.Id == gangwar.Id);

                    if (gangwar.AttackerPoints >= gangwar.DefenderPoints)
                    {
                        gangwar.TeamId = gangwar.AttackerId;
                        dbgangwar.TeamId = gangwar.AttackerId;
                    }
                    else
                    {
                        gangwar.TeamId = gangwar.TeamId;
                        dbgangwar.TeamId = gangwar.TeamId;
                    }

                    var gwplayers = gangwarPlayers.Where(x => x.Value.Id == gangwar.Id).ToList();
                    var teams = new List<RXTeam>();

                    teams.Add(TeamModule.Teams.Find(x => x.Id == gangwar.TeamId));
                    teams.Add(TeamModule.Teams.Find(x => x.Id == gangwar.AttackerId));


                    var gwplayer = new List<GangwarPlayer>();

                    var gangwarstats = new GangwarStats() { Teams = teams, AttackerId = gangwar.AttackerId, DefenderId = gangwar.TeamId, AttackerPoints = (uint)gangwar.AttackerPoints, DefenderPoints = (uint)gangwar.DefenderPoints };

                    foreach (var pp in gwplayers)
                    {
                        gwplayer.Add(new GangwarPlayer() { Id = pp.Key.Id, Name = await pp.Key.GetNameAsync(), Kills = pp.Key.GangwarKills, Deaths = pp.Key.GangwarDeaths });
                    }
                    gangwarstats.Member = gwplayer;

                    gangwarstats.Member = gangwarstats.Member.OrderBy(x => x.Kills).Reverse().ToList();

                    foreach (var pp in gwplayers)
                    {
                        var window = new RXWindow("GangwarFinish");

                        await window.OpenWindow(pp.Key, gangwarstats);

                        await LeaveGangwar(pp.Key, gangwar);
                    }

                    NAPI.Task.Run(() =>
                    {
                        foreach (var GangwarVehicles in NAPI.Pools.GetAllVehicles().Where(x => x.Dimension == gangwar.Dimension))
                        {
                            GangwarVehicles.Delete();
                        }
                    });

                    gangwar.AttackerPoints = 0;
                    gangwar.AttackerFlags = 0;
                    gangwar.AttackerId = 0;
                    gangwar.DefenderPoints = 0;
                    gangwar.DefenderFlags = 0;
                    gangwar.GangwarStats = new List<RXGangwarStats>();
                    gangwar.GangwarMarker.Delete();
                    NAPI.Task.Run(() => gangwar.GangwarBlip.Color = TeamModule.Teams.Find(x => x.Id == gangwar.TeamId).BlipColor);
                    gangwar.IsGettingAttacked = false;
                    await db.SaveChangesAsync();
                }
            }
        }

        public async override Task OnThirtySecond()
        {

            var gws = Gangwars.FindAll(x => x.IsGettingAttacked).ToList();

            foreach (var gangwar in gws)
            {
                if (gangwar.Flag3Team == gangwar.AttackerId)
                {
                    gangwar.AttackerPoints += 5;
                }
                else if (gangwar.Flag3Team == gangwar.TeamId)
                {
                    gangwar.DefenderPoints += 5;
                }
                if (gangwar.Flag1Team == gangwar.AttackerId)
                {
                    gangwar.AttackerPoints += 5;
                }
                else if (gangwar.Flag1Team == gangwar.TeamId)
                {
                    gangwar.DefenderPoints += 5;

                }
                if (gangwar.Flag2Team == gangwar.AttackerId)
                {
                    gangwar.AttackerPoints += 5;
                }
                else if (gangwar.Flag2Team == gangwar.TeamId)
                {
                    gangwar.DefenderPoints += 5;

                }
                await gangwar.UpdateGangwarHud();
            }
        }

        [RemoteEvent]
        public async Task GWMedi(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if (IsPlayerInGangwar(player))
            {
                if (await player.GetHealthAsync() > 99) return;

                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(4000);

                player.DraggingTime = 4000;
                player.IsTaskAllowed = false;
                player.DraggingItem = true;

                await player.PlayAnimationAsync(33, "amb@medic@standing@tendtodead@idle_a", "idle_a", 8);
                await Task.Delay(4000);

                lock (player) if (!RX.PlayerExists(player)) return;

                if (!player.DraggingItem) return;

                player.DraggingItem = false;
                player.IsTaskAllowed = true;

                await player.SetHealthAsync(200);

                await player.SendNotify("Du hast einen Verbandskasten benutzt!", 3500, "green");
                // await player.StopProgressbar();
                await player.StopAnimationAsync();
                await player.disableAllPlayerActions(false);
            }

        }


        [RemoteEvent]
        public async Task GWWeste(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if (IsPlayerInGangwar(player))
            {

                await player.disableAllPlayerActions(true);
                await player.SendProgressbar(4000);

                player.DraggingTime = 4000;
                player.IsTaskAllowed = false;
                player.DraggingItem = true;

                await player.PlayAnimationAsync(33, "anim@heists@narcotics@funding@gang_idle", "gang_chatting_idle01", 8);
                await Task.Delay(4000);

                lock (player) if (!RX.PlayerExists(player)) return;

                if (!player.DraggingItem) return;

                player.DraggingItem = false;
                player.IsTaskAllowed = true;

                await player.SetArmorAsync(100);

                await player.SendNotify("Du hast eine Schutzweste benutzt!", 3500, "green");
                // await player.StopProgressbar();
                await player.StopAnimationAsync();
                await player.disableAllPlayerActions(false);

                await player.SetClothesAsync(9, 15, 2);
            }
        }

        public async Task StartGangwarZone(RXPlayer player, uint gwId)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            var gangwar = Gangwars.Find(x => x.Id == gwId);
            if (gangwar == null) return;

            if (player.Team.Id == 0 || player.Team.HasDuty || player.Team.Id == gangwar.TeamId) return;

            var team = TeamModule.Teams.FirstOrDefault(x => x.Id == gangwar.TeamId);
            var gangwarStart = new GangwarStart() { Id = gangwar.Id, canBeAttacked = !gangwar.IsGettingAttacked, CanBeAttackedDatum = "Bald", LastAttack = gangwar.LastAttacked.ToLocalTime().ToString(), Name = gangwar.Name, Image = team.Logo };

            await Window.OpenWindow(player, gangwarStart);
        }

        [RemoteEvent]
        public async Task StartGangwar(RXPlayer player, uint id)
        {
            var gangwar = Gangwars.Find(x => x.Id == id);
            if (gangwar == null) return;
            if (player.Team.Id == 0 || player.Team.HasDuty || player.Team.Id == gangwar.TeamId) return;

            if (!IsZoneAttackable(gangwar))
            {
                await player.SendNotify("Diese Zone steht noch unter einem Cooldown!", 3500, "red");
                return;
            }

            if (IsTeamInAnyGangwar(gangwar.TeamId))
            {
                await player.SendNotify("Das Team ist bereits im Gangwar!");
                return;
            }

            if (gangwar.IsGettingAttacked)
            {
                await player.SendNotify("Diese Zone wird bereits angegriffen!", 3500, "red");
                return;
            }

            var defender = TeamModule.Teams.FirstOrDefault(x => x.Id == gangwar.TeamId);
            var attacker = TeamModule.Teams.FirstOrDefault(x => x.Id == player.TeamId);

            if (defender.GetOnlineMemberCount() < 5)
            {
                await player.SendNotify("Es sind nicht genug Leute Deiner Fraktion online!");
                return;
            }

            if (attacker.GetOnlineMemberCount() < 5)
            {
                await player.SendNotify("Es sind nicht genug Leute der Fraktion online!");
                return;
            }

            gangwar.AttackerPoints = 0;
            gangwar.AttackerFlags = 0;
            gangwar.AttackerId = player.TeamId;
            gangwar.TeamId = gangwar.TeamId;
            gangwar.DefenderPoints = 0;
            gangwar.DefenderFlags = 0;
            gangwar.GangwarStats = new List<RXGangwarStats>();
            gangwar.IsGettingAttacked = true;

            defender.SendNotification("Euer Fraktionsgebiet " + gangwar.Name + " wird angegriffen!");
            attacker.SendNotification("Ihr greift das Gebiet " + gangwar.Name + " der Fraktion " + defender.Name + " an!");

            var newPosition = new Vector3(gangwar.Position.ToPos().X, gangwar.Position.ToPos().Y, gangwar.Position.ToPos().Z - 30f);
            var mcb = await NAPI.Entity.CreateMCB(newPosition, new Color(0, 238, 255, 180), gangwar.Dimension, (float)gangwar.Size, (float)gangwar.Size, true, (MarkerType)1, false);

            gangwar.GangwarMarker = mcb.Marker;
            gangwar.LastAttacked = DateTime.Now;

            foreach (var xPlayer in await PlayerController.GetPlayersInRange(gangwar.Position.ToPos(), gangwar.Size))
            {
                if (xPlayer == null) continue;

                if (xPlayer.TeamId == attacker.Id)
                {
                    await xPlayer.SetDimensionAsync(gangwar.Dimension);
                    RXWindow weaponselect = new RXWindow("GangwarWeaponSelect");
                    await weaponselect.OpenWindow(xPlayer);
                }
            }
        }

        [RemoteEvent]
        public async Task SelectGangwarWeaponPack(RXPlayer player, string pack)
        {
            var wpack = JsonConvert.DeserializeObject<GangwarWeaponPack>(pack);
            var gangwars = Gangwars;

            foreach (var gangwar in gangwars)
            {
                if (gangwar == null) continue;
                if (!gangwar.IsGettingAttacked) continue;

                player.gangwarWeaponPack = wpack;

                await EnterGangwar(player, gangwar);
            }
        }

        [RXCommand("quitgw")]
        public async Task quitgw(RXPlayer player, string[] args)
        {
            if (IsPlayerInGangwar(player))
            {
                DbGangwar gw;

                if (gangwarPlayers.TryGetValue(player, out gw))
                {
                    if (await player.GetDimensionAsync() != gw.Dimension) return;
                    string distancePositionIDK = null;

                    if (player.Team.Id == gw.AttackerId)
                    {
                        distancePositionIDK = gw.AttackerAusparker;
                    }
                    else
                    {
                        distancePositionIDK = gw.DefenderAusparker;
                    }

                    if (player.Position.DistanceTo(distancePositionIDK.ToPos()) > 5f)
                    {
                        await player.SendNotify("Du bist nicht in der Nähe des Fahrzeug Ausparkpunkt daher konntest du das Gangwar nicht verlassen!");
                        return;
                    }

                    await LeaveGangwar(player, gw);
                }
            }
        }

        public static async Task EnterGangwar(RXPlayer player, DbGangwar gangwar)
        {
            try
            {
                if (gangwar.AttackerId == player.Team.Id)
                {
                    await player.SetPositionAsync(gangwar.AttackerPosition.ToPos());
                }
                else
                {
                    await player.SetPositionAsync(gangwar.DefenderPosition.ToPos());
                }

                await player.SetDimensionAsync(gangwar.Dimension);

                foreach (var currentWeapon in player.Weapons.ToList())
                {
                    RXItemModel itemModel = ItemModelModule.ItemModels.FirstOrDefault(x => x.WeaponHash.ToLower() == currentWeapon.WeaponHash.ToLower());

                    if (itemModel != null)
                    {
                        await player.RemoveWeaponFromLoadout((WeaponHash)NAPI.Util.GetHashKey(itemModel.WeaponHash));
                        player.GangwarContainer.AddItem(itemModel, 1);
                    }
                }

                await player.SendNotify("Die Waffen aus deinem Inventar wurden in deinen Gangwar-Container gelegt diesen kannst du jederzeit im Fraktionslager öffnen!");
                await player.RemoveAllWeaponsAsync();

                var wpack = player.gangwarWeaponPack;

                if (wpack != null)
                {
                    await player.GiveWeaponAsync((WeaponHash)NAPI.Util.GetHashKey(ItemModelModule.ItemModels.Find(x => x.Id == wpack.WeaponId1).WeaponHash), 9999);
                    await player.GiveWeaponAsync((WeaponHash)NAPI.Util.GetHashKey(ItemModelModule.ItemModels.Find(x => x.Id == wpack.WeaponId2).WeaponHash), 9999);
                    await player.GiveWeaponAsync((WeaponHash)NAPI.Util.GetHashKey(ItemModelModule.ItemModels.Find(x => x.Id == wpack.WeaponId3).WeaponHash), 9999);
                }

                await player.SetHealthAsync(100);
                await player.SetArmorAsync(100);
                var defender = TeamModule.Teams.FirstOrDefault(x => x.Id == gangwar.TeamId);
                var attacker = TeamModule.Teams.FirstOrDefault(x => x.Id == gangwar.AttackerId);
                await player.TriggerEventAsync("StopGangwarHud");
                DateTime t1 = DateTime.Now;
                double seconds = 0;

                if (Configuration.DevMode)
                {
                    t1 = gangwar.LastAttacked.AddMinutes(1);
                    seconds = t1.Subtract(DateTime.Now).TotalSeconds;
                }
                else
                {
                    t1 = gangwar.LastAttacked.AddMinutes(20);
                    seconds = t1.Subtract(DateTime.Now).TotalSeconds;
                }

                await player.TriggerEventAsync("ShowGangwarHud", NAPI.Util.ToJson(TeamModule.Teams.Where(x => x.IsGangster()).ToList()), attacker.Id, defender.Id, gangwar.AttackerPoints, gangwar.DefenderPoints, gangwar.Flagcount, seconds);

                if (!IsPlayerInGangwar(player))
                {
                    gangwarPlayers.Add(player, gangwar);
                }
            }
            catch (Exception ex)
            {
                RXLogger.Print(ex.Message);
            }
        }

        public static async Task LeaveGangwar(RXPlayer player, DbGangwar gangwar)
        {
            if (await player.GetDimensionAsync() != gangwar.Dimension) return;

            if (IsPlayerInGangwar(player))
            {
                await player.SetDimensionAsync(0);
                await player.RemoveAllWeaponsAsync();
                await player.SetArmorAsync(0);
                player.GangwarKills = 0;
                player.GangwarDeaths = 0;

                await player.SetPositionAsync(player.Team.Spawn);
                await player.TriggerEventAsync("StopGangwarHud");

                gangwarPlayers.Remove(player);

                await player.SendNotify("Gangwar verlassen!");
            }
        }

        public static bool IsPlayerInGangwar(RXPlayer player)
        {
            if (!gangwarPlayers.ContainsKey(player)) return false;

            return true;
        }

        public static bool IsTeamInAnyGangwar(uint teamid)
        {

            if (Gangwars.FirstOrDefault(x => x.TeamId == teamid && x.IsGettingAttacked) != null) return true;

            return false;
        }

        public static bool IsZoneAttackable(DbGangwar gangwar)
        {
            if (gangwar.LastAttacked.AddHours(12) > DateTime.Now) return false;

            return true;
        }
    }
}
