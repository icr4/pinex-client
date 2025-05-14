using System.Collections.Generic;
using Newtonsoft.Json;
using Models;

namespace Serializers.Payload
{
    public class JoinMainPayload
    {
        [JsonProperty("pools")]
        public List<Pool> pools { get; set; }
        [JsonProperty("user")]
        public User user { get; set; }
    }
}
