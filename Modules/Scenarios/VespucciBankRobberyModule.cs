using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Faction;
using Backend.Modules.Inventory;
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
    class VespucciBankRobberyModule : RXModule
    {
        public VespucciBankRobberyModule() : base("VespucciRobbery") { }

        public static bool IsActive = false;
        public static int TimeLeft = 0;
        public static int RobberyTime = 60; // max zeit in SB
        public static RXTeam RobberTeam = null;
        public static string robname = "Vespucci";
        public static int CountInBreakTresor = 0;

        public static string SecureSystemIPL = "Bank_Vespucci";

        public static DateTime LastVespucciBank = DateTime.Now.AddHours(-2);

        public static Vector3 RobPosition = new Vector3(-1308.69, -812.482, 17.1483);
        public static int VESPUCCIBANK1 = 994;
        public static int VESPUCCIBANK2 = 995;
        public static int VESPUCCIBANK3 = 996;
        public static int VESPUCCIBANK4 = 997;
        public static int VESPUCCIBANK5 = 998;

        public override void LoadAsync()
        {
            IsActive = false;
            TimeLeft = 0;
            RobberyTime = Configuration.DevMode ? 3 : 20;
        }

        public void LoadContainerBankInv(RXContainerObj container)
        {

            Random rnd = new Random();
            container.Slots.Clear();
            container.AddItem(487, rnd.Next(38, 43));
        }

        public static bool CanVespucciBankRobbed()
        {
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (Configuration.DevMode) return true;

            // Check other Robs
            if (RobberyModule.Robberies.Where(r => r.Value.Type == RobType.Juwelier && RobberyModule.IsActive(r.Value.Id)).Count() > 0 || StaatsbankRobberyModule.IsActive)
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
            if (IsActive || RobberyModule.LastScenario.AddHours(2) > DateTime.Now || (LastVespucciBank.AddHours(2) > DateTime.Now && !Configuration.DevMode))
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

            // Messages
            TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState("An Alle Einheiten, ein Einbruch in der Vespucci wurde gemeldet!");
            TeamModule.Teams.Find(x => x.Id == dbPlayer.TeamId).SendNotification("Deine Fraktion raubt nun die Vespucci Bank aus!");
            TimeLeft = RobberyTime;
            IsActive = true;
            RobberTeam = dbPlayer.Team;

            RobberyModule.LastScenario = DateTime.Now;
            NAPI.Task.Run(() =>
            {
                NAPI.World.RequestIpl(SecureSystemIPL);
            });
            LastVespucciBank = DateTime.Now;
        }

        public void CloseRob()
        {
            var StaticContainer = ContainerModule.Containers.Find(x => x.Id == VESPUCCIBANK1);
            StaticContainer.Slots.Clear();
            StaticContainer = ContainerModule.Containers.Find(x => x.Id == VESPUCCIBANK2);
            StaticContainer.Slots.Clear();
            StaticContainer = ContainerModule.Containers.Find(x => x.Id == VESPUCCIBANK3);
            StaticContainer.Slots.Clear();
            StaticContainer = ContainerModule.Containers.Find(x => x.Id == VESPUCCIBANK4);
            StaticContainer.Slots.Clear();
            StaticContainer = ContainerModule.Containers.Find(x => x.Id == VESPUCCIBANK5);
            StaticContainer.Slots.Clear();

            IsActive = false;
            RobberTeam = null;
            TimeLeft = RobberyTime;
        }


        public void CancelRob()
        {
            TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState("An Alle Einheiten, der Einbruch auf die Vespucci Bank wurde erfolgreich verhindert!");
            TeamModule.Teams.Find(x => x.Id == RobberTeam.Id).SendNotification("Deine Fraktion ist beim Ausrauben der Vespucci Bank gescheitert!");


            IsActive = false;
            RobberTeam = null;
            TimeLeft = RobberyTime;


            NAPI.Task.Run(() =>
            {
                NAPI.World.RemoveIpl(SecureSystemIPL);
            });
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
        }
    }
}
