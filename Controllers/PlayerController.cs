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
    class PlayerController : Script
    {
        //[HandleExceptions]
        public static List<RXPlayer> GetValidPlayers()
        {
            return NAPI.Pools.GetAllPlayers().ConvertAll(p => (RXPlayer)p).ToList().Where(p => p != null && p.IsLoggedIn).ToList();
        }

        //[HandleExceptions]
        public static List<RXPlayer> GetPlayers()
        {
            return NAPI.Pools.GetAllPlayers().ConvertAll(p => (RXPlayer)p).ToList().Where(p => p != null).ToList();
        }

        //[HandleExceptions]
        public static async Task<RXPlayer> FindPlayerById(object search, bool loggedIn = true)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;
            if (int.TryParse(searchString, out var playerId))
            {
                var player = await NAPI.Task.RunReturnAsync(() => (loggedIn ? GetValidPlayers() : GetPlayers()).FirstOrDefault(x => x.Id == playerId));
                if (player == null) return null;

                return player;
            }

            return null;
        }

        public static async Task<RXPlayer> FindPlayerByWallet(object search, bool loggedIn = true)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;
            
                var player = await NAPI.Task.RunReturnAsync(() => (loggedIn ? GetValidPlayers() : GetPlayers()).FirstOrDefault(x => x.WalletAdress == searchString));
                if (player == null) return null;

                return player;
            

        }
        public static async Task<RXPlayer> FindPlayerByPhoneNumber(object search)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;
            if (int.TryParse(searchString, out var playerId))
            {
                var player = await NAPI.Task.RunReturnAsync(() => (GetValidPlayers().Find(x => x.Phone == playerId)));
                if (player == null) return null;

                return player;
            }

            return null;
        }

        public static RXPlayer FindPlayerByIdNonAsync(object search, bool loggedIn = true)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;
            if (int.TryParse(searchString, out var playerId))
            {
                var player = NAPI.Task.RunReturn(() => (loggedIn ? GetValidPlayers() : GetPlayers()).FirstOrDefault(x => x.Id == playerId));
                if (player == null) return null;

                return player;
            }

            return null;
        }

        //[HandleExceptions]
        public static async Task<RXPlayer> FindPlayerByName(object search, bool loggedIn = true)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;

            var player = await NAPI.Task.RunReturnAsync(() => (loggedIn ? GetValidPlayers() : GetPlayers()).FirstOrDefault(x => x.Name == searchString));
            if (player == null) return null;

            return player;
        }

        //[HandleExceptions]
        public static async Task<RXPlayer> FindPlayerByStartsName(string search, bool loggedIn = true)
        {
            var searchString = search.ToString();
            if (string.IsNullOrEmpty(searchString)) return null;

            var player = await NAPI.Task.RunReturnAsync(() => (loggedIn ? GetValidPlayers() : GetPlayers()).FirstOrDefault(x => x.Name.ToLower().StartsWith(searchString.ToLower())));
            if (player == null) return null;

            return player;
        }

        //[HandleExceptions]
        public static async Task<List<RXPlayer>> GetPlayersInRange(Vector3 position, float range = 125f)
        {
            return await NAPI.Task.RunReturnAsync(() => GetValidPlayers().Where((player) => player.Position.DistanceTo(position) < range).ToList());
        }

        public static RXPlayer GetClosestPlayer(RXPlayer selff, Vector3 position, float range = 4.0f, UInt32 dimension = 0)
        {
            var dictionary = new Dictionary<float, RXPlayer>();


            foreach (var vehicle in GetValidPlayers())
            {
                if (vehicle == null || selff.Id == vehicle.Id || NAPI.Task.RunReturn(() => vehicle.Dimension) != dimension) continue;

                var _range = NAPI.Task.RunReturn(() => vehicle.Position).DistanceTo(position);

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
