using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Inventory;
using Backend.Modules.Staatsfraktionen.LSMC;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Update;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.X509;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ubiety.Dns.Core;
using static Backend.Models.RXContainer;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Player
{
    public class WeaponListObject
    {
        public string PersonToFrisk { get; set; }
        public bool CanForceWeaponDrop { get; set; }
        public List<WeaponListContainer> WeaponList { get; set; }

        public WeaponListObject(string personToFrisk, bool canForceWeaponDrop, List<WeaponListContainer> weaponList)
        {
            PersonToFrisk = personToFrisk;
            CanForceWeaponDrop = canForceWeaponDrop;
            WeaponList = weaponList;
        }
    }

    public class WeaponListContainer
    {
        public string WeaponName { get; set; }
        public int WeaponCount { get; set; }
        public string WeaponIcon { get; set; }

        public WeaponListContainer(string weaponName, int weaponCount, string weaponIcon)
        {
            WeaponName = weaponName;
            WeaponCount = weaponCount;
            WeaponIcon = weaponIcon;
        }
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class PlayerModule : RXModule
    {
        public PlayerModule() : base("PlayerMenu", new RXWindow("GiveMoney")) { }


        public override async Task OnPlayerDisconnect(RXPlayer player, DisconnectionType type, string reason)
        {
            await player.TriggerEventAsync("DestroyBlips");

            try
            {
                string disconnect_reason = "Spiel verlassen";
                switch (type)
                {
                    case DisconnectionType.Left:
                        disconnect_reason = "Spiel verlassen";
                        break;
                    case DisconnectionType.Timeout:
                        disconnect_reason = "Verbindung verloren";
                        break;
                    case DisconnectionType.Kicked:
                        disconnect_reason = "Gekickt";
                        break;
                }

                // Send Logout Message
                foreach (RXPlayer dbPlayer in PlayerController.GetValidPlayers())
                {
                    if (dbPlayer != null) continue;
                    if (await dbPlayer.GetDimensionAsync() != await player.GetDimensionAsync()) continue;
                    var playerpos = await dbPlayer.GetPositionAsync();
                    if (playerpos.DistanceTo(await player.GetPositionAsync()) <= 40.0f)
                    {
                        await dbPlayer.SendNotify(await dbPlayer.GetNameAsync() + " hat das Spiel verlassen. (Grund: " + disconnect_reason + ")", 5000, "orange");
                    }
                }
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }

        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task GiveMoney(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Id == target.Id || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 20f) return;


            object confirmationBoxObject = new
            {
                t = "Wie viel $ willst du übergeben?",
                e = "GivePlayerMoney",
                d = await target.GetNameAsync()
            };

            var confirmation = new RXWindow("Input");

            await confirmation.OpenWindow(player, confirmationBoxObject);

        }

        [RemoteEvent]
        public async Task SaltyChat_MicStateChanged(RXPlayer player, bool ismuted)
        {
            await player.TriggerEventAsync("setMicMuted", ismuted);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task ShowDoc(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Id == target.Id || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 20f) return;

            await player.SendNotify("Du hast deinen Personalausweis gezeigt!");
            var name = (await player.GetNameAsync()).Split('_');

            List<object> ids = new List<object>();
            List<object> type = new List<object>();

            using var db = new RXContext();

            var playerlicenses = await db.PlayerLicenses.Where(x => x.PlayerId == player.Id).ToListAsync();


            ids.Add(new
            {
                firstname = name[0],
                lastname = name[1],
                //birthday = player.DateOfEntry,
                entrydata = player.DateOfEntry,
                visum = player.Level,
                gender = 0

            });
            type.Add(new
            {
                id = 1,
                type = 0
            });
          /*  var erstehilfe = playerlicenses.FirstOrDefault(x => x.LicenseId == 3);
            if (erstehilfe != null)
            {

                type.Add(new
                {
                    id = 2,
                    type = 3
                });

                ids.Add(new
                {
                    firstname = name[0],
                    lastname = name[1],
                    dateofsign = erstehilfe.DateOfSign.ToString("mm:HH dd:MM:yyyy"),
                    entrydata = player.DateOfEntry,
                    visum = player.Level,
                    gender = 0

                });
            }
          */
            await target.TriggerEventAsync("DisplayLicence", 0, NAPI.Util.ToJson(type), NAPI.Util.ToJson(ids));

        }
        [RemoteEvent]
        public async Task ShowLicence(RXPlayer player, uint id)
        {
            if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;
            var target = PlayerController.GetClosestPlayer(player, await player.GetPositionAsync(), 2f);

            if (target == null || target.Id == player.Id) return;

            await player.SendNotify("Du hast deine Lizenz gezeigt!");
            var name = (await player.GetNameAsync()).Split('_');

            List<object> ids = new List<object>();
            List<object> type = new List<object>();

            using var db = new RXContext();

            var playerlicenses = await db.PlayerLicenses.Where(x => x.LicenseId == id).FirstOrDefaultAsync();

            if (playerlicenses == null) return;


                  type.Add(new
                  {
                      id = playerlicenses.LicenseId,
                      type = id
                  });

                  ids.Add(new
                  {
                      firstname = name[0],
                      lastname = name[1],
                      dateofsign = playerlicenses.DateOfSign.ToString("dd.MM.yyyy"),
                      entrydata = player.DateOfEntry,
                      visum = player.Level,
                      authority = playerlicenses.SignerId,
                      gender = 0

                  });
              
            
            await target.TriggerEventAsync("DisplayLicence", id, NAPI.Util.ToJson(type), NAPI.Util.ToJson(ids));

        }


        //[HandleExceptions]
        [RemoteEvent]
        public async Task TakeDoc(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Id == target.Id || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 20f) return;

            if (target.DeathData.IsDead || target.IsTied || target.IsCuffed)
            {
                await player.SendNotify("Du hast den Personalausweis genommen!");
                var name = (await target.GetNameAsync()).Split('_');

                List<object> ids = new List<object>();
                List<object> type = new List<object>();

                object idcard = new
                {
                    firstname = name[0],
                    lastname = name[1],
                    birthday = target.DateOfEntry,
                    entrydata = target.DateOfEntry,
                    visum = target.Level,
                    gender = 0

                };
                object types = new
                {
                    id = 1,
                    type = 0
                };

                ids.Add(idcard);
                type.Add(types);

                await player.TriggerEventAsync("DisplayLicence", 0, NAPI.Util.ToJson(type), NAPI.Util.ToJson(ids));
            }
        }

        // J3nnileon30 //
        [RemoteEvent]
        public async Task InvSearch(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Injured || player.DeathData.IsDead || player.Id == target.Id || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if (!target.DeathData.IsDead && !target.IsCuffed && !target.IsTied)
            {
                await player.SendNotify("Der Spieler ist nicht gefesselt!");
                return;
            }

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 20f) return;

            RXWindow window = new RXWindow("Inventory");

            player.IsTaskAllowed = false;

            await target.SendNotify("Du wirst durchsucht.");
            await player.SendNotify("Dursuche...");

            await player.SendProgressbar(5000);

            await player.StopAnimationAsync();
            await player.PlayAnimationAsync((int)(AnimationFlags.Loop), "missheistdockssetup1ig_3@base", "welding_base_dockworker", 1);

            player.DraggingTime = 5000;
            player.DraggingItem = true;

            await Task.Delay(5000);

            lock (player) if (!RX.PlayerExists(player)) return;

            if (!player.DraggingItem) return;

            await player.StopAnimationAsync();
            await player.StopProgressbar();

            List<RXClientContainer> containerList = new List<RXClientContainer>
            {
                player.Container.ConvertForClient(player.Container.Id, "Rucksack"),
                target.Container.ConvertForClient(target.Container.Id, "Rucksack")
            };

            player.IsTaskAllowed = true;
            player.DraggingItem = false;

            await window.OpenWindow(player, new { s = false, i = containerList, w = false }, false);
        }
        // J3nnileon30 //

        [RemoteEvent]
        public async Task REQUEST_CARRY_PLAYER(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Id == target.Id || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 20f) return;

            if (target.DeathData.IsDead || target.Injured)
            {
                if (target.IsCarried) return;

                target.IsCarried = true;
                player.IsCarry = true;
                var players = await PlayerController.GetPlayersInRange(await player.GetPositionAsync(), 125f);
                foreach (var hund in players)
                {
                    await hund.TriggerEventAsync("attachmeto", target, player);
                }
                await player.PlayAnimationAsync(49, "missfinale_c2mcs_1", "fin_c2_mcs_1_camman", 8);
                await target.PlayAnimationAsync(33, "nm", "firemans_carry", 8);
                player.SetData<RXPlayer>("carryperson", target);

            }
        }

        [RemoteEvent]
        public async Task requestVehicleSyncData(RXPlayer p_Player, RXVehicle p_RequestedVehicle)
        {


            RXVehicle l_SxVehicle = p_RequestedVehicle;
            if (l_SxVehicle == null || l_SxVehicle.Id == 0)
                return;

            var l_Tuning = l_SxVehicle.Mods;
            var l_DoorStates = l_SxVehicle.DoorStates;

            try
            {
                string l_SerializedTuning = JsonConvert.SerializeObject(l_Tuning);
                string l_SerializedDoor = JsonConvert.SerializeObject(l_DoorStates);


                await p_Player.TriggerEventAsync("responseVehicleSyncData", p_RequestedVehicle, JsonConvert.SerializeObject(l_Tuning),
                    JsonConvert.SerializeObject(l_DoorStates), 0);
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        [RemoteEvent]
        public async Task toggleCrouch(RXPlayer dbPlayer)
        {
            if (dbPlayer == null || await dbPlayer.GetIsInVehicleAsync() || !dbPlayer.IsTaskAllowed || dbPlayer.Injured || dbPlayer.DeathData.IsDead)
            {
                return;
            }

                if (dbPlayer.HasData("isCrouched"))
                {
                    dbPlayer.ResetData("isCrouched");



                Vector3 playerpos = await dbPlayer.GetPositionAsync();
               
                foreach(var player in await PlayerController.GetPlayersInRange(playerpos))
                {
                    await player.TriggerEventAsync("loadCrouchClipsets");
                    await player.TriggerEventAsync("changeCrouchingState", dbPlayer, false);
                }
                }
                else if (await dbPlayer.GetCurrentWeaponAsync() == WeaponHash.Unarmed)
                {
                    dbPlayer.SetData("isCrouched", true);
                Vector3 playerpos = await dbPlayer.GetPositionAsync();

                foreach (var player in await PlayerController.GetPlayersInRange(playerpos))
                {
                    await player.TriggerEventAsync("loadCrouchClipsets");
                    await player.TriggerEventAsync("changeCrouchingState", dbPlayer, true);
                }
            }
        }

        [RemoteEvent]
        public async Task FirstAid(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Id == target.Id || player.IsCuffed || player.IsTied || target.Coma || (!target.DeathData.IsDead && !target.Injured) || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 5f) return;

            if (target.OnWayToKH)
            {

                if (!target.HasData("InInjuryRevivePoint")) return;

                var injurypos = target.GetData<DbInjury>("InInjuryRevivePoint");


                await player.PlayAnimationAsync(33, "mini@cpr@char_a@cpr_def", "cpr_intro", 1);
                player.Freezed = true;

                if (player.Team.IsGangster())
                {
                    await player.SendProgressbar(15000);
                    await Task.Delay(15000);
                }
                else
                {
                    await player.SendProgressbar(9000);
                    await Task.Delay(9000);
                }
                await Task.Delay(1000);
                player.Freezed = false;
                await player.StopAnimationAsync();
                target.Freezed = true;
                target.OnWayToKH = false;
                await target.TriggerEventAsync("noweaponsoninjury", false);
                await target.SendNotify($"Du wurdest behandelt!");
                await player.SendNotify($"Du hast den Patienten behandelt!");
                await target.RevivePlayer();
                return;
            }
            if (await NAPI.Task.RunReturnAsync(() => target.HasData("stabalized")) && player.TeamId != 3)
            {
                await player.SendNotify("Die Person wurde bereits stabilisiert!");
                return;
            } else if (await NAPI.Task.RunReturnAsync(() => player.TeamId == 3 && target.HasData("stabalized")))
            {
                var sxVehicle = await NAPI.Task.RunReturnAsync(() => VehicleController.GetClosestTeamVehicle(player.Position, 15.0f, player.TeamId));

                if (sxVehicle == null)
                {
                    await player.SendNotify($"Kein Krankenwagen zum Transport in der Nähe!");
                    return;
                }
                await player.PlayAnimationAsync(33, "amb@medic@standing@tendtodead@idle_a", "idle_a", 1);
                player.Freezed = true;

                if (player.Team.IsGangster())
                {
                    await player.SendProgressbar(15000);
                    await Task.Delay(15000);
                }
                else
                {
                    await player.SendProgressbar(9000);
                    await Task.Delay(9000);
                }
                await player.StopAnimationAsync();
                await NAPI.Task.RunAsync(() => target.SetIntoVehicle(sxVehicle, 2));
                await Task.Delay(1000);
                player.Freezed = false;
                target.Freezed = true;
                target.OnWayToKH = true;
                await target.TriggerEventAsync("noweaponsoninjury", true);
                await target.SendNotify($"Du wurdest transportbereit gemacht!");
                await player.SendNotify($"Du hast den Patienten transportbereit gemacht!");
                return;
            }


            await player.disableAllPlayerActions(true);
            await player.SendProgressbar(10000);

            player.IsTaskAllowed = false;

            await player.PlayAnimationAsync(33, "mini@cpr@char_a@cpr_def", "cpr_intro", 1);
            await Task.Delay(10000);

            lock (player) if (!RX.PlayerExists(player)) return;

            player.IsTaskAllowed = true;

            await player.SendNotify("Du hast die Person stabilisiert!");
            await target.SendNotify("Jemand hat dich stabilisiert!");

            if (target.Injured)
                target.DeathData.DeathTime = DateTime.Now.AddMinutes(-9);
            else
                target.DeathData.DeathTime = target.DeathData.DeathTime.AddMinutes(5);

            await player.StopAnimationAsync();
            await player.disableAllPlayerActions(false);

            await NAPI.Task.RunAsync(() => target.SetData<bool>("stabalized", true));


        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task REQUEST_PEDS_PLAYER_FRISK(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Id == target.Id || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 3.2f) return;

            player.resetFriskInventoryFlags();
            player.resetDisabledInventoryFlag();

            if (!target.IsCuffed && !target.IsTied && !target.DeathData.IsDead)
            {
                await player.SendNotify("Die Person muss gefesselt sein!");
                return;
            }

            if (await NAPI.Task.RunReturnAsync(() => !player.HasData("lastfriskperson")) || await NAPI.Task.RunReturnAsync(() => player.GetData<uint>("lastfriskperson")) != target.Id)
            {
                await player.TriggerEventAsync("freezePlayer", true);
                await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@prop_human_parking_meter@male@base", "base");

                await player.SendProgressbar(8000);
                await Task.Delay(8000);

                await player.TriggerEventAsync("freezePlayer", false);
                await player.StopAnimationAsync();
            }

            await NAPI.Task.RunAsync(() => player.SetData("lastfriskperson", target.Id));

            var lWeapons = target.Weapons;
            if (lWeapons.Count > 0)
            {
                var lWeaponListContainer = new List<WeaponListContainer>();
                foreach (var lWeapon in lWeapons)
                {
                    var lData = ItemModelModule.ItemModels.FirstOrDefault(x => x.WeaponHash.ToLower() == lWeapon.WeaponHash.ToLower());
                    if (lData == null) continue;

                    lWeaponListContainer.Add(new WeaponListContainer(lData.Name, 1000, lData.ImagePath));
                }

                /* 
                 * 
                 *  IScop abfrage soon
                 * 
                 */

                if (lWeaponListContainer.Count > 0)
                {
                    player.IsInvDisabled = true;

                    var lWeaponListObject = new WeaponListObject(await target.GetNameAsync(), false, lWeaponListContainer);

                    await new RXWindow("Frisk").OpenWindow(player, new { weaponListObject = lWeaponListObject });

                    return;
                }
            }

            player.resetFriskInventoryFlags();
            player.resetDisabledInventoryFlag();

            target.Container.ShowFriskInventory(player, target, "Spieler", target.Cash + target.Blackmoney);
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task FootCuff(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Id == target.Id || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 2.0f) return;

            float distance = await NAPI.Task.RunReturnAsync(() => Math.Abs(target.Heading - player.Heading));

            if (distance > 45) return;

            if (target.DeathData.IsDead || target.IsTied || await NAPI.Task.RunReturnAsync(() => target.GetData<string>("lastCuffedTied")) == "tied") return;

            await NAPI.Task.RunAsync(() =>
            {
                if (target.HasData("follow"))
                {
                    target.IsTied = true;
                    target.ResetData("follow");
                    target.TriggerEvent("toggleShooting", false);
                }
            });

            if (target.IsCuffed)
            {
                await NAPI.Task.RunAsync(() => target.Rotation = new Vector3(0, 0, player.Heading));
                await Task.Delay(500);

                await player.TriggerEventAsync("freezePlayer", true);
                await player.CanInteractAntiFloodNoMSG(5);
                await target.TriggerEventAsync("freezePlayer", true);
                await target.CanInteractAntiFloodNoMSG(5);

                await target.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "b_uncuff");
                await player.PlayAnimationAsync((int)(AnimationFlags.StopOnLastFrame | AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "a_uncuff");

                await Task.Delay(5000);

                await player.TriggerEventAsync("freezePlayer", false);
                await target.TriggerEventAsync("freezePlayer", false);

                await player.StopAnimationAsync();

                await Task.Delay(500);

                target.SetCuffed(false);

                await NAPI.Task.RunAsync(() => target.ResetData("lastCuffedTied"));

                await player.SendNotify("Du hast jemanden die Handschellen abgenommen!");
                await target.SendNotify("Ein Beamter hat dir Handschellen abgenommen!");

                return;
            }
            else
            {

                await NAPI.Task.RunAsync(() => NAPI.Player.SetPlayerCurrentWeapon(target, WeaponHash.Unarmed));

                await player.TriggerEventAsync("freezePlayer", true);
                await player.CanInteractAntiFloodNoMSG(5);
                await target.TriggerEventAsync("freezePlayer", true);
                await target.CanInteractAntiFloodNoMSG(5);

                await player.StopAnimationAsync();

                await NAPI.Task.RunAsync(() => target.Rotation = new Vector3(0, 0, player.Heading));
                await Task.Delay(500);

                await target.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "arrested_spin_l_0");
                await player.PlayAnimationAsync((int)(AnimationFlags.StopOnLastFrame | AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arrest_paired", "arrestcop_p2_back_righted_spin_l_0");

                await Task.Delay(5000);

                await player.TriggerEventAsync("freezePlayer", false);
                await target.TriggerEventAsync("freezePlayer", false);

                await player.StopAnimationAsync();
                await target.StopAnimationAsync();

                await Task.Delay(500);

                target.SetCuffed(true);

                await NAPI.Task.RunAsync(() => target.SetData("lastCuffedTied", "cuffed"));

                await player.SendNotify("Du hast jemanden die Handschellen angelegt!");
                await target.SendNotify("Ein Beamter hat dir Handschellen angelegt!");
            }
        }


        //[HandleExceptions]
        [RemoteEvent]
        public async Task Tie(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Id == target.Id || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 2.0f) return;

            float distance = await NAPI.Task.RunReturnAsync(() => Math.Abs(target.Heading - player.Heading));

            if (distance > 45) return;

            if (target.DeathData.IsDead || target.IsCuffed || await NAPI.Task.RunReturnAsync(() => target.GetData<string>("lastCuffedTied")) == "cuffed") return;

            await NAPI.Task.RunAsync(() =>
            {
                if (target.HasData("follow"))
                {
                    target.IsTied = true;
                    target.ResetData("follow");
                    target.TriggerEvent("toggleShooting", false);
                }
            });

            if (target.IsTied)
            {
                await NAPI.Task.RunAsync(() => target.Rotation = new Vector3(0, 0, player.Heading));
                await Task.Delay(500);

                await player.TriggerEventAsync("freezePlayer", true);
                await player.CanInteractAntiFloodNoMSG(5);
                await target.TriggerEventAsync("freezePlayer", true);
                await target.CanInteractAntiFloodNoMSG(5);

                await target.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "b_uncuff");
                await player.PlayAnimationAsync((int)(AnimationFlags.StopOnLastFrame | AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "a_uncuff");

                await Task.Delay(5000);

                await player.TriggerEventAsync("freezePlayer", false);
                await target.TriggerEventAsync("freezePlayer", false);

                await player.StopAnimationAsync();

                await Task.Delay(500);

                target.SetTied(false);

                await NAPI.Task.RunAsync(() => target.ResetData("lastCuffedTied"));

                await player.SendNotify("Du hast jemanden entfesselt!");
                await target.SendNotify("Du wurdest von jemandem entfesselt!");

                return;
            }
            else
            {
                var model = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == "Seil");
                if (model == null) return;

                if (player.Container.GetItemAmount(model) == 0)
                {
                    await player.SendNotify("Du benötigst ein Seil um einen Spieler zu fesseln!");
                    return;
                }

                player.Container.RemoveItem(model);

                await NAPI.Task.RunAsync(() => NAPI.Player.SetPlayerCurrentWeapon(target, WeaponHash.Unarmed));

                await player.TriggerEventAsync("freezePlayer", true);
                await player.CanInteractAntiFloodNoMSG(5);
                await target.TriggerEventAsync("freezePlayer", true);
                await target.CanInteractAntiFloodNoMSG(5);

                await player.StopAnimationAsync();

                await NAPI.Task.RunAsync(() => target.Rotation = new Vector3(0, 0, player.Heading));
                await Task.Delay(500);

                await target.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arresting", "arrested_spin_l_0");
                await player.PlayAnimationAsync((int)(AnimationFlags.StopOnLastFrame | AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "mp_arrest_paired", "cop_p2_back_right");

                await Task.Delay(5000);

                await player.TriggerEventAsync("freezePlayer", false);
                await target.TriggerEventAsync("freezePlayer", false);

                await player.StopAnimationAsync();
                await target.StopAnimationAsync();

                await Task.Delay(500);

                target.SetTied(true);

                await NAPI.Task.RunAsync(() => target.SetData("lastCuffedTied", "tied"));

                await player.SendNotify("Du hast jemanden gefesselt!");
                await target.SendNotify("Du wurdest von jemandem gefesselt!");
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task Cuff(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Id == target.Id || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync() || !await player.CanInteractAntiFloodNoMSG(1)) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 2.5f) return;

            if (await player.GetIsInVehicleAsync() || await target.GetIsInVehicleAsync())
            {
                await player.SendNotify("Du oder die Person dürfen nicht in einem Fahrzeug sein!");
                return;
            }

            if ((await NAPI.Task.RunReturnAsync(() => target.GetData<string>("lastCuffedTied"))) == "cuffed" && !player.Team.IsState())
            {
                return;
            }

            if (!await NAPI.Task.RunReturnAsync(() => target.HasData("follow")))
            {
                if (!target.IsCuffed && !target.IsTied)
                {
                    await player.SendNotify("Spieler ist nicht gefesselt!");
                    return;
                }

                await player.SendNotify("Du hast jemanden gepackt!");
                await target.SendNotify("Jemand hat dich gepackt!");

                if ((await NAPI.Task.RunReturnAsync(() => target.GetData<string>("lastCuffedTied"))) == "cuffed")
                {
                    target.SetCuffed(false);
                }
                else
                {
                    target.SetTied(false);
                }

                await target.TriggerEventAsync(PlayerDatas.TiedEvent, true);
                await NAPI.Task.RunAsync(() => target.SetData("follow", player.Name));
                await target.TriggerEventAsync("toggleShooting", true);
                await target.StopAnimationAsync();
                await Task.Delay(1000);
                await target.PlayAnimationAsync((int)(AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@move_m@prisoner_cuffed_rc", "aim_low_loop");
            }
            else
            {
                await target.StopAnimationAsync();

                await player.SendNotify("Du hast jemanden losgelassen!");
                await target.SendNotify("Jemand hat dich losgelassen!");

                if ((await NAPI.Task.RunReturnAsync(() => target.GetData<string>("lastCuffedTied"))) == "cuffed")
                {
                   target.SetCuffed(true);
                }
                else
                {
                    target.SetTied(true);
                }

                await NAPI.Task.RunAsync(() => target.ResetData("follow"));
                await target.TriggerEventAsync("toggleShooting", false);
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task Give(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || target == null || !target.IsLoggedIn || player.Id == target.Id || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 2.5f) return;

            await NAPI.Task.RunAsync(() => player.SetData("giveitem", target.Id));
                            await target.StopAnimationAsync();
                await Task.Delay(1000);
            await InventoryModule.requestInventory(player, true);
        }

        [RemoteEvent]
        public async Task GrabPlayer(RXPlayer player, RXPlayer target)
        {
            if (!player.IsLoggedIn || !target.IsLoggedIn || await player.GetIsInVehicleAsync())
            {
                return;
            }

            if (target.IsCuffed == false && target.IsTied == false)
            {
                await player.SendNotify("Der Spieler ist nicht gefesselt!");
                return;
            }

            var veh = VehicleController.GetClosestVehicle(await target.GetPositionAsync(), 4);

            if (veh == null)
            {
                await player.SendNotify("Kein Fahrzeug");
                return;
            }

            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "amb@medic@standing@tendtodead@base", "base");
            await player.SendProgressbar(4000);
            await Task.Delay(4000);
            await player.StopAnimationAsync();
            await player.StopProgressbar();
            await player.SendNotify("Der Spieler wurde ins Fahrzeug gesetzt!");
            if (await veh.IsSeatFree(2))
            {
                await target.SetIntoVehicleAsync(veh, 2);
            }
            else if (await veh.IsSeatFree(3))
            {
                await target.SetIntoVehicleAsync(veh, 3);
            }
            else
            {
                await target.SetIntoVehicleAsync(veh, 1);
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task GivePlayerMoney(RXPlayer player, int money, string targetPlayerName)
        {
            if (!player.IsLoggedIn || await player.GetNameAsync() == targetPlayerName || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

            if (money <= 0)
            {
                await player.SendNotify("Der Betrag ist ungültig!");
                return;
            }

            var target = await PlayerController.FindPlayerByName(targetPlayerName);
            if (target == null || target.Id == player.Id) return;

            if ((await target.GetPositionAsync()).DistanceTo(await player.GetPositionAsync()) > 20f) return;

            if (await player.TakeMoney(money))
            {
                await target.GiveMoney(money);

                await player.SendNotify("Du hast " + money.FormatMoneyNumber() + " übergeben!", 3500, "green");
                await target.SendNotify("Dir wurden " + money.FormatMoneyNumber() + " zugesteckt!", 3500, "green");
            }
            else
            {
                await player.SendNotify("Du hast nicht genügend Geld dabei!");
            }
        }

        //[HandleExceptions]
        public async override Task OnFiveSecond()
        {
            await PlayerController.GetValidPlayers().forEachAlternativeAsync(async player =>
            {
                await DeathController.OnFiveSecond();

                if (!player.IsHigh()) await player.TriggerEventAsync("stopScreenEffect", "DrugsMichaelAliensFight");

                if (player.Sport > 99 && player.IsHigh())
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.6);
                }
                else if (player.Sport > 99)
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.4);
                }
                else if (player.Sport > 85 && player.IsHigh())
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.5);
                }
                else if (player.Sport > 85)
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.3);
                }
                else if (player.Sport > 75 && player.IsHigh())
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.4);
                }
                else if (player.Sport > 75)
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.2);
                }
                else if (player.Sport > 50 && player.IsHigh())
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.3);
                }
                else if (player.Sport > 50)
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.1);
                }
                else if (player.Sport > 25 && player.IsHigh())
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.2);
                }
                else if (player.Sport > 25)
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.05);
                } 
                else if (player.IsHigh())
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1.1);
                }
                else
                {
                    await player.TriggerEventAsync("setRunSprintMultiplierFor", 1);
                }



                if (player.IsCarried)
                {
                    await player.PlayAnimationAsync(33, "nm", "firemans_carry", 8);
                }

                
                if (await NAPI.Task.RunReturnAsync(() => player.HasData("follow")))
                {
                    RXPlayer followedPlayer = await PlayerController.FindPlayerByName(await NAPI.Task.RunReturnAsync(() => player.GetData<string>("follow")));
                    if (followedPlayer != null && followedPlayer.IsLoggedIn && !followedPlayer.DeathData.IsDead && !player.DeathData.IsDead && !await player.GetIsInVehicleAsync())
                    {
                        if (await NAPI.Task.RunReturnAsync(() => followedPlayer.Position.DistanceTo(player.Position)) > 10.0f)
                        {
                            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missarmenian2", "corpse_search_exit_ped");
                            await player.TriggerEventAsync("freezePlayer", true);
                            await Task.Delay(2000);
                            await player.TriggerEventAsync("freezePlayer", false);
                            await player.StopAnimationAsync();
                            await player.PlayAnimationAsync((int)(AnimationFlags.OnlyAnimateUpperBody | AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@move_m@prisoner_cuffed_rc", "aim_low_loop");
                        }
                    }
                }
            });
        }

        public async override Task OnTwentyMinutes()
        {
            await PlayerController.GetValidPlayers().forEachAlternativeAsync(player =>
            {
                if (player.Hunger > 0)
                {
                    player.Hunger -= new Random().Next(3, 9);
                }
                if (player.Thirst > 0)
                {
                    player.Thirst -= new Random().Next(3, 9);
                }
            });

        }
        public async override Task OnHour()
        {
            await PlayerController.GetValidPlayers().forEachAlternativeAsync(player =>
            {
                player.Sport -= 10;
            });

        }
        public async override Task OnMinute()
        {
            await DeathController.OnMinute();

            await PlayerController.GetValidPlayers().forEachAlternativeAsync(async player =>
            {
                if (player.Paytime > 0)
                {
                    player.Paytime--;
                }

                if (player.Paytime == 120 && player.Rank != null && player.Rank.Permission > 90 || player.Paytime == 60 && player.Rank != null && player.Rank.Permission > 90 || player.Paytime == 0 && player.Rank != null && player.Rank.Permission > 90)
                {
                    await player.GivePTAPoints("Console", "Console", "Aktivität (+10 Punkte)", 10);
                }

                if (player.Paytime == 0)
                {
                    player.Paytime = 180;
                    player.Level++;

                    await player.SendNotify("Du bist erfolgreich ein Level aufgestiegen und hast einen Sozialbonus von " + player.Level * 2000 + "$ erhalten!");
                    await player.GiveMoney(player.Level * 2000);
                }
            });
        }

        //[HandleExceptions]
        [RemoteEvent]
        public void fistDamage(RXPlayer player, RXPlayer target)
        {
            if (target != null)
            {
                if (target.HasSharedData("Invincible") && target.GetSharedData<bool>("Invincible")) return;

                target.Health -= 6;
            }
        }

        public override async Task PressedE(RXPlayer player)
        {

            if (player.HasData("carryperson"))
            {
                var target = player.GetData<RXPlayer>("carryperson");

                if (target == null) return;

                await target.StopAnimationAsync();
                await player.StopAnimationAsync();
                player.IsCarry = false;
                target.IsCarried = false;
                player.ResetData("carryperson");
                var players = await PlayerController.GetPlayersInRange(await player.GetPositionAsync(), 125f);
                foreach (var hund in players)
                {
                    await hund.TriggerEventAsync("deattachme", target);
                }
            }
        }

        [RemoteEvent]
        public async Task Pressed_H(RXPlayer player)
        {
            if (!player.CanInteract() || !player.IsTaskAllowed) return;

            if (await NAPI.Task.RunReturnAsync(() => player.HasData("handsup")))
            {
                var players = await PlayerController.GetPlayersInRange(await player.GetPositionAsync(), 125f);
                foreach (var hund in players)
                {
                    await hund.TriggerEventAsync("stopAnimationSlowForAll", player);
                }
                await NAPI.Task.RunAsync(() => player.ResetData("handsup"));
            }
            else
            {
                await player.PlayAnimationAsync(49, "missfbi5ig_21", "hand_up_scientist");
                await NAPI.Task.RunAsync(() => player.SetData<bool>("handsup", true));
            }
        }
    }
}
