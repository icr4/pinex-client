using Newtonsoft.Json;

namespace Models
{
    public class Locale
    {
        [JsonProperty("id")]
        public int id { get; set; }
        [JsonProperty("code")]
        public string code { get; set; }
        [JsonProperty("name")]
        public string name { get; set; }

        public Locale(int id, string code, string name) {
            this.id = id;
            this.code = code;
            this.name = name;
        }
    }
}