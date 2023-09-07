using GTANetworkAPI;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Models
{
    public class RXWindow
    {
        public string Name { get; set; }

        //[HandleExceptions]
        public async Task<bool> OpenWindow(RXPlayer player, object obj = null, bool nojson = false)
        {
            if (obj == null)
                await player.TriggerEventAsync("openWindow", this.Name);
            else if(!nojson)
                await player.TriggerEventAsync("openWindow", this.Name, JsonConvert.SerializeObject(obj));
            else if(nojson)
                await player.TriggerEventAsync("openWindow", this.Name, obj);


            if (this.Name == "TextInputBox")
                await player.TriggerEventAsync("componentReady", "TextInputBox");

            return await Task.FromResult(true);
        }

        //[HandleExceptions]
        public async Task<bool> OpenWindowStr(RXPlayer player, string obj)
        {
            await player.TriggerEventAsync("openWindow", this.Name, obj);

            if (this.Name == "TextInputBox")
                await player.TriggerEventAsync("componentReady", "TextInputBox");

            return await Task.FromResult(true);
        }

        //[HandleExceptions]
        public async Task<bool> CloseWindow(RXPlayer player)
        {
            await player.TriggerEventAsync("closeWindow", this.Name);

            return await Task.FromResult(true);
        }

        //[HandleExceptions]
        public async Task<bool> TriggerEvent(RXPlayer player, string arg_1 = "", string arg_2 = "", string arg_3 = "")
        {
            if (arg_2 == "" && arg_1 != "")
                await player.TriggerEventAsync("componentServerEvent", this.Name, arg_1, arg_2, arg_3);
            else if (arg_2 == "" && arg_1 != "")
                await player.TriggerEventAsync("componentServerEvent", this.Name, arg_1);
            else if (arg_1 == "" && arg_2 == "")
                await player.TriggerEventAsync("componentServerEvent", this.Name);
            else
                await player.TriggerEventAsync("componentServerEvent", this.Name, arg_1, arg_2);

            return await Task.FromResult(true);
        }


        public RXWindow(string name)
        {
            this.Name = name;
        }
    }
}
