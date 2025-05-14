using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Models
{
  public class Achievement
  {
    [JsonProperty("name")]
    public string name;


    [JsonProperty("rewards")]
    public List<AchievementReward> rewards;
  }

  public class AchievementCondition
  {
    [JsonProperty("type")]
    public string type;
    [JsonProperty("value")]
    public int value;
  }

  public class AchievementReward
  {
    [JsonProperty("type")]
    public string type;
    [JsonProperty("value")]
    public int value;
    [JsonProperty("value_str")]
    public string value_str;
  }
}