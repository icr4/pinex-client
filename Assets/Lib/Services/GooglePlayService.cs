using UnityEngine;

#if UNITY_ANDROID
using GooglePlayGames;
using GooglePlayGames.BasicApi;
#endif

public class GooglePlayService : MonoBehaviour
{
  public static GooglePlayService instance { get; set; }
  public int retries = 0;

  void Awake()
  {
    if (instance != null && instance != this) Destroy(this);
    else instance = this;
  }

#if UNITY_ANDROID

    public void Authenticate(System.Action<string> onResult) {
      this.retries = 0;
      PlayGamesPlatform.Instance.Authenticate((SignInStatus status) => ProcessAuthentication(status, onResult));
    }

    internal void ProcessAuthentication(SignInStatus status, System.Action<string> onResult) {
      if (status == SignInStatus.Success) {
        PlayGamesPlatform.Instance.RequestServerSideAccess(true, code => { 
            onResult(code);
         });
      } else if(retries < 2) {
        this.retries += 1;
        PlayGamesPlatform.Instance.ManuallyAuthenticate((SignInStatus status) => ProcessAuthentication(status, onResult));
      } else {
        string text = TranslateService.instance.Translate("login.error.google_play");
        LoginController.instance.SetError(text);
      }
    }

#else

  public void Authenticate(System.Action<string> onResult)
  {
    onResult("");
  }

#endif
}
