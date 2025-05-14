using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Models
{
    public class Set
    {
        [JsonProperty("id")]
        public string id { get; set; }
        [JsonProperty("team")]
        public int team { get; set; }
        [JsonProperty("cards")]
        public List<Card> cards { get; set; }

        public GameObject obj;

        public Set(string id, int team, List<Card> cards) {
            this.id = id;
            this.team = team;
            this.cards = cards;
        }
    }
}