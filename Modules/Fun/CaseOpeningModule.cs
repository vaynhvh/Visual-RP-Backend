using Backend.Models;
using Backend.Modules.Inventory;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Fun
{

    public class CaseObject
    {
        public int i { get; set; }
        public string image { get; set; }
        public string name { get; set; }

    }
    class CaseOpeningModule : RXModule
        {
            public CaseOpeningModule() : base("CaseOpening") { }

            public static Dictionary<RXPlayer, int> caseopening = new Dictionary<RXPlayer, int>();
        
            public static int[] normal = new int[11] { 107, 90, 83, 82, 81, 80, 34, 36, 35, 22, 122 };
            public static int[] epic = new int[14] { 107, 90, 83, 82, 81, 80, 34, 36, 35, 22, 122, 123, 86, 87};
            public static int[] legendary = new int[17] { 107, 90, 83, 82, 81, 80, 34, 36, 35, 22, 122, 123, 86, 87, 88, 124, 125 };


        //events.ShowIF("CaseOpening", JSON.stringify({t: 0, image: "25coupon.png", i: 1, a: [{i: 1, image: "25coupon.png"}, {i: 1, image: "25coupon.png"}]}))
        public static async Task OpenCase(RXPlayer player, string type)
        {
            Random ran = new Random();
            List<CaseObject> winitems = new List<CaseObject>();
            if (type == "Normal")
            {

                if (CaseOpeningModule.caseopening.ContainsKey(player))
                {
                    CaseOpeningModule.caseopening.Remove(player);
                }

              
                RXWindow window = new RXWindow("CaseOpening");

                int firstitem = normal[ran.Next(0, normal.Length)];
                int winitem = normal[ran.Next(0, normal.Length)];
                var aitem = ItemModelModule.ItemModels.Find(x => x.Id == winitem);

                if (aitem == null)
                {
                    await player.SendNotify("Es gab einen Fehler!");
                    return;
                }
                var item = ItemModelModule.ItemModels.Find(x => x.Id == firstitem);

                if (item == null)
                {
                    await player.SendNotify("Es gab einen Fehler!");
                    return;
                }

                foreach (int cc in normal)
                {
                    var witem = ItemModelModule.ItemModels.Find(x => x.Id == cc);
                    if (witem == null) continue;

                    winitems.Add(new CaseObject() { i = (int)witem.Id, image = witem.ImagePath, name = witem.Name });

                }

                caseopening.Add(player, winitem);

                object losobj = new
                {
                    t = 0,
                    image = item.ImagePath,
                    name = item.Name,
                    i = item.Id,
                    a = winitems
                };

                await window.OpenWindow(player, losobj);

                int element = 0;

                for (element = 0; element < 24; element++)
                {
                    int randitem = normal[ran.Next(0, normal.Length)];
                    var witem = ItemModelModule.ItemModels.Find(x => x.Id == randitem);
                    if (witem == null) continue;
                    await player.TriggerEventAsync("updateCaseElement", witem.Id, witem.ImagePath, witem.Name);

                    await Task.Delay(500);
                }
                await player.TriggerEventAsync("updateCaseElement", aitem.Id, aitem.ImagePath, aitem.Name);
            }
            if (type == "Epic")
            {

                if (CaseOpeningModule.caseopening.ContainsKey(player))
                {
                    CaseOpeningModule.caseopening.Remove(player);
                }


                RXWindow window = new RXWindow("CaseOpening");

                int firstitem = epic[ran.Next(0, epic.Length)];
                int winitem = epic[ran.Next(0, epic.Length)];
                var aitem = ItemModelModule.ItemModels.Find(x => x.Id == winitem);

                if (aitem == null)
                {
                    await player.SendNotify("Es gab einen Fehler!");
                    return;
                }
                var item = ItemModelModule.ItemModels.Find(x => x.Id == firstitem);

                if (item == null)
                {
                    await player.SendNotify("Es gab einen Fehler!");
                    return;
                }

                foreach (int cc in epic)
                {
                    var witem = ItemModelModule.ItemModels.Find(x => x.Id == cc);
                    if (witem == null) continue;

                    winitems.Add(new CaseObject() { i = (int)witem.Id, image = witem.ImagePath, name = witem.Name });

                }

                caseopening.Add(player, winitem);

                object losobj = new
                {
                    t = 0,
                    image = item.ImagePath,
                    name = item.Name,
                    i = item.Id,
                    a = winitems
                };

                await window.OpenWindow(player, losobj);

                int element = 0;

                for (element = 0; element < 24; element++)
                {
                    int randitem = epic[ran.Next(0, epic.Length)];
                    var witem = ItemModelModule.ItemModels.Find(x => x.Id == randitem);
                    if (witem == null) continue;
                    await player.TriggerEventAsync("updateCaseElement", witem.Id, witem.ImagePath, witem.Name);

                    await Task.Delay(500);
                }
                await player.TriggerEventAsync("updateCaseElement", aitem.Id, aitem.ImagePath, aitem.Name);
            }
            if (type == "Legendary")
            {

                if (CaseOpeningModule.caseopening.ContainsKey(player))
                {
                    CaseOpeningModule.caseopening.Remove(player);
                }


                RXWindow window = new RXWindow("CaseOpening");

                int firstitem = legendary[ran.Next(0, legendary.Length)];
                int winitem = legendary[ran.Next(0, legendary.Length)];
                var aitem = ItemModelModule.ItemModels.Find(x => x.Id == winitem);

                if (aitem == null)
                {
                    await player.SendNotify("Es gab einen Fehler!");
                    return;
                }
                var item = ItemModelModule.ItemModels.Find(x => x.Id == firstitem);

                if (item == null)
                {
                    await player.SendNotify("Es gab einen Fehler!");
                    return;
                }

                foreach (int cc in legendary)
                {
                    var witem = ItemModelModule.ItemModels.Find(x => x.Id == cc);
                    if (witem == null) continue;

                    winitems.Add(new CaseObject() { i = (int)witem.Id, image = witem.ImagePath, name = witem.Name });

                }

                caseopening.Add(player, winitem);

                object losobj = new
                {
                    t = 0,
                    image = item.ImagePath,
                    name = item.Name,
                    i = item.Id,
                    a = winitems
                };

                await window.OpenWindow(player, losobj);

                int element = 0;

                for (element = 0; element < 24; element++)
                {
                    int randitem = legendary[ran.Next(0, legendary.Length)];
                    var witem = ItemModelModule.ItemModels.Find(x => x.Id == randitem);
                    if (witem == null) continue;
                    await player.TriggerEventAsync("updateCaseElement", witem.Id, witem.ImagePath, witem.Name);

                    await Task.Delay(500);
                }
                await player.TriggerEventAsync("updateCaseElement", aitem.Id, aitem.ImagePath, aitem.Name);
            }
        }

        [RemoteEvent]
        public async Task TakeCaseOpeningWin(RXPlayer player)
        {
            if (await player.GetIsInVehicleAsync() || player.IsTied || player.DeathData.IsDead || !player.IsLoggedIn || player.IsCuffed || !player.IsTaskAllowed) return;

            if (caseopening.ContainsKey(player))
            {
                int value;
                if (caseopening.TryGetValue(player, out value))
                {
                    var aitem = ItemModelModule.ItemModels.Find(x => x.Id == value);

                    if (aitem == null)
                    {
                        await player.SendNotify("Es gab einen Fehler!");
                        return;
                    }

                    player.Container.AddItem(aitem, 1);

                    caseopening.Remove(player);

                }
            }
            else
            {
                await player.SendNotify("Es ist ein Fehler aufgetreten. Bitte melde diesen umgehend im Support!", 5000, "red");
            }

        }
    }
}
