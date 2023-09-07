using Backend.Models;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Time
{
    class TimeModule : RXModule
    {
        public TimeModule() : base("Time") { }


        public override void LoadAsync()
        {
            NAPI.Task.Run(() =>
            {
                var now = DateTime.Now;
                NAPI.World.SetTime(now.Hour, now.Minute, now.Second);
                NAPI.World.SetWeather(Weather.EXTRASUNNY);
            });
        }

        public override async Task OnMinute()
        {
            NAPI.Task.Run(() =>
            {
                var l_Time = DateTime.Now;
                var l_Hour = (uint)l_Time.Hour;
                NAPI.World.SetTime((int)l_Hour, l_Time.Minute, l_Time.Second);
                if (NAPI.World.GetWeather() != Weather.EXTRASUNNY)
                {
                    NAPI.World.SetWeather(Weather.EXTRASUNNY);
                }
            });
        }
    }
}
