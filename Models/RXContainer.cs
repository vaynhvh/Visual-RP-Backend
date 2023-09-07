using Backend.Modules.Inventory;
using Backend.MySql;
using Backend.MySql.Models;
using Backend.Utils;
using GTANetworkAPI;
using Newtonsoft.Json;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using static System.Reflection.Metadata.BlobBuilder;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Models
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    public class RXContainer
    {
        public class RXContainerObj : DbContainer
        {
            public List<RXItem> Slots { get; set; }
            //public bool IsUsed { get; set; }
            //public bool Locked { get; set; }

            public RXContainerObj() { }

            //[HandleExceptions]
            public int GetInventoryUsedSpace()
            {
                int used = 0;

                foreach (RXItem items in this.Slots.ToList())
                {
                    if (items.ItemModelId == 0 || items.Amount == 0) continue;
                    used += items.Model.Weight * items.Amount;
                }
                return used;
            }

            //[HandleExceptions]
            public int GetInventoryFreeSpace()
            {
                return this.MaxWeight - this.GetInventoryUsedSpace();
            }

            //[HandleExceptions]
            public bool CanInventoryItemAdded(RXItemModel itemModel, int amount = 1)
            {
                if (itemModel == null) return false;
                if (amount <= 0) return false;

                int requiredSlots = 0;
                var weight = amount * itemModel.Weight;

                var similarStack = this.GetSlotOfSimilairSingleItemsToStack(itemModel.Id);
                var stackRequiredSlots = 99;

                if (similarStack == -1)
                { //Es gibt bisher keinen Stack mit diesem Itemtyp
                    stackRequiredSlots = amount / itemModel.MaximumStackSize < 1 ? 1 : (int)Math.Ceiling((decimal)amount / (decimal)itemModel.MaximumStackSize);
                }
                else
                { //Es wurde ein Stack mit dem Itemtyp gefunden
                    var spaceLeftOnSlot = itemModel.MaximumStackSize - this.GetAmountOfItemsOnSlot(similarStack);
                    stackRequiredSlots = amount <= spaceLeftOnSlot ? 0 : (int)Math.Ceiling((decimal)(amount - spaceLeftOnSlot) / (decimal)itemModel.MaximumStackSize);
                }

                requiredSlots += itemModel.MaximumStackSize > 1 ? stackRequiredSlots : 1;

                if (this.GetInventoryUsedSpace() + weight > this.MaxWeight)
                {
                    return false;
                }

                if (this.GetUsedSlots() + requiredSlots > this.MaxSlots)
                {
                    return false;
                }

                return true;
            }

            //[HandleExceptions]
            internal int GetSlotOfSimilairSingleItems(uint modelId)
            {
                RXItemModel itemModel = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == modelId);
                if (itemModel == null) return -1;

                return GetSlotOfSimilairItems(itemModel);
            }


            //[HandleExceptions]
            internal int GetSlotOfSimilairSingleItems(RXItemModel model)
            {
                if (model == null) return -1;

                return GetSlotOfSimilairItems(model);
            }

            //[HandleExceptions]
            internal int GetSlotOfSimilairItems(RXItemModel model)
            {
                foreach (RXItem kvp in this.Slots.ToList())
                {
                    if (kvp.ItemModelId == model.Id && kvp.Amount >= 1) return kvp.Slot;
                }
                return -1;
            }

            //[HandleExceptions]
            public int GetItemAmount(uint modelId)
            {
                RXItemModel itemModel = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == modelId);
                if (itemModel == null) return 0;
                return GetItemAmount(itemModel);
            }

            //[HandleExceptions]
            public int GetItemAmount(string itemName)
            {
                RXItemModel itemModel = ItemModelModule.ItemModels.FirstOrDefault(x => x.Name == itemName);
                if (itemModel == null) return 0;
                return GetItemAmount(itemModel);
            }

            public int GetMaxItemAddedAmount(uint itemModelId)
            {
                RXItemModel itemModel = ItemModelModule.ItemModels.FirstOrDefault(x => x.Id == itemModelId);

                return GetMaxItemAddedAmount(itemModel);
            }

            public int GetMaxItemAddedAmount(RXItemModel itemModel)
            {
                int slotAmount = 0;
                int weightAmount = 0;

                if (itemModel.Weight == 0 || itemModel.Weight == 1)
                    weightAmount = GetInventoryFreeSpace();
                else
                    weightAmount = GetInventoryFreeSpace() / itemModel.Weight;

                int freeSlots = this.Slots.ToList().Where(cs => cs == null || cs.Id == 0).Count();
                slotAmount = freeSlots * itemModel.MaximumStackSize;

                foreach (var kvp in this.Slots.ToList())
                {
                    if (kvp.Id == itemModel.Id)
                    {
                        slotAmount += itemModel.MaximumStackSize - kvp.Amount;
                    }
                }

                return slotAmount <= weightAmount ? slotAmount : weightAmount;
            }

            //[HandleExceptions]
            public int GetItemAmount(RXItemModel model)
            {
                int amount = 0;

                if (model == null)
                {
                    return 0;
                }

                try
                {
                    foreach (RXItem item in this.Slots.ToList())
                    {
                        if (item.Model == null) continue;
                        RXItemModel l_Model = item.Model;
                        if (l_Model != model) continue;
                        amount += item.Amount;
                    }
                }
                catch (Exception e)
                {
                    RXLogger.Print(e.ToString());
                    return 0;
                }

                return amount;
            }

            //[HandleExceptions]
            public int GetUsedSlots()
            {
                int usedSlots = 0;

                foreach (RXItem items in this.Slots.ToList())
                {
                    if (items.Amount != 0) usedSlots += 1;
                }
                return usedSlots;
            }

            //[HandleExceptions]
            public void RemoveFromSlot(int slot, int amount = 1)
            {
                RXItem l_Item = this.Slots.FirstOrDefault(s => s.Slot == slot);
                l_Item.Amount -= amount;

                RXItem rXItem = new RXItem
                {
                    Id = l_Item.Id,
                    Slot = slot,
                    Amount = l_Item.Amount,
                    InventoryId = l_Item.InventoryId,
                    ItemModelId = l_Item.ItemModelId,
                    Model = l_Item.Model,
                };

                this.Slots.Remove(this.Slots.FirstOrDefault(s => s.Slot == slot));

                if (l_Item.Amount > 0) this.Slots.Add(rXItem);

                using var db = new RXContext();

                DbItem dbItem = db.Items.FirstOrDefault(s => s.Slot == slot && s.InventoryId == this.Id);
                if (dbItem == null) return;

                var item = this.Slots.FirstOrDefault(s => s.Slot == slot);

                if (item == null || item.Amount < 1)
                    db.Items.Remove(dbItem);
                else
                {
                    dbItem.Amount = item.Amount;
                    dbItem.Slot = item.Slot;
                    dbItem.ItemModelId = item.ItemModelId;
                }

                db.SaveChanges();
            }

            //[HandleExceptions]
            public RXItem GetItemOnSlot(int slot)
            {
                if (slot < 0 || this.Slots.FirstOrDefault(s => s.Slot == slot) == null) return null;
                return this.Slots.FirstOrDefault(s => s.Slot == slot);
            }

            //[HandleExceptions]
            public RXItemModel GetModelOnSlot(int slot)
            {
                if (slot < 0 || this.Slots.FirstOrDefault(s => s.Slot == slot) == null) return null;
                return this.Slots.FirstOrDefault(s => s.Slot == slot)?.Model;
            }

            //[HandleExceptions]
            public bool AddItem(RXItemModel itemModel, int amount = 1, int slot = -1, bool disablesave = false)
            {
                if (itemModel == null) return false;

                RXLogger.Debug("Fortnite Ballz2");

                return AddContainerItem(itemModel, amount, slot, disablesave);
            }

            //[HandleExceptions]
            internal int GetSlotOfSimilairItemsToStackAmount(RXItemModel model, int amount)
            {
                foreach (RXItem item in this.Slots)
                {
                    if (item.ItemModelId == model.Id && item.Amount + amount <= item.Model.MaximumStackSize) return item.Slot;
                }
                return -1;
            }

            //[HandleExceptions]
            internal int GetAmountOfItemsOnSlot(int slot)
            {
                if (slot == -1) return -1;
                if (this.Slots.FirstOrDefault(x => x.Slot == slot) == null) return 0;

                return this.Slots.FirstOrDefault(x => x.Slot == slot).Amount;
            }

            //[HandleExceptions]
            internal int GetSlotOfSimilairSingleItemsToStack(uint modelId)
            {
                RXItemModel itemModel = ItemModelModule.ItemModels.FirstOrDefault(s => s.Id == modelId);
                if (itemModel == null) return -1;

                return GetSlotOfSimilairItemsToStackAmount(itemModel, 1);
            }

            //[HandleExceptions]
            public int GetNextFreeSlot(List<int> alreadyusedslots = null)
            {
                if (alreadyusedslots == null) alreadyusedslots = new List<int>();
                for (int i = 0; i < this.MaxSlots; i++)
                {
                    if (alreadyusedslots.Contains(i)) continue;

                    var item = this.Slots.FirstOrDefault(s => s.Slot == i);
                    if (item == null || item.ItemModelId == 0) return i;
                }
                return -1;
            }

            //[HandleExceptions]
            private void SetSlotClear(int slot)
            {
                this.Slots.Remove(this.Slots.FirstOrDefault(s => s.Slot == slot));
            }

            //[HandleExceptions]
            public void RemoveItemSlotFirst(RXItemModel model, int slot, int amount = 1)
            {
                if (model == null) return;
                if (amount <= 0) return;

                if (this.Slots.FirstOrDefault(s => s.Slot == slot) != null && this.Slots.FirstOrDefault(s => s.Slot == slot).Model == model)
                {
                    var itemModel = this.Slots.FirstOrDefault(s => s.Slot == slot);

                    if (itemModel.Amount <= amount)
                    {
                        amount -= itemModel.Amount;
                        this.SetSlotClear(slot);
                        if (amount > 0) RemoveItem(model, amount);
                    }
                    else
                    {
                        itemModel.Amount -= amount;
                    }
                }

                using var db = new RXContext();

                DbItem dbItem = db.Items.FirstOrDefault(s => s.Slot == slot && s.InventoryId == this.Id);
                if (dbItem == null) return;

                var item = this.Slots.FirstOrDefault(s => s.Slot == slot);

                if (item == null || item.Amount < 1)
                    db.Items.Remove(dbItem);
                else
                {
                    dbItem.Amount = item.Amount;
                    dbItem.Slot = item.Slot;
                    dbItem.ItemModelId = item.ItemModelId;
                }

                db.SaveChanges();

                RXLogger.Debug("Fortnite Ballz234232");

                return;
            }

            //[HandleExceptions]
            public bool AddItem(uint itemId, int amount = 1, int slot = -1, bool disablesave = false)
            {
                RXItemModel itemModel = ItemModelModule.ItemModels.FirstOrDefault(s => s.Id == itemId);
                if (itemModel == null) return false;

                RXLogger.Debug("Fortnite !!!Ballz");

                return AddContainerItem(itemModel, amount, slot, disablesave);
            }

            //[HandleExceptions]
            public void RemoveItem(uint itemId, int amount = 1, bool disablesave = false)
            {
                List<int> affectedSlots = new List<int>();
                if (amount <= 0 || itemId == 0) return;

                RXItemModel model = ItemModelModule.ItemModels.FirstOrDefault(s => s.Id == itemId);

                RemoveItem(model, amount, disablesave);
                return;
            }

            //[HandleExceptions]
            public void RemoveItem(RXItemModel model, int amount = 1, bool disablesave = false)
            {
                if (amount <= 0 || model == null) return;

                int tmpAmount = amount;

                List<int> touchedSlots = new List<int>();

                foreach (RXItem item in this.Slots.ToList())
                {
                    if (item != null && item.ItemModelId == model.Id)
                    {
                        if (item.Amount <= tmpAmount)
                        {
                            tmpAmount -= item.Amount;
                            this.SetSlotClear(item.Slot);
                            if (!touchedSlots.Contains(item.Slot)) touchedSlots.Add(item.Slot);
                        }
                        else if (tmpAmount > 0)
                        {
                            item.Amount -= tmpAmount;
                            tmpAmount = 0;
                            if (!touchedSlots.Contains(item.Slot)) touchedSlots.Add(item.Slot);
                            break;
                        }
                        else break;
                    }
                }

                if (!disablesave)
                {
                    using var db = new RXContext();

                    foreach (int l_Slot in touchedSlots)
                    {
                        DbItem dbItem = db.Items.FirstOrDefault(item => item.Slot == l_Slot && item.InventoryId == this.Id);
                        if (dbItem == null) continue;

                        RXItem rxItem = this.Slots.FirstOrDefault(s => s.Slot == l_Slot);

                        if (rxItem == null)
                            db.Items.Remove(dbItem);
                        else
                        {
                            dbItem.Amount = rxItem.Amount;
                            dbItem.Slot = rxItem.Slot;
                            dbItem.ItemModelId = rxItem.ItemModelId;
                        }

                        db.SaveChanges();
                    }
                }
            }

            public RXItem GetAttachmentOnlyItem()
            {
                foreach (RXItem kvp in this.Slots)
                {
                    if (kvp.Model != null && kvp.Model.AttachmentOnlyId > 0)
                    {
                        return kvp;
                    }
                }
                return null;
            }

            //[HandleExceptions]
            private bool AddContainerItem(RXItemModel model, int amount, int slot = -1, bool disablesave = false)
            {
                if (model == null) return false;
                if (amount == 0) return false;

                List<int> touchedSlots = new List<int>();

                int leftamount = amount;

                try
                {
                    while (leftamount > 0)
                    {
                        if (slot == -1)
                        {
                            slot = this.GetSlotOfSimilairSingleItemsToStack(model.Id);

                            if (slot >= 0 && this.Slots.FirstOrDefault(s => s.Slot == slot).ItemModelId == model.Id)
                            {
                                var itemInSlot = this.Slots.FirstOrDefault(s => s.Slot == slot);
                                RXLogger.Debug("Fortnite Ballzzzzzzz");
                                if (itemInSlot.Amount >= itemInSlot.Model.MaximumStackSize)
                                {
                                    AddContainerItem(model, leftamount);
                                    RXLogger.Debug("!!!!!!!!!1Fortnite Ballz");
                                    break;
                                }
                                else
                                {
                                    int amountCanAdded = itemInSlot.Model.MaximumStackSize - itemInSlot.Amount;

                                    if (amountCanAdded >= leftamount)
                                    {
                                        itemInSlot.Amount += leftamount;
                                        leftamount = 0;
                                        if (!touchedSlots.Contains(slot)) touchedSlots.Add(slot);
                                        RXLogger.Debug("Fortnite Nigga");
                                        break;
                                    }
                                    else
                                    {
                                        RXLogger.Debug("Fortnite Orga");
                                        itemInSlot.Amount += amountCanAdded;
                                        leftamount -= amountCanAdded;
                                    }
                                }
                            }
                            else
                            {
                                slot = this.GetNextFreeSlot();
                                RXLogger.Debug("Fortnite Eierlecken");
                                if (slot == -1) return false;

                                if (leftamount > model.MaximumStackSize)
                                {
                                    this.Slots.Remove(this.Slots.FirstOrDefault(s => s.Slot == slot));

                                    this.Slots.Add(new RXItem
                                    {
                                        Id = model.Id + 1,
                                        Slot = slot,
                                        Amount = model.MaximumStackSize,
                                        InventoryId = this.Id,
                                        ItemModelId = model.Id,
                                        Model = model
                                    });

                                    RXLogger.Debug("Fortnite Verfaulte Eier");

                                    leftamount -= model.MaximumStackSize;
                                }
                                else
                                {
                                    this.Slots.Remove(this.Slots.FirstOrDefault(s => s.Slot == slot));

                                    this.Slots.Add(new RXItem
                                    {
                                        Id = model.Id + 1,
                                        Slot = slot,
                                        Amount = leftamount,
                                        InventoryId = this.Id,
                                        ItemModelId = model.Id,
                                        Model = model
                                    });

                                    RXLogger.Debug("Fortnite Open");

                                    leftamount = 0;
                                }
                            }
                        }
                        else if (slot >= 0)
                        {
                            if (this.Slots.FirstOrDefault(s => s.Slot == slot) != null && this.Slots.FirstOrDefault(s => s.Slot == slot).Id != 0)
                            {
                                var item = this.Slots.FirstOrDefault(s => s.Slot == slot);

                                if (item.Amount >= item.Model.MaximumStackSize || item.Model.Id != model.Id)
                                {
                                    RXLogger.Debug("Fortnite BITTE NICHT BUGGEN");
                                    AddContainerItem(model, leftamount);
                                    break;
                                }
                                else
                                {
                                    int amountCanAdded = item.Model.MaximumStackSize - item.Amount;

                                    if (amountCanAdded >= leftamount)
                                    {
                                        item.Amount += leftamount;
                                        leftamount = 0;
                                        if (!touchedSlots.Contains(slot)) touchedSlots.Add(slot);
                                        break;
                                    }
                                }
                            }
                            else
                            {
                                if (leftamount > model.MaximumStackSize)
                                {
                                    this.Slots.Remove(this.Slots.FirstOrDefault(s => s.Slot == slot));
                                    RXLogger.Debug("Fortnite Ballz");

                                    this.Slots.Add(new RXItem
                                    {
                                        Id = model.Id + 1,
                                        Slot = slot,
                                        Amount = model.MaximumStackSize,
                                        InventoryId = this.Id,
                                        ItemModelId = model.Id,
                                        Model = model
                                    });

                                    leftamount -= model.MaximumStackSize;
                                }
                                else
                                {
                                    this.Slots.Remove(this.Slots.FirstOrDefault(s => s.Slot == slot));

                                    this.Slots.Add(new RXItem
                                    {
                                        Id = model.Id + 1,
                                        Slot = slot,
                                        Amount = leftamount,
                                        InventoryId = this.Id,
                                        ItemModelId = model.Id,
                                        Model = model
                                    });

                                    leftamount = 0;
                                }
                            }
                        }

                        if (!touchedSlots.Contains(slot)) touchedSlots.Add(slot);
                    }
                }
                catch (Exception ex)
                {
                    RXLogger.Print(ex.ToString());
                }

                if (!disablesave)
                {
                    using var db = new RXContext();

                    foreach (int l_Slot in touchedSlots)
                    {
                        RXItem rxItem = this.Slots.FirstOrDefault(s => s.Slot == l_Slot);
                        if (rxItem == null) continue;

                        DbItem dbItem = db.Items.FirstOrDefault(item => item.Slot == l_Slot && item.InventoryId == this.Id);
                        if (dbItem == null)
                        {
                            db.Items.Add(new DbItem
                            {
                                // Id = rxItem.Id,
                                Amount = rxItem.Amount,
                                InventoryId = this.Id,
                                ItemModelId = rxItem.ItemModelId,
                                Slot = l_Slot
                            });
                        }
                        else
                        {
                            dbItem.Amount = rxItem.Amount;
                            dbItem.Slot = l_Slot;
                            dbItem.ItemModelId = rxItem.ItemModelId;
                        }

                        db.SaveChanges();
                    }
                }

                return true;
            }

            //[HandleExceptions]
            public RXClientContainer ConvertForClient(uint sendId, string optionalname, int money = 0, int blackmoney = 0, int type = 0)
            {
                RXClientContainer clientContainerObject = new RXClientContainer(this.MaxWeight, this.MaxSlots);

                clientContainerObject.Slots = new List<RXClientContainerSlot>();
                clientContainerObject.t = type;
                if (this.Slots == null) return null;

                try
                {
                    /*if (clientContainerObject.MaxSlots > this.Slots.Count)
                    {
                        for (int i = 0; i < clientContainerObject.MaxSlots; i++)
                        {
                            if (this.Slots.FirstOrDefault(s => s.Slot == i) == null)
                            {
                                this.Slots.Add(new RXItem
                                {
                                    Id = (uint)new Random().Next(1000),
                                    InventoryId = 0,
                                    Amount = 0,
                                    Slot = i,
                                    ItemModelId = 0,
                                    Model = null
                                });
                            }
                        }
                    }*/
                    int iss = 0;

                    for (iss = 0; iss < this.MaxSlots; iss++)
                    {

                        clientContainerObject.Slots.Add(new RXClientContainerSlot((uint)iss, iss, new RXItemModel(0, ""), 0));

                    }

                    foreach (RXItem item in this.Slots.ToList())
                    {
                        if (item != null && item.Amount > 0)
                        {
                            if (item == null) continue;
                            if (item.Model == null) continue;
                            if (item.Id == null) continue;
                            if (item.InventoryId == null) continue;
                            if (item.ItemModelId == null) continue;

                            try
                            {
                                clientContainerObject.Slots[item.Slot] = new RXClientContainerSlot((uint)item.Slot, item.Slot, item.Model, item.Amount);
                            }
                            catch (Exception e)
                            {
                                RXLogger.Print("MAXIMIZE: " + e.Message);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    RXLogger.Print(e.ToString());
                }

                clientContainerObject.Id = sendId;
                clientContainerObject.Name = optionalname;
                clientContainerObject.Money = money;
                clientContainerObject.Blackmoney = blackmoney;
                clientContainerObject.Slots = clientContainerObject.Slots.Where(x => x != null).ToList();

                return clientContainerObject;
            }

            public async void ShowFriskInventory(RXPlayer player, RXPlayer target, string optionalname = " ", int money = 0)
            {
                player.IsInvDisabled = true;

                List<RXClientContainer> containerList = new List<RXClientContainer>();
                containerList.Add(this.ConvertForClient(1, "Inventar von " + optionalname?.Replace('_', ' '), money));
                await new RXWindow("Inventory").OpenWindow(player, new { inventory = containerList });
            }
        }

        public class RXClientContainer
        {
            [JsonProperty(PropertyName = "id")]
            public uint Id { get; set; }
            [JsonProperty(PropertyName = "i")]
            public int I
            {
                get
                {
                    return (int)Id;
                }
            }
            public int MaxWeight { get; set; }

            [JsonProperty(PropertyName = "t")]
            public int t { get; set; } = 0;

            public int MaxSlots { get; set; }

            [JsonProperty(PropertyName = "name")]
            public string Name { get; set; }

            [JsonProperty(PropertyName = "m")]
            public int Money { get; set; }

            [JsonProperty(PropertyName = "w")]
            public bool weapon { get; set; } = false;
            public int Blackmoney { get; set; }

            [JsonProperty(PropertyName = "s")]
            public List<RXClientContainerSlot> Slots { get; set; }

            public RXClientContainer(int maxWeight, int maxSlots)
            {
                MaxWeight = maxWeight;
                MaxSlots = maxSlots;
                Slots = new List<RXClientContainerSlot>();
            }

        }
        public class RXClientContainerSlot
        {
            [JsonProperty(PropertyName = "i")]
            public int Slot { get; set; }

            [JsonProperty(PropertyName = "itemId")]
            public uint itemId { get; set; }

            [JsonProperty(PropertyName = "amount")]
            public int Amount { get; set; }

            [JsonProperty(PropertyName = "weight")]
            public int Weight { get; set; }
            [JsonProperty(PropertyName = "name")] 
            public string Name { get; set; }

            [JsonProperty(PropertyName = "image")]
            public string ImagePath { get; set; }

            [JsonProperty(PropertyName = "customDataText")]
            public string customDataText { get; set; } = "";

            [JsonProperty(PropertyName = "customData")]
            public string customData { get; set; } = "";

            [JsonProperty(PropertyName = "maxStack")]
            public uint maxStack { get; set; } = 100;

            [JsonProperty(PropertyName = "id")]
            public uint Id { get; set; }

            public RXClientContainerSlot(uint id, int slot, RXItemModel model, int amount)
            {
                Id = id;
                Slot = slot;
                Name = model.Name;
                Weight = model.Weight;
                ImagePath = model.ImagePath;
                Amount = amount;
                itemId = model.Id;
            }
        }
    }
}
