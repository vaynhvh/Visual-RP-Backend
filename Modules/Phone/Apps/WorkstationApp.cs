using System;
using Backend.Models;
using Backend.Utils;
using GTANetworkAPI;
using System.Threading.Tasks;
using Backend.Modules.Workstation;
using Backend.Modules.Inventory;

namespace Backend.Modules.Phone.Apps
{

    public class WorkstationInfo
    {

        public uint Id { get; set; }
        public string name { get; set; }
        public float weight { get; set; }
        public float maxweight { get; set; }
        public float oweight { get; set; }
        public float omaxweight { get; set; }
        public string inputname { get; set; }
        public string outputname { get; set; }
        public uint inputcount { get; set; }
        public uint outputcount { get; set; }


        class WorkstationApp : RXModule
        {
            public WorkstationApp() : base("WorkstationApp", new RXWindow("WorkstationApp")) { }

            [RemoteEvent]
            public async Task requestWorkstationInfo(RXPlayer player)
            {
                try
                {
                    if (player == null) return;

                    var workstation = WorkstationModule.Workstations.Find(x => x.Id == player.WorkstationId);
                    if (workstation == null) return;

                    var rxcontainer = ContainerModule.Containers.Find(x => x.Id == player.WorkstationInputContainerId);
                    var rxoutputcontainer = ContainerModule.Containers.Find(x => x.Id == player.WorkstationOutputContainerId);

                    var workstationinfo = new WorkstationInfo { Id = player.WorkstationId, name = workstation.Name, inputname = ItemModelModule.ItemModels.Find(x => x.Id == workstation.InputItemId).Name, inputcount = (uint)rxcontainer.GetItemAmount(workstation.InputItemId), maxweight = rxcontainer.MaxWeight, outputname = ItemModelModule.ItemModels.Find(x => x.Id == workstation.OutputItemId).Name, outputcount = (uint)rxoutputcontainer.GetItemAmount(workstation.OutputItemId), weight = rxcontainer.GetInventoryUsedSpace(), oweight = rxoutputcontainer.GetInventoryUsedSpace(), omaxweight = rxoutputcontainer.MaxWeight };


                    await this.Window.TriggerEvent(player, "responseWorkstationInfo", NAPI.Util.ToJson(workstationinfo));
                }
                catch (Exception ex)
                {
                    RXLogger.Print(ex.Message);
                    return;
                }
            }
        }
    }
}

