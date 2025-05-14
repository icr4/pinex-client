using System;
using System.Collections.Generic;

namespace Serializers.Api.Achievements
{

  [Serializable]
  public class VerifyResponse
  {
    public int level { get; set; }
    public List<Models.Achievement> achievements { get; set; }
  }
}