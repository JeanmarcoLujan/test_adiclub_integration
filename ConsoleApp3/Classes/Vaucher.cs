using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConsoleApp3.Classes
{
    public class Vaucher
    {
        public int Count { get; set; }
        public List<Reward> Rewards { get; set; } = new List<Reward>();
    }

    public class Reward
    {
        [JsonPropertyName("rewardId")]
        public int RewardId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("rewardType")]
        public string RewardType { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("availableDate")]
        public DateTime? AvailableDate { get; set; }

        [JsonPropertyName("endDate")]
        public DateTime? EndDate { get; set; }

        [JsonPropertyName("voucherValue")]
        public string VoucherValue { get; set; }
    }
}
