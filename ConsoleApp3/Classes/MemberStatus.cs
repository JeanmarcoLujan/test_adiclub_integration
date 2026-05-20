using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace ConsoleApp3.Classes
{
    public class MemberStatus
    {
        [JsonPropertyName("totalPoints")]
        public int TotalPoints { get; set; }

        [JsonPropertyName("tierId")]
        public int TierId { get; set; }

        [JsonPropertyName("nextTierId")]
        public int NextTierId { get; set; }

        [JsonPropertyName("nextTierThreshold")]
        public int NextTierThreshold { get; set; }

        [JsonPropertyName("missingUpgradePoints")]
        public int MissingUpgradePoints { get; set; }

        [JsonPropertyName("nextRewardPoints")]
        public int NextRewardPoints { get; set; }

        [JsonPropertyName("lastPointDate")]
        public DateTime LastPointDate { get; set; }

        [JsonPropertyName("nextLevelPoints")]
        public int NextLevelPoints { get; set; }

        [JsonPropertyName("nextLevelCompletionPercentage")]
        public int NextLevelCompletionPercentage { get; set; }

        [JsonPropertyName("requalificationPoints")]
        public int RequalificationPoints { get; set; }

        [JsonPropertyName("requalificationGoal")]
        public int RequalificationGoal { get; set; }

        [JsonPropertyName("missingRequalificationPoints")]
        public int MissingRequalificationPoints { get; set; }

        [JsonPropertyName("requalificationProgress")]
        public string RequalificationProgress { get; set; }

        [JsonPropertyName("pointsNextExpired")]
        public int PointsNextExpired { get; set; }

        [JsonPropertyName("pointsExpiredDate")]
        public DateTime PointsExpiredDate { get; set; }
    }
}
