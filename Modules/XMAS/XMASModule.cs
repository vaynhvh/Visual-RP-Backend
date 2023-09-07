using Backend.MySql.Models;
using Backend.MySql;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Backend.Models;
using GTANetworkAPI;
using Backend.Utils.Extensions;
using System.IO;
using System.Threading;
using Backend.Modules.Inventory;

namespace Backend.Modules.XMAS
{
    class XMASModule : RXModule
    {
        public XMASModule() : base("XMASModule") { }

        public static List<DbPlayerXMAS> OpenGifts = new List<DbPlayerXMAS>();

        /*public override async void LoadAsync()
         {
             using var db = new RXContext();

             foreach (DbPlayerXMAS gift in await db.XMAS.ToListAsync())
             {
                 OpenGifts.Add(gift);
             }

             var mcb = await NAPI.Entity.CreateMCB(new GTANetworkAPI.Vector3(1079.4077, -704.62756, 57.747025), new Color(255, 140, 0), 0u, 2f, 2.4f, false, MarkerType.VerticalCylinder, true, 781, 4, "Christmas-Geschenke");

             mcb.ColShape.Message = new RXMessage
             {
                 Text = "Drücke E deine Geschenke zu öffnen!",
                 Color = "white",
                 Duration = 3500,
                 Title = "XMAS"
             };

             mcb.ColShape.Action = async player => await GetXMAS(player);

             await NAPI.Task.RunAsync(() =>
             {
                 new NPC((PedHash)NAPI.Util.GetHashKey("u_m_m_jesus_01"), new GTANetworkAPI.Vector3(1079.4077, -704.62756, 57.747025), 178.02f, 0u);
             });
         }


           if ($choose == 1) {
          return "Fahrzeugbrief GT63s AMG";
        } elseif ($choose == 2) {
          return "Wertanlage 25.000$";
         } elseif ($choose == 3) {
          return "Wertanlage 10.000$";
         } elseif ($choose == 4) {
          return "Telefonnummer Gutschein";
         } elseif ($choose == 5) {
          return "Westenkiste (10)";
         } elseif ($choose == 6) {
          return "Fahrzeugbrief Schlagen GT";
         } elseif ($choose == 7) {
          return "30 Freispiele Casino";
         } elseif ($choose == 8) {
          return "Wertanlage 100.000$";
         } elseif ($choose == 9) {
          return "Westenkiste (5)";
         } else {
          return "Kleidungsgutschein Addon Kleidung";
        }
 }

         public async Task GetXMAS(RXPlayer player)
         {
             if (!player.IsLoggedIn || player.IsCuffed || player.IsTied || player.DeathData.IsDead || await player.GetIsInVehicleAsync()) return;

             foreach (DbPlayerXMAS gift in player.PlayerGifts)
             {
                 if (gift.UsedIngame == 0)
                 {
                     gift.UsedIngame = 1;

                     if (gift.GiftType == 1)
                     {
                         await player.SendNotify("Du hast das Geschenk vom Tag " + gift.Day + " geöffnet und hast Fahrzeugbrief GT63s AMG erhalten!");
                         player.Container.AddItem(79, 1);

                     }
                     else if (gift.GiftType == 2)
                     {
                         await player.GiveMoney(25000);
                         await player.SendNotify("Du hast das Geschenk vom Tag " + gift.Day + " geöffnet und hast 25.000$ Wertanlage erhalten!");
                     }
                     else if (gift.GiftType == 3)
                     {
                         await player.GiveMoney(10000);
                         await player.SendNotify("Du hast das Geschenk vom Tag " + gift.Day + " geöffnet und hast 10.000$ Wertanlage erhalten!");
                     }
                     else if (gift.GiftType == 4)
                     {
                         await player.SendNotify("Du hast das Geschenk vom Tag " + gift.Day + " geöffnet und hast einen Telefonnummer Change Gutschein erhalten!");
                         player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Telefonnummer Gutschein"));
                     }
                     else if (gift.GiftType == 5)
                     {
                         await player.SendNotify("Du hast das Geschenk vom Tag " + gift.Day + " geöffnet und hast eine Westenkiste (10) erhalten!");
                         player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Westenkiste"));
                     }
                     else if (gift.GiftType == 6)
                     {
                         await player.SendNotify("Du hast das Geschenk vom Tag " + gift.Day + " geöffnet und hast Fahrzeugbrief Schlagen GT erhalten!");
                         player.Container.AddItem(80, 1);
                     }
                     else if (gift.GiftType == 7)
                     {
                         await player.SendNotify("Du hast das Geschenk vom Tag " + gift.Day + " geöffnet und hast 30 Freispiele im Casino erhalten!");
                         player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Casino-Freispiele"), 30);
                     }
                     else if (gift.GiftType == 8)
                     {
                         await player.GiveMoney(100000);
                         await player.SendNotify("Du hast das Geschenk vom Tag " + gift.Day + " geöffnet und hast 100.000$ Wertanlage erhalten!");
                     } else if (gift.GiftType == 9)
                     {
                         await player.SendNotify("Du hast das Geschenk vom Tag " + gift.Day + " geöffnet und hast eine Westenkiste (5) erhalten!");
                         player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Westenkiste"));
                     } else
                     {
                         await player.SendNotify("Du hast das Geschenk vom Tag " + gift.Day + " geöffnet und hast einen Kleidungsgutschein erhalten!");
                         player.Container.AddItem(ItemModelModule.ItemModels.Find(x => x.Name == "Kleidungsgutschein"));
                     }


                     using var db = new RXContext();

                     var giftDb = await db.XMAS.FirstOrDefaultAsync(x => x.Id == gift.Id);
                     if (giftDb == null) return;

                     giftDb.UsedIngame = 1;

                     await db.SaveChangesAsync();
                 }
             }

         }

         */
    }
}
