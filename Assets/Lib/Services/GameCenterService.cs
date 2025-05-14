using UnityEngine;
using System.Threading.Tasks;

#if UNITY_IOS
using Apple.GameKit;
#endif

public class GameCenterService : MonoBehaviour
{
  public static GameCenterService instance { get; set; }

  void Awake()
  {
    if (instance != null && instance != this) Destroy(this);
    else instance = this;
  }

#if UNITY_IOS

  public void Authenticate(System.Action<string, string> onResult)
  {
    Task.Run(async () =>
    {
      try
      {
        await GKLocalPlayer.Authenticate();
        onResult(GKLocalPlayer.Local.TeamPlayerId, GKLocalPlayer.Local.DisplayName);
      }
      catch
      {
        string text = TranslateService.instance.Translate("login.error.game_center");
        LoginController.instance.SetError(text);
      }
    });
  }

#else

  public void Authenticate(System.Action<string, string> onResult)
  {
    onResult("", "");
  }

#endif
}
