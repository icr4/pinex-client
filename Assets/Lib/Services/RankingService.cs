using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Serializers.Api.Ranking;
using Constants;
using Newtonsoft.Json;

public class RankingService : MonoBehaviour
{
    public static RankingService instance { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public static IEnumerator List(int page, System.Action<ListResponse> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        UnityWebRequest request = UnityWebRequest.Get(EnvConstants.WEB_ENDPOINT + $"/rankings?page={page}");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {ClientService.instance.auth}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError(request.error);
        }
        else
        {
            ListResponse response = JsonConvert.DeserializeObject<ListResponse>(request.downloadHandler.text);

            if (response.error != null) onError(response.error);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }
}
