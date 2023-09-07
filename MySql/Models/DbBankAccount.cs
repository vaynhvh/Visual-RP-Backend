using Backend.Modules.Bank;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;
using System.Threading.Tasks;

namespace Backend.MySql.Models
{
    [Table("bank_history")]
    public class DbBankHistory
    {
        [Key]
        public uint Id { get; set; }
        [JsonProperty(PropertyName = "n")]
        public uint AccountId { get; set; }
        [JsonProperty(PropertyName = "r")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "v")]
        public int Value { get; set; }
        public DateTime Date { get; set; }

        [JsonProperty(PropertyName = "d")]
        public string StringData
        {
            get => Date.ToString();
        }
    }

    [Table("bank_accounts")]
    public class DbBankAccount
    {
        [Key]
        public uint Id { get; set; }
        public string Name { get; set; }
        public int Balance { get; set; } = 0;

        [NotMapped]
        public List<DbBankHistory> History { get; set; }

        public async Task AddBankHistory(int value, string description)
        {
            var bankHistory = new DbBankHistory
            {
                AccountId = Id,
                Name = description,
                Value = value,
                Date = DateTime.Now
            };

            using var db = new RXContext();

            await db.BankHistories.AddAsync(bankHistory);

            await db.SaveChangesAsync();
        }

        public async Task Save()
        {
            using var db = new RXContext();

            var bankAccount = await db.BankAccounts.FirstOrDefaultAsync(x => x.Id == this.Id);
            if (bankAccount == null) return;

            bankAccount.Balance = this.Balance;
            bankAccount.Name = this.Name;

            await db.SaveChangesAsync();
        }

        public async Task<bool> TakeBankMoney(int money, string description = null, bool ignoreMinus = false)
        {
            if (money < 0) return false;
            if (this.Balance < money && !ignoreMinus) return false;
            if (this.Balance - money > this.Balance && !ignoreMinus) return false;

            this.Balance -= money;

            if (description != null)
            {
                await AddBankHistory(-money, description);
            }

            await RX.TakeMoneyFromStaatskonto(money, "Abzug von Konto " + this.Id + " (" + description + ")");

            await Save();

            return true;
        }

        public async Task<bool> GiveBankMoney(int money, string description = null)
        {
            if (money < 1) return false;
            if (this.Balance + money < this.Balance) return false;

            this.Balance += money;

            if (description != null)
            {
                await AddBankHistory(money, description);
            }

            await RX.GiveMoneyToStaatskonto(money, "Plus auf Konto " + this.Id + " (" + description + ")");

            await Save();

            return true;
        }
    }

    [Table("banks")]
    public class DbBank
    {
        [Key]
        public uint Id { get; set; }
        public string Position { get; set; }
    }
}
