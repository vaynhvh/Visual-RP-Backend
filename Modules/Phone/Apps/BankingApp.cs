using Backend.Controllers;
using Backend.Models;
using Backend.Modules.Bank;
using Backend.Utils;
using GTANetworkAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Backend.Modules.Phone.Apps
{
    class BankingApp : RXModule
    {
        public BankingApp() : base("BankAppOverview", new RXWindow("BankAppOverview")) { }

        [RemoteEvent]
        public async Task RqBankAppValue(RXPlayer player)
        {
            try
            {
                if (player == null) return;
                await player.TriggerEventAsync("RsBankAppValue", player.BankAccount.Balance);            
            }
            catch (Exception ex)
            {
                RXLogger.Print(ex.Message);
                return;
            }
        }

        [RemoteEvent]
        public async Task RqBankAppHistory(RXPlayer player)
        {
            try
            {
                if (player == null) return;
                await player.TriggerEventAsync("RsBankAppHistory", NAPI.Util.ToJson(player.BankAccount.History.OrderBy(x => x.Id).Reverse().ToList()));
            }
            catch (Exception ex)
            {
                RXLogger.Print(ex.Message);
                return;
            }
        }
    }

    class BankAppTransfer : RXModule
    {
        public BankAppTransfer() : base("BankAppTransfer", new RXWindow("BankAppTransfer")) { }

        public static int bankingmaxcap = 1000000;
        public static int bankingmincap = 500;
        public static int tax = 1; //1% aktuell deaktiviert

        [RemoteEvent]
        public async Task requestBankingCap(RXPlayer player)
        {   // Achtung - BankingCap wird auch im Player abgefragt
            if (player == null) return;

            await this.Window.TriggerEvent(player, "responseBankingCap", bankingmaxcap.ToString(), bankingmincap.ToString());
        }

    } 
 }
