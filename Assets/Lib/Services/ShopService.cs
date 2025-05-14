using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Serializers.Api.Shop;
using Constants;
using Newtonsoft.Json;
using Models;

public class ShopService : MonoBehaviour
{
  public static ShopService instance { get; private set; }

  void Awake()
  {
    if (instance != null && instance != this) Destroy(this);
    else instance = this;
  }


  public static IEnumerator List(System.Action<ListResponse> onResult)
  {
    UnityWebRequest request = UnityWebRequest.Get(EnvConstants.WEB_ENDPOINT + "/shop/products");
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Authorization", $"Bearer {ClientService.instance.auth}");
    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
      onResult(new ListResponse());
    }
    else
    {
      ListResponse response = JsonConvert.DeserializeObject<ListResponse>(request.downloadHandler.text);
      onResult(response);
    }

    request.Dispose();
  }

  public static IEnumerator Purchase(ShopProduct product, System.Action<PurchaseResponse> onResult, System.Action<object> onError)
  {
    string serializedParams = JsonConvert.SerializeObject(new PurchaseRequest(product.id));

    UnityWebRequest request = UnityWebRequest.Put(EnvConstants.WEB_ENDPOINT + "/shop/orders", serializedParams);
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Authorization", $"Bearer {ClientService.instance.auth}");
    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
      onError(request.error);
    }
    else
    {
      PurchaseResponse response = JsonConvert.DeserializeObject<PurchaseResponse>(request.downloadHandler.text);

      if (response.error != null) onError(response.error);
      else onResult(response);
    }

    request.Dispose();
  }

  public static IEnumerator VerifyPurchase(string receipt, System.Action<VerifyPurchaseResponse> onResult, System.Action<object> onError)
  {
    string serializedParams = JsonConvert.SerializeObject(new VerifyPurchaseRequest(receipt));

    UnityWebRequest request = UnityWebRequest.Put(EnvConstants.WEB_ENDPOINT + "/shop/orders/verify", serializedParams);
    request.SetRequestHeader("Content-Type", "application/json");
    request.SetRequestHeader("Authorization", $"Bearer {ClientService.instance.auth}");
    yield return request.SendWebRequest();

    if (request.result != UnityWebRequest.Result.Success)
    {
      onError(request.error);
    }
    else
    {
      VerifyPurchaseResponse response = JsonConvert.DeserializeObject<VerifyPurchaseResponse>(request.downloadHandler.text);

      if (response.error != null) onError(response.error);
      else onResult(response);
    }

    request.Dispose();
  }
}