using Backend.Models;
using Backend.Utils;
using GTANetworkAPI;
using Microsoft.EntityFrameworkCore.ChangeTracking;
//using PostSharp.Extensibility;
//using PostSharp.Patterns.Diagnostics;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
//using static Backend.Utils.ExceptionAspect;

namespace Backend.Modules.Bank
{
    //[Log(AttributeTargetMemberAttributes = MulticastAttributes.Public)]
    class PaymentModule : RXModule
    {
        public PaymentModule() : base("Payment", new RXWindow("Payment")) { }

        //[HandleExceptions]
        [RemoteEvent]
        public void cancelPayment(RXPlayer player)
        {
            if (!player.IsLoggedIn || player.AwaitingPayment == null) return;

            var payment = player.AwaitingPayment;
            if (payment == null) return;

            NAPI.Task.Run(() => payment.CancelAction.Invoke(player));

            player.AwaitingPayment = null;
        }

        //[HandleExceptions]
        [RemoteEvent]
        public static async Task payBalance(RXPlayer player, int type)
        {
            try { 
                if (!player.IsLoggedIn || (type != 1 && type != 2) || player.AwaitingPayment == null) return;

            var payment = player.AwaitingPayment;
            if (payment == null) return;

                if (await player.TakeMoney(payment.Price))
                {
                    NAPI.Task.Run(() => payment.Action.Invoke(player));
                }
                else
                {
                    await player.SendNotify("Du hast zu wenig Bargeld bei dir!", 3500, "red", "Zahlung fehlgeschlagen");
                }
            

            player.AwaitingPayment = null;
        } catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
}
        [RemoteEvent]
        public static async Task payBalanceFrak(RXPlayer player, int type)
        {
            try
            {
                if (!player.IsLoggedIn || (type != 1 && type != 2) || player.AwaitingPayment == null) return;

                var payment = player.AwaitingPayment;
                if (payment == null) return;

                if (player.Team.Id == 0) return;

                if (!player.TeamMemberData.Bank && payment.NeedsPerm)
                {
                    await player.SendNotify("Du hast keine Berechtigung auf die Bank deiner Fraktion zuzugreifen!");
                    return;
                }

                if (await player.Team.BankAccount.TakeBankMoney(payment.Price, "Fahrzeugkauf"))
                {
                    NAPI.Task.Run(() => payment.Action.Invoke(player));
                }
                else
                {
                    await player.SendNotify("Es ist nicht genug Geld auf eurem Fraktionskonto!", 3500, "red", "Zahlung fehlgeschlagen");
                }


                player.AwaitingPayment = null;
            } catch (Exception e)
            {
                RXLogger.Print(e.Message);
            }
        }

        public static async void CreateFrakPayment(RXPlayer player, int price, Action<RXPlayer> action, string description = null, bool onlyCash = false, bool needsperm = false)
        {
            var paymentWindow = new RXWindow("Payment");

            var payment = new RXPayment(description, price, action, onlyCash, needsperm);
            player.AwaitingPayment = payment;

            await payBalanceFrak(player, 2);
        }
        //[HandleExceptions]
        public static async void CreatePayment(RXPlayer player, int price, Action<RXPlayer> action, string description = null, bool onlyCash = false)
        {
            var paymentWindow = new RXWindow("Payment");

            var payment = new RXPayment(description, price, action, onlyCash, false);
            player.AwaitingPayment = payment;

            await payBalance(player, 2);
        }

        //[HandleExceptions]
        public static async void CreatePaymentWithCancelOption(RXPlayer player, int price, Action<RXPlayer> action, Action<RXPlayer> cancelAction, string description = null, bool onlyCash = false)
        {
            var paymentWindow = new RXWindow("Payment");

            var payment = new RXPayment(description, price, action, cancelAction, onlyCash);
            player.AwaitingPayment = payment;
            await payBalance(player, 2);

        }
    }
}
