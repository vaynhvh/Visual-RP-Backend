using Backend.Models;
using Backend.Models.Appearance;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Clothing
{
    class ClothingModule : RXModule
    {

        public ClothingModule() : base("Clothing") { }

        public override async Task PressedM(RXPlayer player)
        {
            if (player == null || !player.CanInteract()) return;

            await PlayerSwitchClothState(player, 1, false, true);    

        }

        [RemoteEvent]
        public async Task ToggleCloth(RXPlayer player, int id)
        {
            if (player == null || !player.CanInteract()) return;

            if (id < 0)
            {
                await PlayerSwitchClothState(player, ConvertToPositive(id), true, true);

            }
            else
            {
                await PlayerSwitchClothState(player, id, false, true);
            }

        }
        public static int ConvertToPositive(int i)
        {
            return (i + (i >> 31)) ^ (i >> 31);
        }
        [RemoteEvent]
        public async Task inventoryChooseProp(RXPlayer player, int id)
        {
            if (player == null || !player.CanInteract()) return;

            await PlayerSwitchClothState(player, id, true, true);

        }

        public static async Task PlayerSwitchClothState(RXPlayer iPlayer, int rawchoice, bool prop, bool toggle)
        {
            try
            {
                uint choice = (uint)rawchoice; // Maskierung

                using var db = new RXContext();

                DbCharacter dbCharacter = await db.Characters.FirstOrDefaultAsync(c => c.Id == iPlayer.Id);
                if (dbCharacter == null) return;

                Customization customization = JsonConvert.DeserializeObject<Customization>(dbCharacter.Customization);
                if (customization == null) return;

                if (iPlayer == null) return;
                if (!iPlayer.CanInteract()) return;

                if (iPlayer.HasData("lastmaskestate"))
                {
                    DateTime latest = iPlayer.GetData<DateTime>("lastmaskestate");
                    if (latest.AddSeconds(2) > DateTime.Now) return;
                }

                iPlayer.SetData("lastmaskestate", DateTime.Now);
                Dictionary<int, RXClothesProp> clothesParts = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Clothes);
                Dictionary<int, RXClothesProp> clothesProps = JsonConvert.DeserializeObject<Dictionary<int, RXClothesProp>>(dbCharacter.Accessories);

                if (!prop)
                {

                    if (!clothesParts.ContainsKey((int)choice))
                        return;

                    int cc = (int)choice;

                    if (!iPlayer.Freezed && iPlayer.CanInteract() && !await iPlayer.GetIsInVehicleAsync() && toggle)
                    {
                        if (cc == 1)
                        {
                            await iPlayer.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "missfbi4", "takeoff_mask");

                        }
                        else if (cc == 11)
                        {
                            await iPlayer.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "clothingtie", "try_tie_negative_a");
                        }
                        else if (cc == 3)
                        {
                            await iPlayer.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "clothingtie", "try_tie_negative_a");
                        }
                        else if (cc == 4)
                        {
                            await iPlayer.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "re@construction", "out_of_breath");
                        }
                        else if (cc == 6)
                        {
                            await iPlayer.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "random@domestic", "pickup_low");
                        }
                        else if (cc == 7)
                        {
                            await iPlayer.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "clothingtie", "try_tie_positive_a");

                        }
                    }

                    await Task.Delay(1300);


                    if (clothesParts[cc].active == null)
                    {
                        clothesParts[cc].active = false;
                    }


                    if (!toggle || clothesParts.ContainsKey(cc) && clothesParts[cc].active) //Spieler hat das Kleidungsstück an
                    {
                        if (cc == 6)
                        {
                            if (!dbCharacter.Gender)
                            {
                                await iPlayer.SetClothesAsync(cc, 35, 0);
                            } else
                            {
                                await iPlayer.SetClothesAsync(cc, 34, 0);
                            }
                        } else if (cc == 11)
                        {
                            if (!dbCharacter.Gender)
                            {
                                await iPlayer.SetClothesAsync(cc, 5, 0);
                            }
                            else
                            {
                                await iPlayer.SetClothesAsync(cc, 15, 0);
                            }
                        }
                        else if (cc == 7)
                        {
                            if (!dbCharacter.Gender)
                            {
                                await iPlayer.SetClothesAsync(cc, 0, 0);
                            }
                            else
                            {
                                await iPlayer.SetClothesAsync(cc, 0, 0);
                            }
                        }
                        else if (cc == 4)
                        {
                            if (!dbCharacter.Gender)
                            {
                                await iPlayer.SetClothesAsync(cc, 15, 0);
                            }
                            else
                            {
                                await iPlayer.SetClothesAsync(cc, 21, 0);
                            }
                        }
                        else if (cc == 3)
                        {
                            if (!dbCharacter.Gender)
                            {
                                await iPlayer.SetClothesAsync(cc, 15, 0);
                            }
                            else
                            {
                                await iPlayer.SetClothesAsync(cc, 15, 0);
                            }
                        }
                        else
                        {
                            await iPlayer.SetClothesAsync(cc, 0, 0);

                        }
                        if (toggle)
                        {
                            clothesParts[cc].active = false;
                        }

                        if (!iPlayer.Freezed && iPlayer.CanInteract() && !await iPlayer.GetIsInVehicleAsync() && toggle)
                            await iPlayer.StopAnimationAsync();


                        dbCharacter.Clothes = JsonConvert.SerializeObject(clothesParts);
                        dbCharacter.Accessories = JsonConvert.SerializeObject(clothesProps);

                        await db.SaveChangesAsync();
                        return;
                    }
                    if (clothesParts.ContainsKey(cc) && !clothesParts[cc].active) //Spieler hat das Kleidungsstück nicht an
                    {

                        await iPlayer.SetClothesAsync(cc, clothesParts[cc].drawable, clothesParts[cc].texture);
                        if (toggle)
                        {
                            clothesParts[cc].active = true;
                        }
                        if (!iPlayer.Freezed && iPlayer.CanInteract() && !await iPlayer.GetIsInVehicleAsync() && toggle)
                            await iPlayer.StopAnimationAsync();

                        dbCharacter.Clothes = JsonConvert.SerializeObject(clothesParts);
                        dbCharacter.Accessories = JsonConvert.SerializeObject(clothesProps);
                        await db.SaveChangesAsync();
                        return;
                    }

                } else
                {
                    if (!clothesProps.ContainsKey((int)choice))
                        return;

                    int cc = (int)choice;

                    if (!iPlayer.Freezed && iPlayer.CanInteract() && !await iPlayer.GetIsInVehicleAsync() && toggle)
                    {
                        if (cc == 1)
                        {
                            await iPlayer.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "clothingspecs", "take_off");

                        }
                        else if (cc == 2)
                        {
                            await iPlayer.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl | AnimationFlags.OnlyAnimateUpperBody), "mp_cp_stolen_tut", "b_think");

                        }
                    }

                    await Task.Delay(1300);


                    if (clothesProps[cc].active == null)
                    {
                        clothesProps[cc].active = false;
                    }


                    if (clothesProps.ContainsKey(cc) && clothesProps[cc].active) //Spieler hat das Kleidungsstück an
                    {
                        if (cc == 0 || cc == 2 || cc == 6 || cc == 7)
                        {
                            await iPlayer.SetAccessoriesAsync(cc, -1, 0);
                        } else
                        {
                            await iPlayer.SetAccessoriesAsync(cc, 0, 0);
                        }
                        if (toggle)
                        {
                            clothesProps[cc].active = false;
                        }

                        if (!iPlayer.Freezed && iPlayer.CanInteract() && !await iPlayer.GetIsInVehicleAsync() && toggle)
                            await iPlayer.StopAnimationAsync();


                        dbCharacter.Clothes = JsonConvert.SerializeObject(clothesParts);
                        dbCharacter.Accessories = JsonConvert.SerializeObject(clothesProps);

                        await db.SaveChangesAsync();
                        return;
                    }
                    if (clothesProps.ContainsKey(cc) && !clothesProps[cc].active) //Spieler hat das Kleidungsstück nicht an
                    {

                        await iPlayer.SetAccessoriesAsync(cc, clothesProps[cc].drawable, clothesProps[cc].texture);
                        if (toggle)
                        {
                            clothesProps[cc].active = true;
                        }
                        if (!iPlayer.Freezed && iPlayer.CanInteract() && !await iPlayer.GetIsInVehicleAsync() && toggle)
                            await iPlayer.StopAnimationAsync();

                        dbCharacter.Clothes = JsonConvert.SerializeObject(clothesParts);
                        dbCharacter.Accessories = JsonConvert.SerializeObject(clothesProps);
                        await db.SaveChangesAsync();
                        return;
                    }
                }

                // remove anim if still nothing occured
                
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }
    }
}
