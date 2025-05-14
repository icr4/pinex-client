using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class AvatarService : MonoBehaviour
{
    public static AvatarService instance { get; private set; }

    private Dictionary<string, Texture2D> avatars = new();

    private void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public IEnumerator Fetch(string path, System.Action<Texture2D> onResult, System.Action<object> onError, System.Action<object> onEnd)
    {
        if (this.avatars.TryGetValue(path, out Texture2D cachedTexture))
        {
            onResult(cachedTexture);
            onEnd(null);
            yield break;
        }

        UnityWebRequest request = UnityWebRequestTexture.GetTexture(path);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            onError(request.error);
            onEnd(null);
        }
        else
        {
            Texture2D texture = ((DownloadHandlerTexture)request.downloadHandler).texture;

            try
            {
                this.avatars.Add(path, texture);
            }
            catch { }

            onResult(texture);
            onEnd(null);
        }
    }
}