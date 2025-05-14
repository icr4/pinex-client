using Constants;
using UnityEngine;
using UnityEngine.UI;

public class ShareController : MonoBehaviour
{
  public static ShareController instance { get; private set; }
  public GameObject shareBtn, shareModal;

  void Awake()
  {
    if (instance != null && instance != this) Destroy(this);
    else instance = this;
  }

  void Start()
  {
    this.shareBtn.GetComponent<Button>().onClick.AddListener(Show);
    this.shareModal.transform.Find("Panel/CloseBtn").GetComponent<Button>().onClick.AddListener(Hide);
    this.shareModal.transform.Find("Panel/Content/Footer/CopyBtn").GetComponent<Button>().onClick.AddListener(Copy);
    this.shareModal.transform.Find("Panel/Content/Footer/CloseBtn").GetComponent<Button>().onClick.AddListener(Hide);
    this.shareModal.transform.Find("Panel/Content/Container/Link").GetComponent<Text>().text = ClientConstants.SHARE_DISPLAY;

  }

  void Copy()
  {
    TextEditor textEditor = new TextEditor();
    textEditor.text = ClientConstants.SHARE_URL;
    textEditor.SelectAll();
    textEditor.Copy();

    this.shareModal.transform.Find("Panel/Content/Footer/CopyBtn").GetComponent<Button>().interactable = false;
  }

  void Show()
  {
    this.shareModal.SetActive(true);
    this.shareModal.transform.Find("Panel/Content/Footer/CopyBtn").GetComponent<Button>().interactable = true;

  }

  void Hide()
  {
    this.shareModal.SetActive(false);
  }
}