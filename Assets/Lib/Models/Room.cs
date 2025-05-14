using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
    public class Room
    {
        [JsonProperty("room_id")]
        public string room_id;

        [JsonProperty("players")]
        public List<Player> players { get; set; }

        [JsonProperty("sets")]
        public List<Set> sets { get; set; }

        [JsonProperty("turn")]
        public string turn;

        [JsonProperty("turn_state")]
        public string turnState;

        [JsonProperty("status")]
        public string status;

        [JsonProperty("grounded")]
        public List<Card> grounded;

        [JsonProperty("deck_count")]
        public int deck_count;

        [JsonProperty("scores")]
        public List<Score> scores;
    }
}