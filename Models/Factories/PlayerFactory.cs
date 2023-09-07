using GTANetworkAPI;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models.Factories
{
    internal class PlayerFactory
    {
        public string Name => "PlayerFactory";

        public PlayerFactory()
        {
            RAGE.Entities.Players.CreateEntity = netHandle => Create(netHandle);
        }

        public RXPlayer Create(NetHandle netHandle)
        {
            try
            {
                var player = new RXPlayer(netHandle);
                if (player is null)
                    Console.WriteLine("Unable to create player.");

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
