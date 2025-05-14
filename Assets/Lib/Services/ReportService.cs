using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Serializers.Api.Report;
using Constants;
using Newtonsoft.Json;

public class ReportService : MonoBehaviour
{
    public static ReportService instance { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public static IEnumerator Create(string room, int reason, string notes, string user_id, System.Action<Response> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        string serializedParams = JsonConvert.SerializeObject(new CreateRequest(room, reason, notes, user_id));

        UnityWebRequest request = UnityWebRequest.Put(EnvConstants.WEB_ENDPOINT + "/reports", serializedParams);
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {ClientService.instance.auth}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError(request.error);
        }
        else
        {
            Response response = JsonConvert.DeserializeObject<Response>(request.downloadHandler.text);

            if (response.error != null) onError(response.error);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }

}
