using Models;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Purchasing;
using System;
using UnityEngine.Purchasing.Extension;

public class ShopController : MonoBehaviour, IStoreListener, IDetailedStoreListener
{
  public static ShopController instance { get; private set; }

  public ProductCollection products => throw new NotImplementedException();

  public GameObject shopModal, shopBtn;
  private GameObject productRes;
  public Sprite coinsRes, tokensRes, eurRes;
  public ConfigurationBuilder IAPBuilder;

  private IStoreController storeController;

  void Awake()
  {
    if (instance != null && instance != this) Destroy(this);
    else instance = this;
  }

  void Start()
  {
    this.productRes = Resources.Load("Prefabs/ShopProduct", typeof(GameObject)) as GameObject;
    this.coinsRes = Resources.Load("Interface/VectorIcons/Coins", typeof(Sprite)) as Sprite;
    this.tokensRes = Resources.Load("Interface/VectorIcons/Tokens", typeof(Sprite)) as Sprite;
    this.eurRes = Resources.Load("Interface/Icons/UI_Icon_CurrencyEuro", typeof(Sprite)) as Sprite;

    this.shopBtn.GetComponent<Button>().onClick.AddListener(Show);
    this.shopModal.transform.Find("Modal/CloseBtn").GetComponent<Button>().onClick.AddListener(Hide);
  }

  public void Show()
  {
    StandardPurchasingModule purchasingModule = StandardPurchasingModule.Instance();
    this.IAPBuilder = ConfigurationBuilder.Instance(purchasingModule);

    this.LoadProducts();
    this.shopModal.SetActive(true);
    StartCoroutine(Scrolldown());
  }

  public void Hide()
  {
    this.shopModal.SetActive(false);
  }

  public void BuyProduct(ShopProduct product)
  {
    if (!CanBuyProduct(product))
    {
      string text = TranslateService.instance.Translate("lobby.shop.insufficent_balance");
      ConfirmService.instance.Show("lobby.shop.title", text, () => { });
      return;
    }

    string confirm = TranslateService.instance.Translate("lobby.shop.purchase_confirm");
    ConfirmService.instance.Show("lobby.shop.title", confirm, () => { }, () =>
    {
      this.Purchase(product);
    });
  }

  private void Purchase(ShopProduct product)
  {
    StartCoroutine(ShopService.Purchase(product, (res) =>
      {
        if (res.success)
        {
          this.HandleOrder(product, res.status);
        }
        else
        {
          this.Hide();

          string text = TranslateService.instance.Translate($"lobby.shop.{res.error}");
          ConfirmService.instance.Show("lobby.shop.title", text, () => { });
        }
      }, (error) =>
      {
        this.ThrowGenericError();
      })
    );
  }

  private void ThrowGenericError()
  {
    this.Hide();
    string text = TranslateService.instance.Translate($"lobby.shop.generic_error");
    ConfirmService.instance.Show("lobby.shop.title", text, () => { });
  }

  private void HandleOrder(ShopProduct product, string status)
  {
    if (status == "confirmed")
    {
      this.AssignRewards(product);
    }
    else if (product.price_type == "eur")
    {
      this.storeController.InitiatePurchase(product.name);
    }
  }

  private void AssignRewards(ShopProduct product)
  {
    if (product.consumable)
    {
      product.obj.GetComponent<ShopProductComponent>().product.purchased = true;
      product.obj.GetComponent<ShopProductComponent>().SetAvailable();
    }

    LobbyController.instance.LoadUser(() =>
    {
      string text = TranslateService.instance.Translate("lobby.shop.purchase_ok");
      GameObject modal = ConfirmService.instance.Show("lobby.shop.title", text, () => { });

      string closeText = TranslateService.instance.Translate("confirm.buttons.close");
      modal.transform.Find("Panel/Content/Footer/CancelBtn/Text").GetComponent<Text>().text = closeText;
    });
  }

  private void LoadProducts()
  {
    Transform parent = this.shopModal.transform.Find("Modal/Body/Content/List").transform;

    foreach (Transform child in parent.transform)
    {
      GameObject.Destroy(child.gameObject);
    }

    StartCoroutine(ShopService.List((res) =>
    {
      res.products.ForEach((product) =>
      {
        if (product.price_type == "eur")
        {
          this.IAPBuilder.AddProduct(product.name, ProductType.Consumable, new IDs {
            {product.name, GooglePlay.Name},
            {product.name, MacAppStore.Name}
          });
        }

        GameObject productObj = Instantiate(this.productRes, parent);
        productObj.GetComponent<ShopProductComponent>().Set(product);
      });

      if (res.products.FindAll((p) => p.price_type == "eur").Count > 0)
      {
        UnityPurchasing.Initialize(this, this.IAPBuilder);
      }
    }));
  }

  private bool CanBuyProduct(ShopProduct product)
  {
    User user = ClientService.instance.user;

    if (product.price_type == "eur") return true;
    if (product.price_type == "coins" && user.coins >= (int)product.price) return true;
    if (product.price_type == "tokens" && user.tokens >= (int)product.price) return true;
    return false;
  }

  private IEnumerator Scrolldown()
  {
    GameObject scrollBarObj = this.shopModal.transform.Find("Modal/Body/Scrollbar").gameObject;
    yield return new WaitForSeconds(0.15f);
    scrollBarObj.GetComponent<Scrollbar>().value = 1;
  }

  //------------------
  // IAP Callbacks
  //------------------

  public void ConfirmPendingPurchase(Product product)
  {
  }

  public void OnInitializeFailed(InitializationFailureReason error)
  {
    this.ThrowGenericError();
  }

  public void OnInitializeFailed(InitializationFailureReason error, string message)
  {
    this.ThrowGenericError();
  }

  public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs purchaseEvent)
  {
    Product purchased = purchaseEvent.purchasedProduct;

    if (purchased.hasReceipt)
    {
      StartCoroutine(ShopService.VerifyPurchase(purchased.receipt, (res) =>
      {
        this.AssignRewards(res.product);
        this.ConfirmPendingPurchase(purchased);
      }, (error) =>
      {
        this.ThrowGenericError();
      }));

      return PurchaseProcessingResult.Complete;
    }
    else
    {
      return PurchaseProcessingResult.Pending;
    }
  }

  public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
  {
    this.ThrowGenericError();
  }

  public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
  {
    this.storeController = controller;
  }

  public void OnPurchaseFailed(Product product, PurchaseFailureDescription failureDescription)
  {
    this.ThrowGenericError();
  }
}