using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models.Factories
{
    internal class VehicleFactory
    {
        public string Name => "VehicleFactory";

        public VehicleFactory()
        {
            RAGE.Entities.Vehicles.CreateEntity = netHandle => Create(netHandle);
        }

        public RXVehicle Create(NetHandle netHandle)
        {
            try
            {
                var player = new RXVehicle(netHandle);
                if (player is null)
                    Console.WriteLine("Unable to create vehicle.");

                return player!;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return null;
            }
        }
    }
}
