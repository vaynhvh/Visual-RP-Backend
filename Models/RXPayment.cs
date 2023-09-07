using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class RXPayment
    {
        public string Reason { get; set; }
        public int Price { get; set; }
        public Action<RXPlayer> Action { get; set; } = player => { };
        public Action<RXPlayer> CancelAction { get; set; } = player => { };
        public bool OnlyCash { get; set; } = false;
        public bool NeedsPerm { get; set; } = false;

        public RXPayment(string reason, int price, Action<RXPlayer> action, Action<RXPlayer> cancelAction, bool onlyCash = false, bool needsperm = false)
        {
            Reason = reason;
            Price = price;
            Action = action;
            CancelAction = cancelAction;
            NeedsPerm = needsperm;
            OnlyCash = onlyCash;
        }

        public RXPayment(string reason, int price, Action<RXPlayer> action, bool onlyCash = false, bool needsperm = false)
        {
            Reason = reason;
            Price = price;
            Action = action;
            OnlyCash = onlyCash;
            NeedsPerm = needsperm;
        }
    }
}
