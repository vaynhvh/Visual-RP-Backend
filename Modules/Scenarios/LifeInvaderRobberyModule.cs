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
    class LifeInvaderRobberyModule : RXModule
    {
        public LifeInvaderRobberyModule() : base("LifeInvaderRobbery") { }


        public static bool IsActive = false;
        public int TimeLeft = 0;
        public int RobberyTime = 60; // max zeit in SB
        public RXTeam RobberTeam = null;
        public string robname = "Liveinvader";
        public bool IsHacked = false;

        public int LIFEINVADERSERVER = 1003;

        public static string SecureSystemIPL = "";

        public DateTime LastVespucciBank = DateTime.Now.AddHours(-2);

        public Vector3 RobPosition = new Vector3(-1082.66, -245.444, 37.7633);


        public override void LoadAsync()
        {
            base.LoadAsync();
        }

        public void LoadContainerLifeInvader(RXContainerObj container)
        {
            container.Slots.Clear();
            container.AddItem(1101, 1);
        }

        public bool CanLifeinvaderRobbed()
        {
            // Timecheck +- 30 min restarts
            var hour = DateTime.Now.Hour;
            var min = DateTime.Now.Minute;

            if (Configuration.DevMode) return true;

            // Check other Robs
            if (RobberyModule.Robberies.Where(r => r.Value.Type == RobType.Juwelier && RobberyModule.IsActive(r.Value.Id)).Count() > 0 || StaatsbankRobberyModule.IsActive || VespucciBankRobberyModule.IsActive)
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


        public async Task StartRob(RXPlayer dbPlayer)
        {

            if (!dbPlayer.Team.IsGangster())
            {
                await dbPlayer.SendNotify("Diese Funktion ist nur für Badfraks!");
                return;
            }

            if (Configuration.DevMode != true)
            {
                // Timecheck +- 30 min restarts
                if (!CanLifeinvaderRobbed())
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

            // Set start datas
            TimeLeft = RobberyTime;
            IsActive = true;
            RobberTeam = dbPlayer.Team;
            IsHacked = false;

            // Messages
            TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState("An Alle Einheiten, ein Einbruch in das Lifeinvader-Büro wurde gemeldet!");
            TeamModule.Teams.Find(x => x.Id == dbPlayer.TeamId).SendNotification("Deine Fraktion raubt nun die Serverschränke des Lifeinvaders aus!");

            LastVespucciBank = DateTime.Now;
            RobberyModule.LastScenario = DateTime.Now;

            int time = 300000;
            if (Configuration.DevMode) time = 30000;

            await dbPlayer.SendNotify("Du schaltest nun die Sicherheitssysteme aus!");
            await dbPlayer.PlayAnimationAsync((int)(AnimationFlags.Loop | AnimationFlags.AllowPlayerControl), "anim@heists@prison_heistig1_P1_guard_checks_bus", "loop");
            dbPlayer.Freezed = true;
            await dbPlayer.disableAllPlayerActions(true);
            await dbPlayer.SendProgressbar(time);
            await Task.Delay(time);
            if (dbPlayer.IsCuffed || dbPlayer.IsTied || dbPlayer.Injured)
            {
                CancelRob();
                return;
            }
            dbPlayer.Freezed = false;
            await dbPlayer.disableAllPlayerActions(false);

            await dbPlayer.StopAnimationAsync();

            await dbPlayer.SendNotify("Du hast den Serverschrank erfolgreich aufgeschweißt!");

            IsHacked = true;

        }
        public void CancelRob()
        {
            TeamModule.Teams.Find(x => x.Id == 1).SendMessageToAllState("An Alle Einheiten, der Einbruch auf das Lifeinvader-Gebäude wurde erfolgreich verhindert!");
            TeamModule.Teams.Find(x => x.Id == RobberTeam.Id).SendNotification("Deine Fraktion ist beim Ausrauben des Lifeinvaders gescheitert!");


            IsActive = false;
            RobberTeam = null;
            TimeLeft = RobberyTime;
        }

        public void CloseRob()
        {
            var StaticContainer = ContainerModule.Containers.Find(x => x.Id == LIFEINVADERSERVER);
            StaticContainer.Slots.Clear();

            IsActive = false;
            this.RobberTeam = null;
            this.TimeLeft = RobberyTime;
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
