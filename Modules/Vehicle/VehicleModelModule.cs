using Backend.Models;
using Backend.MySql;
using Backend.MySql.Models;
using Microsoft.EntityFrameworkCore;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Vehicle
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class VehicleModelModule : RXModule
    {
        public VehicleModelModule() : base("VehicleModel") { }

        public static List<RXVehicleModel> VehicleModels = new List<RXVehicleModel>();

        //[HandleExceptions]
        public override async void LoadAsync()
        {
            using var db = new RXContext();

            foreach (DbVehicleModel vehmodel in await db.VehicleModels.ToListAsync())
            {
                VehicleModels.Add(new RXVehicleModel { Id = vehmodel.Id, Name = vehmodel.Name, Classification = (VehicleClassificationTypes)vehmodel.Classification, Fuel = vehmodel.Fuel, FuelConsumption = vehmodel.FuelConsumption, Hash = vehmodel.Hash, InventorySize = vehmodel.InventorySize, InventoryWeight = vehmodel.InventoryWeight, MaxKMH = vehmodel.MaxKMH, Multiplier = vehmodel.Multiplier, Seats = vehmodel.Seats, Type = vehmodel.Type});

            }
        }
    }
}
