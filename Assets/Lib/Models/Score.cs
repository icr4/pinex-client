using Newtonsoft.Json;

namespace Models
{
    public class Score
    {
        [JsonProperty("team")]
        public int team;

        [JsonProperty("total")]
        public int total;

        [JsonProperty("score")]
        public int score;

        [JsonProperty("to_pay")]
        public int toPay;
    }
}