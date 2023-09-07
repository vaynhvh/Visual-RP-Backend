using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Tablet.Apps;
using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Backend.Modules.Admin.Tablet
{
    public class TabletApp
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "appName")]
        public string AppName { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "icon")]
        public string Icon { get; set; }

        public TabletApp(uint id, string appName, string name, string icon)
        {
            Id = id;
            AppName = appName;
            Name = name;
            Icon = icon;
        }
    }

    class TabletModule : RXModule
    {
        public TabletModule() : base("Tablet") { }

        [RemoteEvent]
        public async Task requestIpadApp(RXPlayer player)
        {
            if (!player.CanInteract() || !player.InAduty) return;

            var ipadDesktopApp = new RXWindow("IpadDesktopApp");

            List<TabletApp> tabletApps = new List<TabletApp>();

            tabletApps.Add(new TabletApp(1, "SupportOverviewApp", "Support", "204316.svg"));
            tabletApps.Add(new TabletApp(2, "SupportVehicleApp", "Fahrzeugsupport", "234788.svg"));

            await ipadDesktopApp.TriggerEvent(player, "responseIpadApps", JsonConvert.SerializeObject(tabletApps));
        }
    }
}
