using GTANetworkAPI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models.Factories
{
    internal class ColShapeFactory
    {
        public string Name => "ColShapeFactory";

        public ColShapeFactory()
        {
            RAGE.Entities.Colshapes.CreateEntity = netHandle => Create(netHandle);
        }

        public RXColShape Create(NetHandle netHandle)
        {
            try
            {
                var player = new RXColShape(netHandle);
                if (player is null)
                    Console.WriteLine("Unable to create colshape.");

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
