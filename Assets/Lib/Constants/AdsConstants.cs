using System.Collections.Generic;
using Models;

namespace Constants
{
  public static class AdsConstants
  {
#if UNITY_ANDROID
        public static string REWARDED_ID = "ca-app-pub-4918196192145151/5092332232";
        public static string INTERSTITIAL_ID = "ca-app-pub-4918196192145151/8469605930";
#elif UNITY_IPHONE
        public static string REWARDED_ID = "ca-app-pub-4918196192145151/4644575833";
        public static string INTERSTITIAL_ID = "ca-app-pub-4918196192145151/7623428129";
#else
    public static string REWARDED_ID "";
    public static string INTERSTITIAL_ID = "";
#endif
  }
}