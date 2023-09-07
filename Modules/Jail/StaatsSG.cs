using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Faction;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Utilities.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Jail
{
    public enum SGJobs
    {
        WASHING = 1,
        WORKBENCH = 2,
    }

    class StaatsSG : RXModule
    {
        public StaatsSG() : base("StaatsSG") { }

        public static Vector3 klingelPosition = new Vector3(1846.572, 2604.7156, 45.578667);
        public static Vector3 sgBellPosition = new Vector3(1690.99, 2533.63, 61.3783);

        public static Vector3 JobMenuPosition = new Vector3(1761.11, 2574.75, 45.9177);

        public Dictionary<DbPlayer, SGJobs> SGJobPlayers = new Dictionary<DbPlayer, SGJobs>();

        public DateTime lastKlingelUsed = DateTime.Now;

        public List<uint> removeSGItemsOnNormalUnjail = new List<uint>();

        public static int SGWashingJobMax = 5;
        public static int SGWorkbenchJobMax = 5;

        public static Vector3 sgEmergencyPosition = new Vector3(1775.75, 2552.02, 45.565);
        public DateTime lastsgEmergencyUsed = DateTime.Now;

        public override async void LoadAsync()
        {
            var mcb = await NAPI.Entity.CreateMCB(klingelPosition, new Color(255, 140, 0), 0u, 1.4f, 1.4f, false, MarkerType.VerticalCylinder, true, 61, 1); //2.4f, 1.2f, true, MarkerType.VerticalCylinder);

            mcb.ColShape.Message = new RXMessage
            {
                Text = "Benutze E um die Klingel zu betätigen!",
                Color = "lightblue",
                Duration = 3500,
                Title = "Staatsgefängnis Klingel",
            };

            mcb.ColShape.Action = async player => await UseSGKlingel(player);
        }

        public async Task UseSGKlingel(RXPlayer player)
        {

            if (player.Rank.Permission > 90 && player.InAduty)
            {
                await player.SendNotify("Du hast die Klingel betätigt!");
                if (!player.Team.IsState())
                {
                    await player.SendNotify($"1337Allahuakbar$sgbell", 5000);
                }

                foreach (RXPlayer target in PlayerController.GetValidPlayers().ToList().Where(x => x.Team.IsState()))
                {
                    var tpos = await target.GetPositionAsync();
                    if (tpos.DistanceTo(sgBellPosition) < 200.0f)
                    {
                        await target.SendNotify($"1337Allahuakbar$sgbell", 5000);
                    }
                }

            } else if (lastKlingelUsed.AddMinutes(2) > DateTime.Now)
            {
                await player.SendNotify("Die Klingel kann nur jede 2 Minuten gedrückt werden!");
                return;
            }
            else
            {
                lastKlingelUsed = DateTime.Now;

                await player.SendNotify("Du hast die Klingel betätigt!");
                if (!player.Team.IsState())
                {
                    await player.SendNotify($"1337Allahuakbar$sgbell", 5000);
                }

                foreach (RXPlayer target in PlayerController.GetValidPlayers().ToList().Where(x => x.Team.IsState()))
                {
                    var tpos = await target.GetPositionAsync();
                    if (tpos.DistanceTo(sgBellPosition) < 200.0f)
                    {
                        await target.SendNotify($"1337Allahuakbar$sgbell", 5000);
                    }
                }
                return;
            }
        }

    }
}
