using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Serializers.Api.Profile;
using Constants;
using Newtonsoft.Json;
using Serializers.Api.Achievements;

public class ProfileService : MonoBehaviour
{
    public static ProfileService instance { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public static IEnumerator Show(System.Action<ShowResponse> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        UnityWebRequest request = UnityWebRequest.Get(EnvConstants.WEB_ENDPOINT + "/profile");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {ClientService.instance.auth}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError(request.error);
        }
        else
        {
            ShowResponse response = JsonConvert.DeserializeObject<ShowResponse>(request.downloadHandler.text);
            ClientService.instance.user = response.user;

            if (!PlayerPrefs.HasKey("last_user_level"))
            {
                PlayerPrefs.SetInt("last_user_level", response.user.level);
            }

            if (response.error != null) onError(response.error);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }

    public static IEnumerator VerifyAchievements(System.Action<VerifyResponse> onResult, System.Action<object> onError, System.Action onEnd)
    {
        UnityWebRequest request = UnityWebRequest.Get(EnvConstants.WEB_ENDPOINT + "/profile/achievements/verify");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {ClientService.instance.auth}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError(request.error);
        }
        else
        {
            VerifyResponse response = JsonConvert.DeserializeObject<VerifyResponse>(request.downloadHandler.text);
            onResult(response);
        }

        onEnd();
        request.Dispose();
    }

    public static IEnumerator Update(UpdateUsernameRequest request, System.Action<UpdateResponse> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        string payload = JsonConvert.SerializeObject(request);
        return Update(payload, onResult, onError, onEnd);
    }

    public static IEnumerator Update(UpdateAvatarRequest request, System.Action<UpdateResponse> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        string payload = JsonConvert.SerializeObject(request);
        return Update(payload, onResult, onError, onEnd);
    }

    public static IEnumerator Update(string payload, System.Action<UpdateResponse> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        UnityWebRequest request = UnityWebRequest.Put(EnvConstants.WEB_ENDPOINT + "/profile", payload);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {ClientService.instance.auth}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError(request.error);
        }
        else
        {
            UpdateResponse response = JsonConvert.DeserializeObject<UpdateResponse>(request.downloadHandler.text);
            if (response.error != null) onError(response.error);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }
}
