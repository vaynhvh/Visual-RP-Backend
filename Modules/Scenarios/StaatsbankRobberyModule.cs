using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Faction;
using Backend.Modules.Inventory;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Backend.Models.RXContainer;

namespace Backend.Modules.Scenarios
{
    public class StaatsbankTunnel
    {
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public bool IsOutsideOpen { get; set; }
        public bool IsInsideOpen { get; set; }
        public RXTeam IsActiveForTeam { get; set; }

        public Vector3 Outside { get; set; }
        public Vector3 Inside { get; set; }

        public DateTime TunnelCreated { get; set; }
    }

    class StaatsbankRobberyModule : RXModule
    {
        public StaatsbankRobberyModule() : base("StaatsbankRobbery") { }


        public static bool IsActive = false;
        public static int TimeLeft = 0;
        public static int RobberyTime = 60; // max zeit in SB
        public static RXTeam RobberTeam = null;
        public static string robname = "Staatsbank";
        public static int CountInBreakTresor = 0;

        public static bool DoorHacked = false;

        public static int MainDoorId = 2;
        public static int SideDoorId = 3;

        public static int STAATSBANK1 = 985;
        public static int STAATSBANK2 = 986;
        public static int STAATSBANK3 = 987;
        public static int STAATSBANK4 = 988;
        public static int STAATSBANK5 = 989;
        public static int STAATSBANK6 = 990;
        public static int STAATSBANK7 = 991;
        public static int STAATSBANK8 = 992;

        public static DateTime LastStaatsbank = DateTime.Now.AddHours(-2);

        public static List<StaatsbankTunnel> StaatsbankTunnels = new List<StaatsbankTunnel>();

        public static Vector3 DrillingPoint = new Vector3(249.063, 219.389, 101.684);
        public static Vector3 HackingPoint = new Vector3(264.818, 219.881, 101.683);

        public static Vector3 RobPosition = new Vector3(263.758f, 214.239, 101.683);

        public override void LoadAsync()
        {
            StaatsbankTunnels = new List<StaatsbankTunnel>();

            StaatsbankTunnels.Add(new StaatsbankTunnel()
            {
                Position = new Vector3(-59.36, 184.86, 87.4008),
                Heading = 41.7401f,
                IsActiveForTeam = null,
                IsInsideOpen = false,
                IsOutsideOpen = false,
                Inside = null,
                Outside = null
            });

            StaatsbankTunnels.Add(new StaatsbankTunnel()
            {
                Position = new Vector3(127.651, -114.29, 54.8409),
                Heading = 348.561f,
                IsActiveForTeam = null,
                IsInsideOpen = false,
                IsOutsideOpen = false,
                Inside = null,
                Outside = null
            });

            StaatsbankTunnels.Add(new StaatsbankTunnel()
            {
                Position = new Vector3(1274.83, -1091.35, 38.7322),
                Heading = 126.774f,
                IsActiveForTeam = null,
                IsInsideOpen = false,
                IsOutsideOpen = false,
                Inside = null,
                Outside = null
            });

            StaatsbankTunnels.Add(new StaatsbankTunnel()
            {
                Position = new Vector3(1257.24, -1066.06, 38.7322),
                Heading = 131.823f,
                IsActiveForTeam = null,
                IsInsideOpen = false,
                IsOutsideOpen = false,
                Inside = null,
                Outside = null
            });

            IsActive = false;
            TimeLeft = 0;
            RobberyTime = Configuration.DevMode ? 3 : 20;
        }

        public void LoadContainerBankInv(RXContainerObj container)
        {
         
            Random rnd = new Random();
            container.Slots.Clear();
            container.AddItem(487, rnd.Next(38, 43));
            container.AddItem(880, 1);
        }

        public static bool CanStaatsbankRobbed()
        {
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (Configuration.DevMode) return true;

            // Check other Robs
            if (RobberyModule.Robberies.Where(r => r.Value.Type == RobType.Juwelier && RobberyModule.IsActive(r.Value.Id)).Count() > 0 || VespucciBankRobberyModule.IsActive || LifeInvaderRobberyModule.IsActive)
            {
                return false;
            }

            switch (hour)
            {
                case 7:
                case 15:
                case 23:
                    if (min >= 10)
                    {
                        return false;
                    }

                    break;
                case 8:
                case 16:
                case 0:
                    if (min < 15)
                    {
                        return false;
                    }

                    break;
            }


            return true;
        }


        public static async Task StartRob(RXPlayer dbPlayer)
        {

                if (!dbPlayer.Team.IsGangster())
                {
                    await dbPlayer.SendNotify("Diese Funktion ist nur für Badfraks!");
                    return;
                }

                if (Configuration.DevMode != true)
                {
                    // Timecheck +- 30 min restarts
                    if (!CanStaatsbankRobbed())
                    {
                    await dbPlayer.SendNotify("Das geht gerade nicht!");
                        return;
                    }
                }
                if (IsActive || RobberyModule.LastScenario.AddHours(2) > DateTime.Now || (LastStaatsbank.AddHours(2) > DateTime.Now && !Configuration.DevMode))
                {
                    await dbPlayer.SendNotify("Das geht gerade nicht!");
                    return;
                }

                if (TeamModule.Teams.Find(x => x.Id == 1).GetMemberCount() < 10 && !Configuration.DevMode)
                {
                    await dbPlayer.SendNotify("Der Staat ist aktuell zu schwach!");
                    return;
                }

                DateTime actualDate = DateTime.Now;
                    await dbPlayer.SendNotify("Sie versuchen nun den Tresor zu knacken!");

                    // Set start datas
                    TimeLeft = RobberyTime;
                    IsActive = true;
                    DoorHacked = false;
                    RobberTeam = dbPlayer.Team;

                    // Messages
                    TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState("An Alle Einheiten, ein Einbruch in der Staatsbank wurde gemeldet!");
                    TeamModule.Teams.Find(x => x.Id == dbPlayer.TeamId).SendNotification("Deine Fraktion raubt nun die Staatsbank aus!");

                    LastStaatsbank = DateTime.Now;
                    RobberyModule.LastScenario = DateTime.Now;
        }

        public void CloseRob()
        {
            var StaticContainer = ContainerModule.Containers.Find(x => x.Id == STAATSBANK1);
            StaticContainer.Slots.Clear();

            StaticContainer = ContainerModule.Containers.Find(x => x.Id == STAATSBANK2);
            StaticContainer.Slots.Clear();

            StaticContainer = ContainerModule.Containers.Find(x => x.Id == STAATSBANK3);
            StaticContainer.Slots.Clear();
            StaticContainer = ContainerModule.Containers.Find(x => x.Id == STAATSBANK4);
            StaticContainer.Slots.Clear();

            StaticContainer = ContainerModule.Containers.Find(x => x.Id == STAATSBANK5);
            StaticContainer.Slots.Clear();
            StaticContainer = ContainerModule.Containers.Find(x => x.Id == STAATSBANK6);
            StaticContainer.Slots.Clear();
            StaticContainer = ContainerModule.Containers.Find(x => x.Id == STAATSBANK7);
            StaticContainer.Slots.Clear();
            StaticContainer = ContainerModule.Containers.Find(x => x.Id == STAATSBANK8);
            StaticContainer.Slots.Clear();

            IsActive = false;
            RobberTeam = null;
            TimeLeft = RobberyTime;
            DoorHacked = false;
        }


        public void CancelRob()
        {
            TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState("An Alle Einheiten, der Einbruch auf die Staatsbank wurde erfolgreich verhindert!");
            TeamModule.Teams.Find(x => x.Id == RobberTeam.Id).SendNotification("Deine Fraktion ist beim Ausrauben der Staatsbank gescheitert!");


            IsActive = false;
            RobberTeam = null;
            TimeLeft = RobberyTime;
        }

        public override async Task OnMinute()
        {
            if (IsActive)
            {
                // Check if Teamplayer is in Reange
                if (RobberTeam == null || PlayerController.GetValidPlayers().Where(p => p != null && p.TeamId == RobberTeam.Id && !p.Injured && NAPI.Task.RunReturn(() => p.Position.DistanceTo(RobPosition)) < 15.0f).Count() <= 0)
                {
                    CancelRob();
                    return;
                }

                if (TimeLeft == 60)
                {
                    CloseRob();
                }
                TimeLeft--;
            }

            // Schließe Tunnel wieder... nach 15 min
            StaatsbankTunnel staatsbankTunnel = StaatsbankTunnels.Where(
                t => t.IsActiveForTeam != null && t.IsInsideOpen && t.IsOutsideOpen
                 && t.TunnelCreated != null && t.TunnelCreated.AddMinutes(15) < DateTime.Now).FirstOrDefault();

            if (staatsbankTunnel != null)
            {
                NAPI.Task.Run(() =>
                {
                    if (staatsbankTunnel.Outside != null)
                    {
                
                    }

                    if (staatsbankTunnel.Inside != null)
                    {
                     
                    }
                });
            }
        }


    }
}
