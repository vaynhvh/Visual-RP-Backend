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
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Inventory
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class ItemModelModule : RXModule
    {
        public ItemModelModule() : base("ItemModel") { }

        public static List<RXItemModel> ItemModels = new List<RXItemModel>();

        //[HandleExceptions]
        public override async void LoadAsync()
        {
            using var db = new RXContext();

            foreach (var item in await db.ItemModels.ToListAsync())
            {
                ItemModels.Add(new RXItemModel(item.Id, item.Name, item.Weight, item.Illegal, item.Script, item.MaximumStackSize, item.RemoveOnUse, item.WeaponHash, item.ImagePath, item.ItemModel, 0));
            }

            RXLogger.Print("ItemModels: " + ItemModels.Count);
        }
    }
}
