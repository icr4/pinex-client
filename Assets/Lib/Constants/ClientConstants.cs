using System.Collections.Generic;
using Models;
using UnityEngine;

namespace Constants
{
    public static class ClientConstants
    {
        public static string WEBSITE = "https://www.pinnacola.it";
        public static string WEBSITE_DISPLAY = "www.pinnacola.it";
        public static string SHARE_DISPLAY = WEBSITE_DISPLAY + "/download";
        public static string SHARE_URL = WEBSITE + "/download";
        public static string VERSION = Application.version;
        public static List<Locale> LOCALES = new List<Locale> {
            new Locale(0, "it", "Italiano"),
            new Locale(1, "en", "English")
        };

        public static List<string> REPORT_PLAYER_REASONS = new List<string> {
            "cheat",
            "offense",
            "other"
        };

        public static List<string> CARD_UTF_SEEDS = new List<string> {
            "♣",
            "♥",
            "♠",
            "♦",
            "🃏",
            "2♠"
        };

        public static List<string> CARD_UTF_COLORS = new List<string> {
            "grey",
            "#ff0000",
            "grey",
            "#ff0000",
            "white",
            "grey"
        };

        public static List<string> CARD_UTF_NUMBERS = new List<string> {
            "A",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "J",
            "Q",
            "K",
            "A",
            ""
        };

        public static int[] CARD_SEEDS_RED = { 2, 4 };
    }
}