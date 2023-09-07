using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Attachment;
using Backend.Modules.Faction;
using Backend.Modules.Garage;
using Backend.Modules.Inventory;
using Backend.Modules.Leitstellen;
using Backend.Modules.Vehicle;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Laptop
{
    public class OverviewVehicle
    {
        [JsonProperty(PropertyName = "i")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "p")]
        public bool InGarage { get; set; }

        [JsonProperty(PropertyName = "g")]
        public string GarageName { get; set; }

        [JsonProperty(PropertyName = "n")]
        public string Vehiclehash { get; set; }

        [JsonProperty(PropertyName = "no")]
        public string Notiz { get; set; }

        [JsonProperty(PropertyName = "carCor")]
        public CarCoordinate CarCor { get; set; }

        public bool iR { get; set; } = false;
    }

    public class CarCoordinate
    {
        [JsonProperty(PropertyName = "x")]
        public float position_x { get; set; }
        [JsonProperty(PropertyName = "y")]
        public float position_y { get; set; }
        [JsonProperty(PropertyName = "z")]
        public float position_z { get; set; }
    }

    public class ComputerCheckData

    {

        [JsonProperty(PropertyName = "oo")]
        public bool Leitstelle { get; set; }
    }

    public class LaptopApp
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        public LaptopApp(string id, string name, string icon)
        {
            Id = id;
            Name = name;
            Icon = icon;
        }
    }

    class LaptopModule : RXModule
    {
        public LaptopModule() : base("Laptop", new RXWindow("PoliceComputer")) { }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task computerCheck(RXPlayer player, uint type)
        {
            if (!player.CanInteract()) return;

            if (player.Team.IsState() && await player.GetIsInVehicleAsync())
            {
                RXVehicle veh = await player.GetVehicleAsync(); 
                if (veh == null) return;

                ComputerCheckData cc = new ComputerCheckData { Leitstelle = LeitstellenModule.IsLeiststelle(player) };

                await this.Window.OpenWindow(player, cc);
            } 

          
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task closeComputer(RXPlayer player, uint type)
        {
            if (!player.CanInteract()) return;

            if (!await player.GetIsInVehicleAsync())
            {
                await player.StopAnimationAsync();
                await AttachmentModule.RemoveAllAttachments(player);

            }

            if (type == 1)
            {
                await player.TriggerEventAsync("closeComputer");
            }
            else if (player.Rank.Permission > 0 && player.InAduty)
            {
                await player.TriggerEventAsync("closeIpad");
            }
        }

        [RemoteEvent]//[HandleExceptions, RemoteEvent]
        public async Task requestComputerApps(RXPlayer player)
        {
            if (!player.IsLoggedIn) return;

            await Task.Delay(100);

            var desktopApp = new RXWindow("DesktopApp");

            List<LaptopApp> laptopApps = new List<LaptopApp>();

            laptopApps.Add(new LaptopApp("FahrzeugUebersichtApp", "KFZ Info", "234788.svg"));

            if (player.TeamId > 0 && player.Team != null)
            {
                laptopApps.Add(new LaptopApp("FraktionListApp", "Fraktion", "1055644.svg"));
            }

            if (player.TeamId > 0 && player.Team != null && player.Team.Type == TeamType.LSPD)
            {
                laptopApps.Add(new LaptopApp("PoliceAktenSearchApp", "Akten", "858320.svg"));
                laptopApps.Add(new LaptopApp("StreifenApp", "Streife", "858320.svg"));
                laptopApps.Add(new LaptopApp("ServiceOverviewApp", "Service", "204316.svg"));
            }

            if (player.TeamId > 0 && player.Team != null && player.Team.Type == TeamType.Medic)
            {
                laptopApps.Add(new LaptopApp("ServiceOverviewApp", "Service", "204316.svg"));
            }

            laptopApps.Add(new LaptopApp("EmailApp", "Email", "email.png"));
            laptopApps.Add(new LaptopApp("ExportApp", "Export", "export.png"));

            await desktopApp.TriggerEvent(player, "responseComputerApps", JsonConvert.SerializeObject(laptopApps));
        }
    }
}
