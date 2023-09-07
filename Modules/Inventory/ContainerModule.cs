using Backend.Controllers;
using Backend.Models;
using Backend.MySql;
using Backend.Utils;
using Microsoft.EntityFrameworkCore;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Backend.Models.RXContainer;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Inventory
{
    public enum ContainerMoveTypes
    {
        SelfInventory = 1,
        ExternInventory = 2,
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class ContainerModule : RXModule
    {
        public ContainerModule() : base("Container") { }

        public static List<RXContainerObj> Containers = new List<RXContainerObj>();

        public static ContainerModule Instance = new ContainerModule();

        //[HandleExceptions]
        public async override void LoadAsync()
        {
            using var _db = new RXContext();

            RequireModule("ItemModel");

            if ((await _db.Containers.ToListAsync()).Count <= 0) return;

            TransferDBContextValues(await _db.Containers.ToListAsync(), con =>
            {
                using var db = new RXContext();

                List<RXItem> items = db.Items.Where(item => item.InventoryId == con.Id).ToList().ConvertAll(item => new RXItem
                {
                    Id = item.Id,
                    Amount = item.Amount,
                    InventoryId = item.InventoryId,
                    ItemModelId = item.ItemModelId,
                    Model = ItemModelModule.ItemModels.FirstOrDefault(model => model.Id == item.ItemModelId),
                    Slot = item.Slot
                });

                Containers.Add(new RXContainerObj
                {
                    Id = con.Id,
                    Name = con.Name,
                    MaxSlots = con.MaxSlots,
                    MaxWeight = con.MaxWeight,
                    Slots = items
                });
            });
        }

        //[HandleExceptions]
        public override async Task OnMinute()
        {
            using var db = new RXContext();

            List<RXContainerObj> copyContainers = new List<RXContainerObj>();

            if ((await db.Containers.ToListAsync()).Count <= 0) return;

            TransferDBContextValues(await db.Containers.ToListAsync(), con =>
            {
                using var _db = new RXContext();

                List<RXItem> items = _db.Items.Where(item => item.InventoryId == con.Id).ToList().ConvertAll(item => new RXItem
                {
                    Id = item.Id,
                    Amount = item.Amount,
                    InventoryId = item.InventoryId,
                    ItemModelId = item.ItemModelId,
                    Model = ItemModelModule.ItemModels.FirstOrDefault(model => model.Id == item.ItemModelId),
                    Slot = item.Slot
                });

                copyContainers.Add(new RXContainerObj
                {
                    Id = con.Id,
                    Name = con.Name,
                    MaxSlots = con.MaxSlots,
                    MaxWeight = con.MaxWeight,
                    Slots = items
                });
            });

            Containers = copyContainers;
        }

        //[HandleExceptions]
        public static async Task RefreshContainersAsync()
        {
            using var db = new RXContext();

            List<RXContainerObj> copyContainers = new List<RXContainerObj>();

            Instance.TransferDBContextValues(await db.Containers.ToListAsync(), con =>
            {
                using var _db = new RXContext();

                List<RXItem> items = _db.Items.Where(item => item.InventoryId == con.Id).ToList().ConvertAll(item => new RXItem
                {
                    Id = item.Id,
                    Amount = item.Amount,
                    InventoryId = item.InventoryId,
                    ItemModelId = item.ItemModelId,
                    Model = ItemModelModule.ItemModels.FirstOrDefault(model => model.Id == item.ItemModelId),
                    Slot = item.Slot
                });

                copyContainers.Add(new RXContainerObj
                {
                    Id = con.Id,
                    Name = con.Name,
                    MaxSlots = con.MaxSlots,
                    MaxWeight = con.MaxWeight,
                    Slots = items
                });
            });

            Containers = copyContainers;
        }

        //[HandleExceptions]
        public static async Task MoveItemToAnotherContainer(RXContainerObj sourceContainer, RXContainerObj externContainer, int sourceSlot, int destinationSlot, int amount)
        {
            try
            {
                RXItemModel itemModel = sourceContainer.Slots.FirstOrDefault(s => s.Slot == sourceSlot).Model;
                sourceContainer.RemoveItemSlotFirst(itemModel, sourceSlot, amount);
                externContainer.AddItem(itemModel, amount, destinationSlot);
            }
            catch (Exception ex)
            {
                RXLogger.Print("MoveItemToAnotherContainer: " + ex.Message);
            }
        }
    }
}
