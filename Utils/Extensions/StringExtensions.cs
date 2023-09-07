using Backend.Models;
using Backend.Modules.Faction;
using GTANetworkAPI;
using Newtonsoft.Json;
using Org.BouncyCastle.Utilities.IO;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Utils.Extensions
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    internal static class StringExtensions
    {
        //[HandleExceptions]
        public static string ConvertHTML(this Color color)
        {
            return $"rgb({color.Red}, {color.Green}, {color.Blue})";
        }

        //[HandleExceptions]
        public static string FormatMoneyNumber(this uint money)
        {
            return (money.ToString("#,##0") + " $").Replace(".", ",");
        }

        //[HandleExceptions]
        public static string FormatMoneyNumber(this int money)
        {
            return (money.ToString("#,##0") + " $").Replace(".", ",");
        }

        //[HandleExceptions]
        public static Task<bool> ContainsSymbols(this string str)
        {
            Regex regex = new Regex(@"([a-zA-Z]+)_([a-zA-Z]+)");

            return Task.FromResult(!regex.IsMatch(str));
        }

        //[HandleExceptions]
        public static Vector3 ToPos(this string str)
        {
            try
            {
                string[] strs = str.Split(",");
                return new Vector3(
                    float.Parse(strs[0].Replace(".", ",")),
                    float.Parse(strs[1].Replace(".", ",")),
                    float.Parse(strs[2].Replace(".", ","))
                );
            }
            catch
            {
                return new Vector3(0, 0, 0);
            }
        }
        public static HashSet<RXTeam> ToTeam(this string str)
        {
            try
            {

                HashSet<RXTeam> Teams = new HashSet<RXTeam>();

                if (!string.IsNullOrEmpty(str))
                {
                    var splittedTeams = str.Split(',');
                    foreach (var teamIdString in splittedTeams)
                    {
                        if (!uint.TryParse(teamIdString, out var teamId) || teamId == 0) continue;
                        Teams.Add(TeamModule.Teams.Find(x => x.Id == teamId));
                    }
                }

                return Teams;
            }
            catch
            {
                return new HashSet<RXTeam>();
            }
        }
        public static HashSet<uint> ToUINT(this string str)
        {
            try
            {

                HashSet<uint> PlayerIds = new HashSet<uint>();

                if (!string.IsNullOrEmpty(str))
                {
                    var splittedTeams = str.Split(',');
                    foreach (var teamIdString in splittedTeams)
                    {
                        if (!uint.TryParse(teamIdString, out var playerid) || playerid == 0) continue;
                        PlayerIds.Add(playerid);
                    }
                }

                return PlayerIds;
            }
            catch
            {
                return new HashSet<uint>();
            }
        }



        //[HandleExceptions]
        public static string FromPos(this Vector3 pos)
        {
            return pos.X.ToString().Replace(",", ".") + "," + pos.Y.ToString().Replace(",", ".") + "," + pos.Z.ToString().Replace(",", ".");
        }

        //[HandleExceptions]
        public static bool IsValidJson<T>(this string strInput)
        {
            if (string.IsNullOrWhiteSpace(strInput)) return false;

            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JsonConvert.DeserializeObject<T>(strInput);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
