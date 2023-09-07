using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Minijobs.Mower;
using Backend.MySql.Models;
using Backend.Utils;
using Backend.Utils.Extensions;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Minijobs
{
    public static class MinijobHandler
    {
        public static List<RXVehicle> JobVehicles = new List<RXVehicle>();

        public static double GetDistance(Vector3 pos1, Vector3 pos2)
        {
            return pos1.DistanceTo(pos2);
        }

        public static bool IsPointNearPoint(float range, Vector3 pos1, Vector3 pos2)
        {
            return GetDistance(pos1, pos2) <= range;
        }

        public static async Task RemoveJobVehicleIfExist(RXPlayer dbPlayer)
        {
            foreach (var vehicle in JobVehicles)
            {
                if (vehicle == null) continue;
                if (vehicle.OwnerId != dbPlayer.Id) continue;
                await vehicle.DeleteAsync();
                JobVehicles.Remove(vehicle);
                return;
            }
        }

        public static async Task<bool> IsJobVehicleAtPoint(RXPlayer dbPlayer, Vector3 pos)
        {
            foreach (RXVehicle Vehicle in JobVehicles)
            {
                if (Vehicle == null) continue;
                if (Vehicle.JobId != MowerModule.MowerJobVehMarkId && IsPointNearPoint(7.0f, await Vehicle.GetPositionAsync(), pos))
                {
                    return true;
                }

                if (IsPointNearPoint(5.0f, await Vehicle.GetPositionAsync(), pos))
                {
                    return true;
                }
            }
            return false;
        }

        public static RXVehicle GetJobVehicle(RXPlayer dbPlayer, int catId)
        {
            foreach (var vehicle in GetAllJobVehicles())
            {
                if (vehicle == null) continue;
                if (vehicle.JobId != catId || vehicle.OwnerId != dbPlayer.Id) continue;
                return vehicle;
            }
            return null;
        }

        public static async Task<RXVehicle> GetNearestJobVehicle(RXPlayer dbPlayer, int catId, float range)
        {
            foreach (var vehicle in GetClosestJobVehicles(await dbPlayer.GetPositionAsync(), range))
            {
                if (vehicle == null) continue;
                if (vehicle.JobId != catId) continue;
                return vehicle;
            }
            return null;
        }

        public static List<RXVehicle> GetAllJobVehicles()
        {
            return JobVehicles;
        }

        public static IEnumerable<RXVehicle> GetClosestJobVehicles(Vector3 positon, float range = 7.0f)
        {
            try
            {
                
                    return VehicleController.GetValidVehicles().Where(sx => sx != null && !sx.IsNull && sx.JobId != 0 && sx.Position.DistanceTo(positon) < range);
            }
            catch (Exception e)
            {
                RXLogger.Print(e.Message);
                return null;
            }
        }
    }
}
