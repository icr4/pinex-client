using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Serializers.Api.Login;
using Constants;
using Newtonsoft.Json;

public class LoginService : MonoBehaviour
{
    public static LoginService instance { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public static IEnumerator Login(string token, string provider, string username, System.Action<LoginResponse> onResult, System.Action<LoginResponse> onError, System.Action<object> onEnd)
    {
        string serializedParams = JsonConvert.SerializeObject(new LoginRequest(token, provider, username));

        UnityWebRequest request = UnityWebRequest.Put(EnvConstants.WEB_ENDPOINT + "/login", serializedParams);
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError(new LoginResponse() { error = "default" });
        }
        else
        {
            LoginResponse response = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
            if (response.error != null) onError(response);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }

    public static IEnumerator Verify(string token, System.Action<LoginResponse> onResult, System.Action<LoginResponse> onError, System.Action<object> onEnd)
    {
        string serializedParams = JsonConvert.SerializeObject(new LoginVerifyRequest(token));

        UnityWebRequest request = UnityWebRequest.Put(EnvConstants.WEB_ENDPOINT + "/login/verify", serializedParams);
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError(new LoginResponse() { error = "default" });
        }
        else
        {
            LoginResponse response = JsonConvert.DeserializeObject<LoginResponse>(request.downloadHandler.text);
            if (response.error != null) onError(response);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }
}
