using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Serializers.Api.Friendship;
using Constants;
using Newtonsoft.Json;

public class FriendshipService : MonoBehaviour
{
    public static FriendshipService instance { get; private set; }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public static IEnumerator Search(string query, System.Action<SearchResponse> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        UnityWebRequest request = UnityWebRequest.Get(EnvConstants.WEB_ENDPOINT + $"/friendships/search?query={query}");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {ClientService.instance.auth}");
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError(request.error);
        }
        else
        {
            SearchResponse response = JsonConvert.DeserializeObject<SearchResponse>(request.downloadHandler.text);
            if (response.error != null) onError(response.error);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }

    public static IEnumerator List(System.Action<ListResponse> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        UnityWebRequest request = UnityWebRequest.Get(EnvConstants.WEB_ENDPOINT + "/friendships");
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
            ClientService.instance.friendships = response.friendships;

            if (response.error != null) onError(response.error);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }

    public static IEnumerator Create(string friendId, System.Action<ListResponse> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        string serializedParams = JsonConvert.SerializeObject(new CreateRequest(friendId));

        UnityWebRequest request = UnityWebRequest.Put(EnvConstants.WEB_ENDPOINT + "/friendships", serializedParams);
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
            ClientService.instance.friendships = response.friendships;

            if (response.error != null) onError(response.error);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }

    public static IEnumerator Confirm(string refId, System.Action<ListResponse> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        string serializedParams = JsonConvert.SerializeObject(new ConfirmRequest(refId));

        UnityWebRequest request = UnityWebRequest.Put(EnvConstants.WEB_ENDPOINT + "/friendships/confirm", serializedParams);
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
            ClientService.instance.friendships = response.friendships;

            if (response.error != null) onError(response.error);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }

    public static IEnumerator Decline(string refId, System.Action<ListResponse> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        string serializedParams = JsonConvert.SerializeObject(new ConfirmRequest(refId));

        UnityWebRequest request = UnityWebRequest.Put(EnvConstants.WEB_ENDPOINT + "/friendships/decline", serializedParams);
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
            ClientService.instance.friendships = response.friendships;

            if (response.error != null) onError(response.error);
            else onResult(response);
        }

        request.Dispose();
        onEnd(null);
    }
}
