using Backend.Controllers;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Text;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Models
{
    public class NPC
    {
        public PedHash PedHash { get; set; }
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public UInt32 Dimension { get; set; }

        //[HandleExceptions]
        public NPC(PedHash pedHash, Vector3 position, float heading, uint dimension)
        {
            PedHash = pedHash;
            Position = position;
            Heading = heading;
            Dimension = dimension;

            Resource.ServerNpcs.Add(this);
            NAPI.Task.Run(() =>
            {
                foreach (RXPlayer player in PlayerController.GetValidPlayers())
                {
                    player.TriggerEvent("loadNpc", PedHash, Position.X, Position.Y, Position.Z, Heading, Dimension);
                }
            });
        }
    }
}
