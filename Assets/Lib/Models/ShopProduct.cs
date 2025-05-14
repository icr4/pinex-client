using Newtonsoft.Json;
using UnityEngine;

namespace Models
{
  public class ShopProduct
  {
    [JsonProperty("id")]
    public string id;

    [JsonProperty("name")]
    public string name;

    [JsonProperty("price_type")]
    public string price_type;

    [JsonProperty("price")]
    public float price;

    [JsonProperty("reward_type")]
    public string reward_type;

    [JsonProperty("reward_int")]
    public int reward_int;

    [JsonProperty("reward_str")]
    public string reward_str;

    [JsonProperty("consumable")]
    public bool consumable;
    [JsonProperty("purchased")]
    public bool purchased;

    public GameObject obj;
  }
}