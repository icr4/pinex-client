using System;
using System.Collections.Generic;
using Models;

namespace Serializers.Api.Shop
{

  [Serializable]
  public class PurchaseRequest
  {
    public PurchaseRequest(string product_id)
    {
      this.product_id = product_id;
    }

    public string product_id;
  }

  [Serializable]
  public class VerifyPurchaseRequest
  {

    public VerifyPurchaseRequest(string receipt)
    {
      this.receipt = receipt;
    }

    public string receipt;
  }

  [Serializable]
  public class ListResponse
  {
    public bool success { get; set; }
    public List<Models.ShopProduct> products { get; set; }
  }

  [Serializable]
  public class PurchaseResponse
  {
    public bool success { get; set; }
    public string error { get; set; }
    public string status { get; set; }
  }

  [Serializable]
  public class VerifyPurchaseResponse
  {
    public bool success { get; set; }
    public ShopProduct product { get; set; }
    public string error { get; set; }
  }
}