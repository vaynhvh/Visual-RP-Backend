using Backend.Models;
using Backend.MySql;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend
{
    [Flags]
    public enum AnimationFlags
    {
        Loop = 1 << 0,
        StopOnLastFrame = 1 << 1,
        OnlyAnimateUpperBody = 1 << 4,
        AllowPlayerControl = 1 << 5,
        Cancellable = 1 << 7,
        AllowRotation = 32,
        CancelableWithMovement = 128,
        RagdollOnCollision = 4194304
    }

    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class RX
    {
        //[HandleExceptions]
        public static void SendGlobalNotifyToAll(string text, int duration = 8000, string color = "red", Icon icon = Icon.Admin)
        {
            int iconStr = 0;

            if (icon == Icon.Admin)
            {
                iconStr = 0;
            }
            if (icon == Icon.LSPD)
            {
                iconStr = 1;
            }
            if (icon == Icon.Marriage)
            {
                iconStr = 4;
            }
            if (icon == Icon.Dev)
            {
                iconStr = 2;
            }
            if (icon == Icon.Events)
            {
                iconStr = 5;
            }
            if (icon == Icon.WN)
            {
                iconStr = 6;
            }
            if (icon == Icon.Drop)
            {
                iconStr = 7;
            }
            if (icon == Icon.LSMC)
            {
                iconStr = 8;
            }
            if (icon == Icon.Army)
            {
                iconStr = 9;
            }
            //if (color == ) color = "#242424";


            NAPI.Task.Run(() => NAPI.ClientEvent.TriggerClientEventForAll("sendGlobalNotification", text, iconStr));
        }

        //[HandleExceptions]
        public static void SendGlobalNotifyToAllWhich(Predicate<RXPlayer> which, string text, int duration = 8000, string color = "red", Icon icon = Icon.Admin)
        {

            int iconStr = 0;

            if (icon == Icon.Admin)
            {
                iconStr = 0;
            }
            if (icon == Icon.LSPD)
            {
                iconStr = 1;
            }
            if (icon == Icon.Marriage)
            {
                iconStr = 4;
            }
            if (icon == Icon.Dev)
            {
                iconStr = 2;
            }
            if (icon == Icon.Events)
            {
                iconStr = 5;
            }
            if (icon == Icon.WN)
            {
                iconStr = 6;
            }
            if (icon == Icon.Drop)
            {
                iconStr = 7;
            }
            if (icon == Icon.LSMC)
            {
                iconStr = 8;
            }
            if (icon == Icon.Army)
            {
                iconStr = 9;
            }

            NAPI.Task.Run(() =>
            {
                for (int i = NAPI.Pools.GetAllPlayers().Count - 1; i >= 0; i--)
                {
                    RXPlayer player = (RXPlayer)NAPI.Pools.GetAllPlayers()[i];

                    if (which(player))
                    {
                        player.TriggerEvent("sendGlobalNotification", text, duration, color, iconStr);
                    }
                }
            });
        }

        //[HandleExceptions]
        public static void SendNotifyToAll(string text, int duration = 3500, string color = "red", string title = "")
            => NAPI.Task.Run(() => NAPI.ClientEvent.TriggerClientEventForAll("sendPlayerNotification", text, duration, color, title, ""));

        //[HandleExceptions]
        public static void SendNotifyToAllWhich(Predicate<RXPlayer> which, string text, int duration = 3500, string color = "red", string title = "")
        {
            NAPI.Task.Run(() =>
            {
                for (int i = NAPI.Pools.GetAllPlayers().Count - 1; i >= 0; i--)
                {
                    RXPlayer player = (RXPlayer)NAPI.Pools.GetAllPlayers()[i];

                    if (which(player))
                    {
                        player.TriggerEvent("sendPlayerNotification", text, duration, color, title, "");
                    }
                }
            });
        }

        //[HandleExceptions]
        public static async Task TakeMoneyFromStaatskonto(int amount, string description = "")
        {
            using var db = new RXContext();

            var account = await db.BankAccounts.FirstOrDefaultAsync(x => x.Id == 1);
            if (account == null) return;

            account.Balance -= amount;

            await account.AddBankHistory(-amount, description);

            await db.SaveChangesAsync();
        }

        //[HandleExceptions]
        public static async Task GiveMoneyToStaatskonto(int amount, string description = "")
        {
            using var db = new RXContext();

            var account = await db.BankAccounts.FirstOrDefaultAsync(x => x.Id == 1);
            if (account == null) return;

            account.Balance += amount;

            await account.AddBankHistory(amount, description);

            await db.SaveChangesAsync();
        }

        //[HandleExceptions]
        public static HeadOverlay CreateHeadOverlay(byte index, byte color, byte secondaryColor, float opacity)
        {
            HeadOverlay result = default(HeadOverlay);
            result.Index = index;
            result.Color = color;
            result.SecondaryColor = secondaryColor;
            result.Opacity = opacity;
            return result;
        }

        //[HandleExceptions]
        public static bool PlayerExists(RXPlayer player)
        {
            if (player == null) return false;

            lock (player)
            {
                if (!NAPI.Pools.GetAllPlayers().Contains(player)) return false;

                return true;
            }
        }

        public static string GenerateRandomHexString(int length)
        {
            if (length % 2 != 0)
            {
                return "";
            }

            byte[] buffer = new byte[length / 2];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(buffer);
            }

            StringBuilder hex = new StringBuilder(length);
            foreach (byte b in buffer)
            {
                hex.AppendFormat("{0:x2}", b);
            }

            return $"0x{hex.ToString()}";
        }
    }
}
