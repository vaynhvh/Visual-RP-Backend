using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Backend.Models
{
    public class RXBankHistory
    {
        [JsonProperty(PropertyName = "id")]
        public uint Id { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "value")]
        public int Value { get; set; }

        public RXBankHistory() { }
    }

    public class RXBank
    {
        [JsonProperty(PropertyName = "n")]
        public string Title { get; set; }

        [JsonProperty(PropertyName = "b")]
        public int Balance { get; set; }

        [JsonProperty(PropertyName = "m")]
        public int Money { get; set; }

        [JsonProperty(PropertyName = "c")]
        public string adddata { get; set; } = "Banken sind Banken!";

        [JsonProperty(PropertyName = "i")]
        public uint BankId { get; set; }

        [JsonProperty(PropertyName = "wp")]
        public uint WithdrawFeePer { get; set; } = 0;

        [JsonProperty(PropertyName = "dp")]
        public uint DepositFeePer { get; set; } = 0;

        [JsonProperty(PropertyName = "dm")]
        public uint DepositeeFeeMin { get; set; } = 0;

        [JsonProperty(PropertyName = "wx")]
        public uint WithdrawFeeMax { get; set; } = 0;

        [JsonProperty(PropertyName = "dx")]
        public uint DepositFeeMax { get; set; } = 0;

        [JsonProperty(PropertyName = "fb")]
        public bool Frakbank { get; set; }

        public RXBank() { }
    }
}
