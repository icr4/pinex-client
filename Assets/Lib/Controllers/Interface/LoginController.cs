using UnityEngine;
using System;
using Serializers.Api.Login;

public class LoginController : MonoBehaviour
{
    public static LoginController instance { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        // If token is about to expire, generate a new one to prevent in-app crashing
        long currentTime = new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds();
        long expTime = long.Parse(PlayerPrefs.GetString("auth_exp", "0"));

        if (PlayerPrefs.HasKey("auth_token") && currentTime < expTime)
        {
            this.LoginByToken(PlayerPrefs.GetString("auth_token"));
        }
        else
        {
            this.LoginByService();
        }
    }

    //-------------------------
    // Login Service
    //-------------------------


    void LoginByToken(string token)
    {
        StartCoroutine(LoginService.Verify(token, (response) =>
        {
            this.Connect(response);
        }, (response) =>
        {
            this.LoginByService();
        }, (end) => { }));
    }

    private void LoginByService()
    {
        switch (Application.platform)
        {
            case RuntimePlatform.Android:
                GooglePlayService.instance.Authenticate((code) =>
                {
                    this.Login("google_play", code);
                });
                break;
            case RuntimePlatform.IPhonePlayer:
                GameCenterService.instance.Authenticate((code, username) =>
                {
                    this.Login("game_center", code, username);
                });
                break;
            default:
                this.Login("manual", "p2");
                break;
        }
    }


    void Login(string provider, string token, string username = "")
    {
        StartCoroutine(LoginService.Login(token, provider, username, (response) =>
        {
            long exp = new DateTimeOffset(DateTime.UtcNow.AddDays(8)).ToUnixTimeSeconds();
            PlayerPrefs.SetString("auth_exp", exp.ToString());

            this.Connect(response);
        }, (response) =>
        {
            this.SetError(response);
        }, (end) => { }));
    }

    void Connect(LoginResponse response)
    {
        PlayerPrefs.SetString("auth_token", response.auth_token);
        SocketService.instance.Connect(response.auth_token);
        ClientService.instance.user = response.user;
        ClientService.instance.auth = response.auth_token;
    }

    //-------------------------
    // Login Error Handling
    //-------------------------

    public void SetError(string error)
    {
        ConfirmService.instance.Show("login.error.title", error);
    }

    public void SetError(LoginResponse response)
    {
        string error = this.TranslateErrorMessage(response);
        this.SetError(error);
    }

    private string TranslateErrorMessage(LoginResponse response)
    {
        switch (response.error)
        {
            default:
                return TranslateService.instance.Translate($"login.error.default");
            case "already_logged_in":
                return TranslateService.instance.Translate($"login.error.already_logged_in");
            case "wrong_credentials":
                return TranslateService.instance.Translate($"login.error.wrong_credentials");
            case "missing_credentials":
                return TranslateService.instance.Translate($"login.error.missing_credentials");
            case "invalid_version":
                return TranslateService.instance.Translate($"login.error.invalid_version");
            case "banned":
                DateTime expiresAt = DateTime.Parse(response.expires_at);
                string expires = expiresAt.ToString("yyyy/MM/dd hh:mm");

                return TranslateService.instance.Translate($"login.error.banned", new() { response.reason.Capitalize(), expires });
        }
    }
}
