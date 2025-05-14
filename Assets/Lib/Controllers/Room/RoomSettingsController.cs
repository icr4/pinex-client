using UnityEngine;
using UnityEngine.UI;
using Constants;

public class RoomSettingsController : MonoBehaviour
{
    public static RoomSettingsController instance { get; private set; }

    public GameObject settingsModal;
    public Button openBtn, audioBtn, reportBtn, leaveBtn, closeBtn;

    void Start()
    {
        this.openBtn.onClick.AddListener(Show);
        this.audioBtn.onClick.AddListener(Audio);
        this.reportBtn.onClick.AddListener(Report);
        this.leaveBtn.onClick.AddListener(Leave);
        this.closeBtn.onClick.AddListener(Hide);

        this.UpdateAudioBtn();
    }

    public void Show()
    {
        this.settingsModal.GetComponent<SideModalController>().Show();
    }

    public void Hide()
    {
        this.settingsModal.GetComponent<SideModalController>().Hide();
    }

    void Audio()
    {
        AudioService.instance.Toggle();
        this.UpdateAudioBtn();
    }

    void UpdateAudioBtn()
    {
        if (AudioService.instance.isEnabled)
        {
            this.audioBtn.transform.Find("ImageEnabled").gameObject.SetActive(true);
            this.audioBtn.transform.Find("ImageDisabled").gameObject.SetActive(false);
        }
        else
        {
            this.audioBtn.transform.Find("ImageEnabled").gameObject.SetActive(false);
            this.audioBtn.transform.Find("ImageDisabled").gameObject.SetActive(true);
        }
    }

    void Report()
    {
        ReportController.instance.Show();
        this.Hide();
    }

    void Leave()
    {
        this.Hide();
        AdsService.instance.showInterstitial = false;

        ConfirmService.instance.Show(
            TranslateService.instance.Translate("room.settings.confirm.leave"),
            () => { },
            () =>
            {
                InputController.instance.LeaveRoom();
            });
    }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }
}