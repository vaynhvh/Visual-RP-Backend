using Backend.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Backend.Modules.Labs
{
    internal static class LabCoords
    {
        internal static readonly Vector3 PrisonLockerPutIn = new Vector3(1688.95, 2580.35, 45.9115);
        internal static readonly Vector3 PrisonLockerTakeOut = new Vector3(1847, 2585.89, 45.6721);

        internal static readonly Vector3 PaletoPDLockerPutInOut = new Vector3(-437.41, 5994.58, 31.7165);
        internal static readonly Vector3 SandyPDLockerPutInOut = new Vector3(1850.28, 3685.49, 34.2081);

        internal static readonly Vector3 LSPDPDLockerPutIn = new Vector3(473.097, -1007.48, 26.2733);
        internal static readonly Vector3 LSPDLockerPutOut = new Vector3(441.955, -984.301, 30.6895);

        internal static readonly Vector3 MethlaboratoryLaptopPosition = new Vector3(1001.99, -3194.96, -38.9932);
        internal static readonly Vector3 MethlaboratoryStartPosition = new Vector3(1010.97, -3196.77, -38.9931);
        internal static readonly Vector3 MethlaboratoryInvInputPosition = new Vector3(1005.8, -3200.18, -38.5193);
        internal static readonly Vector3 MethlaboratoryInvOutputPosition = new Vector3(1007.75, -3199.41, -38.9932);
        internal static readonly Vector3 MethlaboratoryInvUpgradePosition = new Vector3(1004.78, -3195.11, -38.9931); // Frisk point
        internal static readonly Vector3 MethlaboratoryInvFuelPosition = new Vector3(1011.98, -3200.07, -38.9931);
        internal static readonly Vector3 MethlaboratoryBatterieSwitch = new Vector3(1016.35, -3194.96, -38.9932);
        internal static readonly Vector3 MethlaboratoryEphePulver = new Vector3(1013.66, -3195.02, -38.9931);
        internal static readonly Vector3 MethlaboratoryAnalyzePosition = new Vector3(997.868, -3200.65, -38.9932);
        internal static readonly Vector3 MethlaboratoryCheckBoilerQuality = new Vector3(1006.23, -3195.03, -38.9931);
        internal static readonly Vector3 MethLaboratoryEntranceExitPosition = new Vector3(997.12976, -3200.6887, -36.393715);
        
        internal static readonly Vector3 WeaponlaboratoryInvInputPosition = new Vector3(839.79944, -3244.2993, -98.699104);
        internal static readonly Vector3 WeaponlaboratoryInvOutputPosition = new Vector3(838.10046, -3244.851, -98.699104);
        internal static readonly Vector3 WeaponlaboratoryInvFuelPosition = new Vector3(831.21185, -3239.3083, -98.699104);
        internal static readonly Vector3 WeaponlaboratoryComputerPosition = new Vector3(835.2482, -3244.702, -98.699104);
        internal static readonly Vector3 WeaponlaboratoryWeaponBuildMenuPosition = new Vector3(831.1384, -3243.351, -98.699104);
        internal static readonly Vector3 WeaponlaboratoryEntranceExitPosition = new Vector3(857.2223, -3249.6548, -98.351135);

        internal static readonly Vector3 CannabislaboratoryInvInputPosition = new Vector3(1064.95, -3189.18, -39.1612);
        internal static readonly Vector3 CannabislaboratoryInvOutputPosition = new Vector3(1039.24, -3205.17, -38.1665);
        internal static readonly Vector3 CannabislaboratoryInvFuelPosition = new Vector3(1060.57, -3184.85, -39.1647);
        internal static readonly Vector3 CannabislaboratoryComputerPosition = new Vector3(1060.12, -3182.04, -39.1648);
        internal static readonly Vector3 CannabislaboratoryBatterieSwitch = new Vector3(1034.65, -3205.57, -38.1766);
        internal static readonly Vector3 CannabislaboratoryCannabisPulver = new Vector3(1034.33, -3203.81, -38.1779);
        internal static readonly Vector3 CannabislaboratoryCheckBoilerQuality = new Vector3(1044.16, -3195.75, -38.1586);
    }
    class LabManager : RXModule
    {
        public LabManager() : base("Lab") { }


        public static int TimeToImpound = 90000;
        public static int TimeToFrisk = 30000;
        public static int TimeToAnalyze = 30000;
        public static int TimeToBreakDoor = 600000;
        public static int TimeToHack = 60000;

        public static int RepairPrice = 1000000;

        public static int HoursDisablingAfterHackAttack = 6;

        public override void LoadAsync()
        { 
            if (Configuration.DevMode)
            {
                TimeToImpound = 3000;
                TimeToFrisk = 3000;
                TimeToAnalyze = 3000;
                TimeToBreakDoor = 3000;
                TimeToHack = 3000;
            }
        }

        public static async Task LoadPlayerLabPoints(RXPlayer player)
        {
            if (player.TeamId == 0) return;

            var lab = WeaponLab.WeaponLabs.Find(x => x.TeamId == player.TeamId);
            if (lab != null)
            {
                List<RXColShape> colShapes = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllColShapes().Cast<RXColShape>().ToList());

                foreach (RXColShape colShape in colShapes)
                {
                    if (colShape != null && colShape.ContainerRestrictedPlayer == player.Id && colShape.ContainerId == player.LabInputContainerId)
                    {
                        await NAPI.Task.RunAsync(() => colShape.Delete());
                    }
                    if (colShape != null && colShape.ContainerRestrictedPlayer == player.Id && colShape.ContainerId == player.LabOutputContainerId)
                    {
                        await NAPI.Task.RunAsync(() => colShape.Delete());
                    }
                }


                await NAPI.Task.RunAsync(() =>
                {
                    var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(LabCoords.WeaponlaboratoryInvInputPosition, 1.4f, 1.4f, 0);

                    colShape.IsContainerColShape = true;
                    colShape.ContainerId = player.LabInputContainerId;
                    colShape.ContainerOpen = true;
                    colShape.ContainerRestrictedPlayer = player.Id;
                });

                await NAPI.Task.RunAsync(() =>
                {
                    var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(LabCoords.WeaponlaboratoryInvOutputPosition, 1.4f, 1.4f, 0);

                    colShape.IsContainerColShape = true;
                    colShape.ContainerId = player.LabOutputContainerId;
                    colShape.ContainerOpen = true;
                    colShape.ContainerRestrictedPlayer = player.Id;
                });
            }
            var methlab = MethLab.MethLabs.Find(x => x.TeamId == player.TeamId);
            if (methlab != null)
            {
                List<RXColShape> colShapes = await NAPI.Task.RunReturnAsync(() => NAPI.Pools.GetAllColShapes().Cast<RXColShape>().ToList());

                foreach (RXColShape colShape in colShapes)
                {
                    if (colShape != null && colShape.ContainerRestrictedPlayer == player.Id && colShape.ContainerId == player.LabInputContainerId)
                    {
                        await NAPI.Task.RunAsync(() => colShape.Delete());
                    }
                    if (colShape != null && colShape.ContainerRestrictedPlayer == player.Id && colShape.ContainerId == player.LabOutputContainerId)
                    {
                        await NAPI.Task.RunAsync(() => colShape.Delete());
                    }
                }


                await NAPI.Task.RunAsync(() =>
                {
                    var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(LabCoords.MethlaboratoryInvInputPosition, 1.4f, 1.4f, 0);

                    colShape.IsContainerColShape = true;
                    colShape.ContainerId = player.LabInputContainerId;
                    colShape.ContainerOpen = true;
                    colShape.ContainerRestrictedPlayer = player.Id;
                });

                await NAPI.Task.RunAsync(() =>
                {
                    var colShape = (RXColShape)NAPI.ColShape.CreateCylinderColShape(LabCoords.MethlaboratoryInvOutputPosition, 1.4f, 1.4f, 0);

                    colShape.IsContainerColShape = true;
                    colShape.ContainerId = player.LabOutputContainerId;
                    colShape.ContainerOpen = true;
                    colShape.ContainerRestrictedPlayer = player.Id;
                });
            }


       
        }

        public async Task<bool> IsLabOrImpoundVehicle(RXVehicle vehicle)
        {

            return await vehicle.GetModelAsync() == (uint)VehicleHash.Brickade || await vehicle.GetModelAsync() == (uint)VehicleHash.Burrito || await vehicle.GetModelAsync() == (uint)VehicleHash.Burrito2 || await vehicle.GetModelAsync() == (uint)VehicleHash.Burrito3 || await vehicle.GetModelAsync() == (uint)VehicleHash.Burrito4 || await vehicle.GetModelAsync() == (uint)VehicleHash.Burrito5 || await vehicle.GetModelAsync() == (uint)VehicleHash.Gburrito || await vehicle.GetModelAsync() == (uint)VehicleHash.Gburrito2 || await vehicle.GetModelAsync() == (uint)VehicleHash.Benson;

        }

        public async Task OpenLabMenu(RXPlayer player)
        {

        }
    }
}
