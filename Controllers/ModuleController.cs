using Backend.Models;
using Backend.Modules;
using Backend.MySql;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Controllers
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class ModuleController : Script
    {
        public readonly List<RXModule> _modules;

        public static ModuleController Instance { get; } = new ModuleController();

        public ModuleController()
        {
            _modules = new List<RXModule>();
        }

        //[HandleExceptions]
        public static async void LoadAll(RXContext _context)
        {
            await Instance._modules.forEachAlternativeAsync(async module =>
            {
                if (module.Enabled) return;

                RXLogger.Print("Loading Module " + module.Name + "..");

                module.LoadAsync();
                module.Enabled = true;
            });
        }

        //[HandleExceptions]
        public static void StartTimers()
        {
            {
                //1 Sekunden
                Timer timer = new Timer(1000);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnSecond();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
            {
                //2 Sekunden
                Timer timer = new Timer(2000);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnTwoSecond();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
            {
                //10 Sekunden
                Timer timer = new Timer(10000);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnTenSecond();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
            {
                //5 Sekunden
                Timer timer = new Timer(5000);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnFiveSecond();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
            {
                //5 Sekunden
                Timer timer = new Timer(30000);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnThirtySecond();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
            {
                //10 Minuten
                Timer timer = new Timer(60000);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnMinute();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
            {
                //10 Minuten
                Timer timer = new Timer(60000 * 10);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnTenMinute();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
            {
                //15 Minuten
                Timer timer = new Timer(60000 * 15);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnFuenfzehnMinute();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
            {
                //5 Minuten
                Timer timer = new Timer(60000 * 5);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnFiveMinute();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
            {
                //20 Minuten
                Timer timer = new Timer(60000 * 20);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnTwentyMinute();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
            {
                //1 Stunde
                Timer timer = new Timer(60000 * 60);

                timer.Elapsed += async (sender, e) =>
                {
                    await OnHour();
                };

                timer.AutoReset = true;
                timer.Enabled = true;
            }
        }

        //[HandleExceptions]
        public void Register(RXModule module)
        {
            Instance._modules.Add(module);
        }

        //[HandleExceptions]
        [ServerEvent(Event.PlayerConnected)]
        public async Task OnPlayerConnect(RXPlayer player)
        {
            RXLogger.Print("Player connected: " + await player.GetNameAsync() + " - " + await player.GetAddressAsync() + " | " + await player.GetSocialNameAsync() + " | " + await NAPI.Task.RunReturnAsync(() => player.SocialClubId));

            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnPlayerConnect(player);
                }
            }
        }

        [ServerEvent(Event.PlayerEnterVehicle)]
        public async Task OnPlayerEnterVehicle(RXPlayer player, RXVehicle vehicle, sbyte seat)
        {

            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnPlayerEnterVehicle(player, vehicle, seat);
                }
            }
        }
        [ServerEvent(Event.PlayerExitVehicle)]
        public async Task OnPlayerExitVehicle(RXPlayer player, RXVehicle vehicle)
        {

            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnPlayerExitVehicle(player, vehicle);
                }
            }
        }

        //[HandleExceptions]
        public static async Task getClientDamage(RXPlayer player, float healthLoss)
        {
            RXLogger.Print("Player damage: " + await player.GetNameAsync() + " - " + await player.GetAddressAsync() + " | Health-Loss: " + healthLoss);

            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnPlayerDamage(player, healthLoss);
                }
            }
        }

        //[HandleExceptions]
        [ServerEvent(Event.PlayerDisconnected)]
        public void OnPlayerDisconnect(RXPlayer player, DisconnectionType type, string reason)
        {
            lock (player)
            {
                NAPI.Task.Run(async () =>
                {
                    RXLogger.Print("Player disconnected: " + player.Name + " - " + player.Address + " | Type: " + type.ToString() + " | Reason: " + reason);

                    for (int i = Instance._modules.Count - 1; i >= 0; i--)
                    {
                        RXModule module = Instance._modules[i];

                        if (module.Enabled)
                        {
                            await module.OnPlayerDisconnect(player, type, reason);
                        }
                    }
                });
            }
        }

        //[HandleExceptions]
        [ServerEvent(Event.PlayerEnterColshape)]
        public async Task OnColShapeEnter(RXColShape shape, RXPlayer entity)
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnColShape(shape, entity, true);
                }
            }
        }

        //[HandleExceptions]
        [ServerEvent(Event.PlayerExitColshape)]
        public async Task OnColShapeExit(RXColShape shape, RXPlayer entity)
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnColShape(shape, entity, false);
                }
            }
        }

        //[HandleExceptions]
        [ServerEvent(Event.PlayerDeath)]
        public async Task OnPlayerDeath(RXPlayer player, RXPlayer killer, uint reason)
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnPlayerDeath(player, killer);
                }
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task Pressed_E(RXPlayer player)
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.PressedE(player);
                }
            }
        }

        [RemoteEvent]
        public async Task Pressed_M(RXPlayer player)
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.PressedM(player);
                }
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task Pressed_L(RXPlayer player)
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.PressedL(player);
                }
            }
        }

        //[HandleExceptions]
        [RemoteEvent]
        public async Task Pressed_K(RXPlayer player)
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.PressedK(player);
                }
            }
        }

        public static async Task OnSecond()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnSecond();
                }
            }
        }

        //[HandleExceptions]
        public static async Task OnTwoSecond()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnTwoSecond();
                }
            }
        }

        //[HandleExceptions]
        public static async Task OnTenSecond()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnTenSecond();
                }
            }
        }
        public static async Task OnFuenfzehnMinute()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnFifteenMinutes();
                }
            }
        }
        public static async Task OnTwentyMinute()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnTwentyMinutes();
                }
            }
        }
        public static async Task OnFiveMinute()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnFiveMinute();
                }
            }
        }
        //[HandleExceptions]
        public static async Task OnFiveSecond()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnFiveSecond();
                }
            }
        }

        public static async Task OnThirtySecond()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnThirtySecond();
                }
            }
        }

        public static async Task OnMinute()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnMinute();
                }
            }
        }

        //[HandleExceptions]
        public static async Task OnTenMinute()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnTenMinute();
                }
            }
        }

        //[HandleExceptions]
        public static async Task OnHour()
        {
            for (int i = Instance._modules.Count - 1; i >= 0; i--)
            {
                RXModule module = Instance._modules[i];

                if (module.Enabled)
                {
                    await module.OnHour();
                }
            }
        }
    }
}
