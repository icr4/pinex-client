// #define DEV

namespace Constants
{
  public static class EnvConstants
  {

#if !DEV
    // Production Environment
    public static string WEB_ENDPOINT = "https://ex.pinnacola.it/api";
    public static string WEBSOCKET_ENDPOINT = "wss://ex.pinnacola.it/socket";
    public static string ENV_NAME = "production";
#else
    // Development Environment
    public static string WEB_ENDPOINT = "http://192.168.1.98:4000/api";
    public static string WEBSOCKET_ENDPOINT = "ws://192.168.1.98:4000/socket";
    public static string ENV_NAME = "development";

#warning [WARNING] !!! Development Environment Enabled
#endif
  }
}