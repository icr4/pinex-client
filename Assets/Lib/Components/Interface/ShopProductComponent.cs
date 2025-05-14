using UnityEngine;
using UnityEngine.UI;
using Models;
using UnityEngine.Purchasing;

public class ShopProductComponent : MonoBehaviour
{
  public ShopProduct product;

  public void Set(ShopProduct product)
  {
    this.product = product;
    this.product.obj = this.gameObject;

    this.SetReward();
    this.SetPrice();
    this.SetAvailable();

    this.transform.Find("BuyBtn").GetComponent<Button>().onClick.AddListener(BuyProduct);

    this.gameObject.SetActive(true);
  }

  void BuyProduct()
  {
    ShopController.instance.BuyProduct(this.product);
  }

  public void SetAvailable()
  {
    if (this.product.purchased)
    {
      this.transform.Find("BuyBtn").GetComponent<Button>().interactable = false;
    }
    else
    {
      this.transform.Find("BuyBtn").GetComponent<Button>().interactable = true;
    }
  }

  void SetReward()
  {
    if (this.product.reward_type == "avatar")
    {
      this.SetAvatarReward();
    }
    else if (this.product.reward_type == "coins")
    {
      this.SetCoinsReward();
    }
    else if (this.product.reward_type == "tokens")
    {
      this.SetTokensReward();
    }
  }

  void SetAvatarReward()
  {
    StartCoroutine(AvatarService.instance.Fetch(product.reward_str, (result) =>
            {
              this.transform.Find("AvatarReward/Frame/Avatar").GetComponent<RawImage>().texture = result as Texture2D;
            }, (error) => { }, (end) => { })
          );

    this.transform.Find("Value").GetComponent<Text>().text = "Avatar";
    this.transform.Find("Value").gameObject.SetActive(true);
    this.transform.Find("Icon").gameObject.SetActive(false);
    this.transform.Find("AvatarReward").gameObject.SetActive(true);
  }

  void SetCoinsReward()
  {
    this.transform.Find("Value").GetComponent<Text>().text = product.reward_int.ToString();
    this.transform.Find("Icon").GetComponent<Image>().sprite = ShopController.instance.coinsRes;

    this.transform.Find("Value").gameObject.SetActive(true);
    this.transform.Find("Icon").gameObject.SetActive(true);
    this.transform.Find("AvatarReward").gameObject.SetActive(false);
  }

  void SetTokensReward()
  {
    this.transform.Find("Value").GetComponent<Text>().text = product.reward_int.ToString();
    this.transform.Find("Icon").GetComponent<Image>().sprite = ShopController.instance.tokensRes;

    this.transform.Find("Value").gameObject.SetActive(true);
    this.transform.Find("Icon").gameObject.SetActive(true);
    this.transform.Find("AvatarReward").gameObject.SetActive(false);
  }

  void SetPrice()
  {
    if (this.product.price_type == "coins")
    {
      this.SetCoinsPrice();
    }
    else if (this.product.price_type == "tokens")
    {
      this.SetTokensPrice();
    }
    else if (this.product.price_type == "eur")
    {
      this.SetEurPrice();
    }
  }

  void SetCoinsPrice()
  {
    this.transform.Find("BuyBtn/PriceType").GetComponent<Image>().sprite = ShopController.instance.coinsRes;
    this.transform.Find("BuyBtn/Price").GetComponent<Text>().text = ((int)this.product.price).ToString();
  }

  void SetTokensPrice()
  {
    this.transform.Find("BuyBtn/PriceType").GetComponent<Image>().sprite = ShopController.instance.tokensRes;
    this.transform.Find("BuyBtn/Price").GetComponent<Text>().text = ((int)this.product.price).ToString();
  }

  void SetEurPrice()
  {
    this.transform.Find("BuyBtn/PriceType").GetComponent<Image>().sprite = ShopController.instance.eurRes;
    this.transform.Find("BuyBtn/Price").GetComponent<Text>().text = this.product.price.ToString();
  }
}
