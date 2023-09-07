using Backend.Models;
using Backend.Modules.Discord;
using Backend.Modules.Faction;
using Backend.Modules.Gangwar;
using Backend.Modules.Laptop.Apps;
using Backend.Modules.Paintball;
using Backend.Modules.Phone.Apps;
using Backend.Modules.Voice;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using Google.Protobuf.WellKnownTypes;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Asn1.X509;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    class DeathController : Script
    {
        public static async Task ApplyDeathEffectsAsync(RXPlayer player)
        {
            if (!player.IsLoggedIn || !player.DeathData.IsDead) return;

            if (player.IsHigh())
            {
                await player.TriggerEventAsync("stopScreenEffect", "DrugsMichaelAliensFight");
                player.Joints = 0;
                player.LastJoint = DateTime.MinValue;
            }

            await NAPI.Task.RunAsync(() =>
            {
                player.SetSharedData("voiceRange", (int)VoiceRange.normal);
                player.SetData("voiceType", 1);
                player.TriggerEvent("setVoiceType", 1);
            });

            await VoiceModule.ChangeFrequency(player, 0);

            await player.disableAllPlayerActions(true);

            player.Freezed = true;
            player.Invincible = true;

            await player.StopAnimationAsync();
            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missarmenian2", "corpse_search_exit_ped");
        }

        public static async Task ApplyDeathEffectsNoInvincibleAsync(RXPlayer player)
        {
            if (!player.IsLoggedIn) return;

            await NAPI.Task.RunAsync(() =>
            {
                player.SetSharedData("voiceRange", (int)VoiceRange.normal);
                player.SetData("voiceType", 1);
                player.TriggerEvent("setVoiceType", 1);
            });

            await VoiceModule.ChangeFrequency(player, 0);

            await player.disableAllPlayerActions(true);

            player.Freezed = true;

            await player.StopAnimationAsync();
            await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "random@dealgonewrong", "idle_a");
        }

        [ServerEvent(Event.PlayerDeath)]
        public async Task OnPlayerDeath(RXPlayer player, RXPlayer killer, uint hash)
        {
            if (!player.IsLoggedIn || player.DeathData.IsDead) return;

            await TelefonApp.CancelCall(player);

            bool killedByPlayer = killer != null && killer != player && killer.IsLoggedIn;

            if (hash == 133987706 || hash == 2741846334)
            {
                if (killer != null && killer.IsLoggedIn && await killer.GetIsInVehicleAsync())
                {
                    hash = 133987706; // Run over by car (if other player is involved in Fall)

                    RX.SendNotifyToAllWhich(pl => pl.InAduty, "Der Spieler " + await killer.GetNameAsync() + " wurde als verdächtig eingestuft. Grund: VDM", 8000, "red", "Administration");

                    if (killer.VDMCounter >= 1)
                    {
                        RX.SendNotifyToAllWhich(pl => pl.InAduty, "Der Spieler " + await killer.GetNameAsync() + " wurde automatisch gekickt. Grund: VDM", 8000, "red", "Administration");

                        await killer.KickPlayer("VDM");

                        killer = null;
                    }
                    else
                    {
                        killer.VDMCounter++;
                        killer.PlayerKills.Add(await player.GetNameAsync());

                        NAPI.Task.Run(() => killer.VDMCounter = 0, 5000);
                    }

                }
            }

            string killerWeapon = Convert.ToString((WeaponHash)hash) != "" ? Convert.ToString((WeaponHash)hash) : "unbekannt";

            if (killedByPlayer)
            {
                player.LastKiller = await killer.GetNameAsync();
                if (!player.inPaintball && !killer.inPaintball)
                {
                    killer.PlayerKills.Add(await player.GetNameAsync());
                }

                DiscordModule.Logs.Add(new DiscordLog("Death", (await player.GetNameAsync()) + " wurde von " + await killer.GetNameAsync() + " mit " + killerWeapon + " getötet.", "https://discord.com/api/webhooks/1142917806053142749/DANn6Hpymnj344G0gUH5ZYoBqiKGGN6az8loF6GE1ft1z__TGYjnRDCAbfU7jeOCIVkX"));



                if (GangwarModule.IsPlayerInGangwar(player))
                {
                    DbGangwar gw;
                    if (GangwarModule.gangwarPlayers.TryGetValue(player, out gw))
                    {

                        var ppos = await player.GetPositionAsync();
                        if (ppos.DistanceTo(gw.Position.ToPos()) < gw.Size)
                        {



                            if (gw.AttackerId == killer.Team.Id && killer.Team.Id != player.Team.Id)
                            {
                                gw.AttackerPoints += 3;
                                await killer.SendNotify("Deine Fraktion hat 3 Punkte für das Eliminieren eines Gegners erhalten!");
                            }
                            else if (gw.TeamId == killer.Team.Id && killer.Team.Id != player.Team.Id)
                            {
                                gw.DefenderPoints += 3;
                                await killer.SendNotify("Deine Fraktion hat 3 Punkte für das Eliminieren eines Gegners erhalten!");
                            }
                            await gw.UpdateGangwarHud();
                        }
                    }
                }
                if (player.inPaintball && killer.inPaintball)
                {

                    await killer.SendNotify("Du hast " + await player.GetNameAsync() + " umgescheppert!");
                    await player.SendNotify("Du wurdest von " + await killer.GetNameAsync() + " umgescheppert! (HP: " + await killer.GetHealthAsync() + "/Armor: " + await killer.GetArmorAsync() + ")");

                    await killer.SetHealthAsync(100);
                    await killer.SetArmorAsync(100);

                    var lobby = PaintballModule.getPlayerLobby(player);

                    if (lobby != null)
                    {

                        var pp = lobby.participants.Find(x => x.PlayerId == player.Id);
                        var kk = lobby.participants.Find(x => x.PlayerId == killer.Id);



                        if (pp.PlayerTeam == kk.PlayerTeam && lobby.TypeId == 2)
                        {
                            await killer.SendNotify("Teamkilling = Hurensohn!");
                        }
                        else
                        {
                            killer.Paintballkills++;
                            player.Paintballdeaths++;
                        }


                        pp.PlayerDeaths = player.Paintballdeaths;
                        kk.PlayerKills = killer.Paintballkills;


                        if (lobby.TypeId == 3)
                        {
                            killer.FFAKillStreak++;

                            if (player.FFAKillStreak == 0)
                            {
                                player.FFAKillStreak = 0;
                            }
                            else
                            {
                                player.FFAKillStreak--;
                            }

                            await PaintballModule.GivePlayerArenaWeapons(killer);

                        }


                    }

                }
                player.LastKillerWeapon = killerWeapon;
            }
            else
            {
                DiscordModule.Logs.Add(new DiscordLog("Death", (await player.GetNameAsync()) + " ist gestorben.", "https://discord.com/api/webhooks/1142917806053142749/DANn6Hpymnj344G0gUH5ZYoBqiKGGN6az8loF6GE1ft1z__TGYjnRDCAbfU7jeOCIVkX"));

            }

            if (!player.inPaintball && !player.TrainingsDuty)
            {
                await player.TriggerEventAsync("transitionToBlurred", 200);
            }

            await NAPI.Task.RunAsync(() => player.ResetData("stabalized"));

            player.Injured = false;

            if (player.Dimension == 0)
            {
                player.ResetData("CurrentServiceIndex");
                player.ResetData("CurrentServiceTeamID");
                await ServiceApp.SendService(player, 2, "Es wurde eine schwerverletzte Person gemeldet!");
            }

            player.DeathData = new RXDeathData
            {
                DeathTime = DateTime.Now,
                IsDead = true
            };

            using var db = new RXContext();

            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
            if (dbPlayer == null) return;

            dbPlayer.DeathStatus = true;
            dbPlayer.DeathTime = DateTime.Now;

            await db.SaveChangesAsync();

            await player.SpawnAsync(await player.GetPositionAsync());

            player.Injured = false;
            player.Freezed = true;

            await ApplyDeathEffectsAsync(player);


            await NAPI.Task.RunAsync(() => player.ResetData("stabalized"));
        }
        public static async Task OnFiveSecond()
        {
            await PlayerController.GetValidPlayers().forEachAlternativeAsync(async player =>
            {
                if (player.DeathData.IsDead && player.inPaintball)
                {
                    player.DeathData.DeathTime = DateTime.Now;
                    player.DeathData.IsDead = false;
                    player.Coma = false;
                    player.Freezed = false;
                    player.Invincible = false;

                    player.Invisible = false;

                    await player.disableAllPlayerActions(false);
                    await player.StopAnimationAsync();

                    await PaintballModule.SpawnInArena(player);
                    await player.TriggerEventAsync("transitionFromBlurred", 50);

                    await new RXWindow("Death").CloseWindow(player);

                    using var db = new RXContext();

                    var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
                    if (dbPlayer == null) return;

                    dbPlayer.Coma = false;
                    dbPlayer.DeathStatus = false;
                    dbPlayer.DeathTime = DateTime.Now;


                    await db.SaveChangesAsync();

                }
                if (player.DeathData.IsDead && player.TrainingsDuty)
                {

                    player.DeathData.DeathTime = DateTime.Now;
                    player.DeathData.IsDead = false;
                    player.Coma = false;
                    player.Freezed = false;
                    player.Invincible = false;

                    player.Invisible = false;

                    await player.disableAllPlayerActions(false);
                    await player.StopAnimationAsync();

                    await player.TriggerEventAsync("transitionFromBlurred", 50);

                    await new RXWindow("Death").CloseWindow(player);

                    using var db = new RXContext();

                    var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
                    if (dbPlayer == null) return;

                    dbPlayer.Coma = false;
                    dbPlayer.DeathStatus = false;
                    dbPlayer.DeathTime = DateTime.Now;


                    await db.SaveChangesAsync();

                }
            });
        }
        public static async Task OnMinute()
        {
            try
            {
                await PlayerController.GetValidPlayers().forEachAlternativeAsync(async player =>
                {
                    if (!player.inPaintball && player.DeathData.IsDead || !player.inPaintball && player.Injured)
                    {

                        await player.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "missarmenian2", "corpse_search_exit_ped");

                        if ((DateTime.Now - player.DeathData.DeathTime).TotalMinutes > 1 && GangwarModule.IsPlayerInGangwar(player))
                        {

                            player.DeathData.DeathTime = DateTime.Now;
                            player.DeathData.IsDead = false;
                            player.Coma = false;
                            player.Freezed = false;
                            player.Invincible = false;

                            player.Invisible = false;

                            await player.disableAllPlayerActions(false);
                            await player.StopAnimationAsync();
                            await player.SpawnAsync(player.Team.Spawn, 90f);

                            await new RXWindow("Death").CloseWindow(player);

                            using var db = new RXContext();

                            var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
                            if (dbPlayer == null) return;

                            dbPlayer.Coma = false;
                            dbPlayer.DeathStatus = false;
                            dbPlayer.DeathTime = DateTime.Now;

                            await player.TriggerEventAsync("transitionFromBlurred", 2000);



                            await db.SaveChangesAsync();

                            DbGangwar gw;
                            if (GangwarModule.gangwarPlayers.TryGetValue(player, out gw))
                            {
                                await GangwarModule.EnterGangwar(player, gw);
                            }
                        }
                        else
                        if ((DateTime.Now - player.DeathData.DeathTime).TotalMinutes > 5)
                        {
                            if (player.Injured && !player.OnWayToKH)
                            {
                                await player.RevivePlayer();
                            }
                            else
                            {
                                if (!player.Coma && !player.OnWayToKH)
                                {
                                    using var db = new RXContext();

                                    var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
                                    if (dbPlayer == null) return;

                                    dbPlayer.Coma = true;
                                    dbPlayer.Weapons = "[]";
                                    dbPlayer.Cash = 0;
                                    dbPlayer.DeathTime = DateTime.Now;

                                    db.Items.RemoveRange(db.Items.ToList().Where(item => item.InventoryId == dbPlayer.InventoryId));

                                    await db.SaveChangesAsync();

                                    await NAPI.Task.RunAsync(async () =>
                                        await NAPI.Pools.GetAllObjects().Where(x => x.Position.DistanceTo(player.Position) < 4).forEachAlternativeAsync(x => x.Delete()));

                                    player.DeathData.DeathTime = DateTime.Now;
                                    player.Coma = true;
                                    player.Weapons.Clear();
                                    player.Joints = 0;
                                    player.Cash = 0;
                                    player.Blackmoney = 0;
                                    player.Invisible = true;
                                    player.DeathProp = await NAPI.Task.RunReturnAsync(() => NAPI.Object.CreateObject(NAPI.Util.GetHashKey("xm_prop_body_bag"), player.Position.Subtract(new Vector3(0, 0, 1)), new Vector3(), 255, 0));

                                    await player.RemoveAllWeaponsAsync();

                                    await new RXWindow("Death").OpenWindow(player);
                                }
                                else
                                {

                                    player.DeathData.DeathTime = DateTime.Now;
                                    player.DeathData.IsDead = false;
                                    player.Coma = false;
                                    player.Freezed = false;
                                    player.Invincible = false;
                                    player.Invisible = false;

                                    await player.disableAllPlayerActions(false);
                                    await player.StopAnimationAsync();
                                    await player.SpawnAsync(new Vector3(296.95038, -588.1542, 43.2609), 67.85581f);

                                    await player.TriggerEventAsync("transitionFromBlurred", 2000);

                                    await new RXWindow("Death").CloseWindow(player);

                                    using var db = new RXContext();

                                    var dbPlayer = await db.Players.FirstOrDefaultAsync(x => x.Id == player.Id);
                                    if (dbPlayer == null) return;

                                    dbPlayer.Coma = false;
                                    dbPlayer.DeathStatus = false;
                                    dbPlayer.DeathTime = DateTime.Now;

                                    await db.SaveChangesAsync();
                                }
                            }
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                DiscordModule.Logs.Add(new DiscordLog("Crash", ex.ToString(), DiscordModule.Errors));
            }
        }
    }
}
