using Backend.Models;
using Backend.Utils.Extensions;
using GTANetworkAPI;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Controllers
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class VehicleController : Script
    {
        //[HandleExceptions]
        public static List<RXVehicle> GetValidVehicles()
        {
            return NAPI.Pools.GetAllVehicles().ConvertAll(v => (RXVehicle)v).ToList().Where(v => v != null && v.ModelData != null && v.TeamId == 0 && v.Id != 0).ToList();
        }

        //[HandleExceptions]
        public static List<RXVehicle> GetValidVehiclesIncludeTeam()
        {
            return NAPI.Pools.GetAllVehicles().ConvertAll(v => (RXVehicle)v).ToList().Where(v => v != null).ToList();
        }

        public static async Task<List<RXVehicle>> GetVehiclesInRange(Vector3 position, float range = 125f)
        {
            return await NAPI.Task.RunReturnAsync(() => GetValidVehicles().Where((player) => player.Position.DistanceTo(position) < range).ToList());
        }
        public static async Task<List<RXVehicle>> GetTeamVehiclesInRange(uint teamId, Vector3 position, float range = 125f)
        {
            return await NAPI.Task.RunReturnAsync(() => GetValidVehiclesIncludeTeam().Where((player) => player.TeamId == teamId && player.Position.DistanceTo(position) < range).ToList());
        }

        //[HandleExceptions]
        public static List<RXVehicle> GetVehicles()
        {
            return NAPI.Pools.GetAllVehicles().ConvertAll(v => (RXVehicle)v).ToList().Where(v => v != null).ToList();
        }

        //[HandleExceptions]
        public static RXVehicle FindVehicleById(object search)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;
            if (int.TryParse(searchString, out var vehicleId))
            {
                var vehicle = GetValidVehicles().FirstOrDefault(x => x.Id == vehicleId);
                if (vehicle == null) return null;

                return vehicle;
            }

            return null;
        }

        //[HandleExceptions]
        public static RXVehicle FindVehicleByPlate(object search, bool doublecheck = false)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;

            var vehicle = GetValidVehicles().FirstOrDefault(x => x.Plate == searchString && (doublecheck ? x.NumberPlate == searchString : true));
            if (vehicle == null) return null;

            return vehicle;
        }

        public static RXVehicle GetClosestTeamVehicle(Vector3 position, float range = 4.0f, uint TeamId = 0)
        {
            var dictionary = new Dictionary<float, RXVehicle>();


            foreach (var vehicle in GetValidVehiclesIncludeTeam())
            {
                if (vehicle == null || vehicle.TeamId != TeamId) continue;

                var _range = vehicle.Position.DistanceTo(position);

                if (_range <= range && !dictionary.ContainsKey(_range))
                {
                    dictionary.Add(_range, vehicle);
                }
            }

            var list = dictionary.Keys.ToList();
            list.Sort();


            return (dictionary.Count() > 0 && dictionary.ContainsKey(list[0])) ? dictionary[list[0]] : null;
        }

        //[HandleExceptions]
        public static RXVehicle GetClosestVehicle(Vector3 position, float range = 4.0f, UInt32 dimension = 0)
        {
            var dictionary = new Dictionary<float, RXVehicle>();
           

                foreach (var vehicle in GetValidVehiclesIncludeTeam())
                {
                    if (vehicle == null || vehicle.Dimension != dimension) continue;

                    var _range = vehicle.Position.DistanceTo(position);

                    if (_range <= range && !dictionary.ContainsKey(_range))
                    {
                        dictionary.Add(_range, vehicle);
                    }
                }

                var list = dictionary.Keys.ToList();
                list.Sort();


            return (dictionary.Count() > 0 && dictionary.ContainsKey(list[0])) ? dictionary[list[0]] : null;
        }
    }
}
