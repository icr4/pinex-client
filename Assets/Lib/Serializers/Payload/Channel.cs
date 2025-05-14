using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Serializers.Payload
{
    public class ChannelReply
    {
        [JsonProperty("reason")]
        public string reason { get; set; }
        [JsonProperty("error")]
        public string error { get; set; }
    }
}
