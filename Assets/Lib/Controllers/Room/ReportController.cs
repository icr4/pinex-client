using UnityEngine;
using UnityEngine.UI;
using Models;
using Constants;

public class ReportController : MonoBehaviour
{
  public static ReportController instance { get; private set; }

  public GameObject modalObj;

  void Awake()
  {
    if (instance != null && instance != this) Destroy(this);
    else instance = this;
  }

  public void Show()
  {
    modalObj.transform.Find("Body/Content/Footer/ReportBtn").GetComponent<Button>().onClick.RemoveAllListeners();
    modalObj.transform.Find("Body/Content/Player/Value").GetComponent<Text>().text = ClientService.instance.user.username;
    modalObj.transform.Find("Body/Content/Notes/Input").GetComponent<InputField>().text = "";
    modalObj.transform.Find("Body/Content/Reason").gameObject.SetActive(false);

    modalObj.transform.Find("Body/Content/Footer/ReportBtn").GetComponent<Button>().onClick.AddListener(() => this.Submit());
    modalObj.gameObject.SetActive(true);
  }

  public void Show(Player player)
  {
    modalObj.transform.Find("Body/Content/Footer/ReportBtn").GetComponent<Button>().onClick.RemoveAllListeners();
    modalObj.transform.Find("Body/Content/Player/Value").GetComponent<Text>().text = player.username;
    modalObj.transform.Find("Body/Content/Notes/Input").GetComponent<InputField>().text = "";
    modalObj.transform.Find("Body/Content/Reason").gameObject.SetActive(true);
    modalObj.transform.Find("Body/Content/Reason/Value/ReasonBtn").GetComponent<Dropdown>().options = ClientConstants.REPORT_PLAYER_REASONS.ConvertAll(reason =>
    {
      reason = TranslateService.instance.Translate($"room.report.reason.{reason}");
      return new Dropdown.OptionData(reason);
    });

    modalObj.transform.Find("Body/Content/Footer/ReportBtn").GetComponent<Button>().onClick.AddListener(() => this.Submit(player));
    modalObj.gameObject.SetActive(true);
  }

  void Submit()
  {
    string room = RoomController.instance.room.room_id;
    string notes = modalObj.transform.Find("Body/Content/Notes/Input").GetComponent<InputField>().text;

    string description = TranslateService.instance.Translate("room.report.confirm");
    this.modalObj.GetComponent<SideModalController>().Hide();

    ConfirmService.instance.Show(description, () => { }, () =>
    {
      StartCoroutine(
        ReportService.Create(room, 0, notes, "", (res) => { }, (error) => { }, (end) => { })
      );
    });
  }

  void Submit(Player player)
  {
    string room = RoomController.instance.room.room_id;
    int reason = modalObj.transform.Find("Body/Content/Reason/Value/ReasonBtn").GetComponent<Dropdown>().value;
    string notes = modalObj.transform.Find("Body/Content/Notes/Input").GetComponent<InputField>().text;

    string description = TranslateService.instance.Translate("room.report.confirm");
    this.modalObj.GetComponent<SideModalController>().Hide();

    ConfirmService.instance.Show(description, () => { }, () =>
    {
      StartCoroutine(
        ReportService.Create(room, reason, notes, player.id, (res) => { }, (error) => { }, (end) => { })
      );
    });
  }
}