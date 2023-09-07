using Backend.Models;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Native
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class NativeMenuModule : RXModule
    {
        public NativeMenuModule() : base("NativeMenu") { }

        //[HandleExceptions]
        [RemoteEvent("m")]
        public void OnNativeMenu(RXPlayer player, string id)
        {
            if (string.IsNullOrEmpty(id)) return;

            if (id != "NaN" && int.TryParse(id, out var selection) && player.Menu != null)
            {
                NativeMenu menu = player.Menu;

                if (menu.Items.Count < selection) return;

                NativeItem item = menu.Items[selection];
                if (item == null) return;

                NAPI.Task.Run(() => item.Action.Invoke(player));
            }
        }
    }
}
