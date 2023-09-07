using Backend.Modules.ColShape;
using Backend.Modules.Inventory;
using Backend.MySql.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Backend.Models.RXContainer;

namespace Backend.Models
{
    public class RXColShape : ColShape
    {
        public bool PlantPlace { get; set; } = false;

        public bool IsInteractionColShape { get; set; } = false;

        public RXMessage Message { get; set; }

        public Action<RXPlayer> Action { get; set; } = player => { };

        public bool IsContainerColShape { get; set; } = false;
        public uint ContainerId { get; set; } = 0;
        public int ContainerType { get; set; } = 0;
        public RXContainerObj Container { get => ContainerModule.Containers.FirstOrDefault(x => x.Id == ContainerId); }
        public bool ContainerOpen { get; set; } = false;
        public string ContainerCustomName { get; set; } = "";
        public uint ContainerRestrictedTeam { get; set; } = 0;
        public uint ContainerRestrictedPlayer { get; set; } = 0;
        public uint ContainerRestrictedWorkstation { get; set; } = 0;

        public ColShapeKeyType ColShapeKeyType = ColShapeKeyType.E;

        public async Task<uint> GetDimensionAsync() => await NAPI.Task.RunReturnAsync(() => this.Dimension);



        public NetHandle ColShapeHandle { get; set; }
        public DbPlant PlantData { get; set; } = new DbPlant { Id = 0 };
        public GTANetworkAPI.Object PlantObj { get; set; } = null;

        public RXColShape(NetHandle handle) : base(handle)
        {
            ColShapeHandle = handle;
        }
    }
}
