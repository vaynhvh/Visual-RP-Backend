using System;
using System.Collections.Generic;
using System.Text;

namespace Backend
{
    internal class Configuration
    {
        public static bool DevMode = false;

        public static bool ACDevMode = true;

        public static bool PaintballEvent = false;

        public static bool Open = false;

        public static readonly Dictionary<string, string> Connection = new Dictionary<string, string>
        {
            { "hostname", "localhost" },
            { "database", "pelogarp" },
            { "user", "root" },
            { "password", "" },
        };

        public static string ConnectionString = "server=" + Connection["hostname"] + ";database=" + Connection["database"] + ";user=" + Connection["user"] + ";password=" + Connection["password"] + ";pwd=" + Connection["password"] + ";ConvertZeroDateTime=True;";

        public static readonly Dictionary<string, string> GameDesignConnection = new Dictionary<string, string>
        {
            { "hostname", "localhost" },
            { "database", "gamedesign" },
            { "user", "root" },
            { "password", "" },
        };

        public static string GameDesignConnectionString = "server=" + GameDesignConnection["hostname"] + ";database=" + GameDesignConnection["database"] + ";user=" + GameDesignConnection["user"] + ";password=" + GameDesignConnection["password"] + ";pwd=" + GameDesignConnection["password"] + ";ConvertZeroDateTime=True;";
    }
}
