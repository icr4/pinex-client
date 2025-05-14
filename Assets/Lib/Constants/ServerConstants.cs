namespace Constants
{
    public static class ServerConstants
    {
        //public static string WEBSOCKET_ENDPOINT = "wss://ex.pinnacola.it/socket";
        public static string WEBSOCKET_ENDPOINT = "ws://192.168.1.47:4000/socket";

        public static string[] MAIN_INBOUND_EVENTS = {
            "room_start",
            "lookup_last_room",
            "send_matchmaking",
            "update_user"
        };
        public static string[] ROOM_INBOUND_EVENTS = {
            "user_joined",
            "client_ready",
            "room_running",
            "draw_card",
            "put_card",
            "put_set",
            "drop_card",
            "replace_card",
            "chat"
        };

        public static string[] IGNORED_EVENTS = { };

        public static int TURN_DURATION = 60000;
    }
}