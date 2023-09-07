using Backend.Controllers;
using Backend.Models;
using Backend.MySql;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    abstract class RXModule : Script
    {
        public string Name { get; set; }

        public RXWindow Window { get; set; }

        public bool Enabled { get; set; } = false;

        //[HandleExceptions]
        public virtual async void LoadAsync()
        {
        }

        //[HandleExceptions]
        public virtual async Task OnPlayerConnect(RXPlayer player)
        {
        }

        //[HandleExceptions]
        public virtual async Task OnPlayerDisconnect(RXPlayer player, DisconnectionType type, string reason)
        {
        }

        public virtual async Task OnPlayerEnterVehicle(RXPlayer player, RXVehicle vehicle, sbyte seat)
        {
        }
        public virtual async Task OnPlayerExitVehicle(RXPlayer player, RXVehicle vehicle)
        {
        }

        //[HandleExceptions]
        public virtual async Task OnColShape(RXColShape shape, RXPlayer player, bool state)
        {
        }

        //[HandleExceptions]
        public virtual async Task OnPlayerDeath(RXPlayer player, RXPlayer killer)
        {
        }
        public virtual async Task OnPlayerDamage(RXPlayer player, float healthLoss)
        {
        }
      
        public virtual async Task OnMinute()
        {
        }
        
        public virtual async Task OnSecond()
        {
        }

        //[HandleExceptions]
        public virtual async Task OnTwoSecond()
        {
        }

        //[HandleExceptions]
        public virtual async Task OnTenSecond()
        {
        }

        //[HandleExceptions]
        public virtual async Task OnFiveSecond()
        {
        }
        public virtual async Task OnThirtySecond()
        {
        }


        public virtual async Task OnFiveMinute()
        {
        }

        //[HandleExceptions]
        public virtual async Task OnTenMinute()
        {
        }

        public virtual async Task OnFifteenMinutes()
        {
        }
        public virtual async Task OnTwentyMinutes()
        {
        }

        //[HandleExceptions]
        public virtual async Task OnHour()
        {
        }

        /*public virtual async Task OnTimerTick(TimerType timerType)
        {
        }*/

        //[HandleExceptions]
        public virtual async Task PressedE(RXPlayer player)
        {
        }

        //[HandleExceptions]
        public virtual async Task PressedL(RXPlayer player)
        {
        }

        //[HandleExceptions]
        public virtual async Task PressedK(RXPlayer player)
        {
        }

        public virtual async Task PressedM(RXPlayer player)
        {
        }

        //[HandleExceptions]
        public void RequireModule(string moduleStr)
        {
            var module = ModuleController.Instance._modules.FirstOrDefault(m => m.Name == moduleStr);
            if (module == null)
            {
                RXLogger.Print("Module " + moduleStr + " not exists.");
                return;
            }

            if (!module.Enabled)
            {
                RXLogger.Print("Loading Module " + moduleStr + "..");

                module.LoadAsync();
                module.Enabled = true;
            }
        }

        //[HandleExceptions]
        public async void TransferDBContextValues<T>(IEnumerable<T> enumerable, Action<T> action)
        {
            for (int i = enumerable.Count() - 1; i >= 0; i--)
            {
                var obj = enumerable.ToList()[i];

                action(obj);
            }
        }

        public RXModule(string name, RXWindow window = null)
        {
            this.Name = name;
            this.Window = window;

            ModuleController.Instance.Register(this);
        }
    }
}
