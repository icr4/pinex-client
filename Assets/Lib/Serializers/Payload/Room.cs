using Newtonsoft.Json;
using Models;

namespace Serializers.Payload
{
    public class JoinRoomPayload
    {
        [JsonProperty("room")]
        public Room room { get; set; }
        [JsonProperty("player")]
        public Player player { get; set; }
    }

    public class CardPayload
    {
        [JsonProperty("number")]
        public int number;
        [JsonProperty("seed")]
        public int seed;
    }
}
