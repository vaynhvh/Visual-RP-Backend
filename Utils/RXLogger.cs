using Backend.Models;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Utils
{
    public enum LogType
    {
        INFO,
        WARNING,
        ERROR,
        SUCCESS,
    }

    public class RXLogger : Script
    {
        //[HandleExceptions]
        public static void Print(string str, LogType logType = LogType.INFO)
        {
            DateTime localDate = DateTime.Now;
            var culture = new CultureInfo("de-DE");

            ConsoleColor color = ConsoleColor.Cyan;

            switch (logType)
            {
                case LogType.INFO:
                    color = ConsoleColor.Cyan;
                    break;

                case LogType.ERROR:
                    color = ConsoleColor.Red;
                    break;

                case LogType.WARNING:
                    color = ConsoleColor.Yellow;
                    break;

                case LogType.SUCCESS:
                    color = ConsoleColor.Green;
                    break;
            }

            Console.ForegroundColor = color;
            Console.Write("[+] ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("[" + localDate.ToString(culture).Split(' ')[1] + "] ");
            Console.Write(str + "\n");
            Console.ResetColor();
        }

        //[HandleExceptions]
        [RemoteEvent("log")]
        public static void Debug2(RXPlayer player, string str)
        {
            if (!Configuration.DevMode) return;

            NAPI.Util.ConsoleOutput($"(RX) { str }");
        }

        //[HandleExceptions]
        public static void Debug(string str)
        {
            if (!Configuration.DevMode) return;

            NAPI.Util.ConsoleOutput($"(RX) { str }");
        }
    }
}
