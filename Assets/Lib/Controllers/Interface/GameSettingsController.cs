using UnityEngine;
using UnityEngine.UI;

public class GameSettingsController : MonoBehaviour
{
    public static GameSettingsController instance { get; private set; }

    public GameObject settingsModal, modal;
    public Button openBtn, closeBtn;
    public Button accountTab, avatarTab;
    public GameObject accountContent, avatarContent;

    void Start()
    {
        this.Load();

        this.closeBtn.onClick.AddListener(Hide);
        this.accountTab.onClick.AddListener(() => OpenTab("Account"));
        this.avatarTab.onClick.AddListener(() => OpenTab("Avatar"));
        this.openBtn.onClick.AddListener(Show);
    }

    public void Show()
    {
        UsernameSettingsController.instance.Set();
        AvatarSettingsController.instance.Set();
        LanguageSettingsController.instance.Set();

        this.settingsModal.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.settingsModal.gameObject.SetActive(false);
    }

    private void Load()
    {
        this.LoadAccount();
    }

    private void LoadAccount()
    {
        string authenticator = TranslateService.instance.Translate($"authenticators.{ClientService.instance.user.provider}");
        string authData = $"{authenticator}\n{ClientService.instance.user.email}";

        this.accountContent.transform.Find("Authenticator/Value").GetComponent<Text>().text = authData;
    }

    private void OpenTab(string tab)
    {
        if (tab == "Account")
        {
            this.accountTab.transform.Find("Active").gameObject.SetActive(true);
            this.accountTab.transform.Find("Inactive").gameObject.SetActive(false);

            this.avatarTab.transform.Find("Active").gameObject.SetActive(false);
            this.avatarTab.transform.Find("Inactive").gameObject.SetActive(true);

            this.avatarContent.gameObject.SetActive(false);
            this.accountContent.gameObject.SetActive(true);
        }
        else if (tab == "Avatar")
        {
            this.avatarTab.transform.Find("Active").gameObject.SetActive(true);
            this.avatarTab.transform.Find("Inactive").gameObject.SetActive(false);

            this.accountTab.transform.Find("Active").gameObject.SetActive(false);
            this.accountTab.transform.Find("Inactive").gameObject.SetActive(true);

            this.accountContent.gameObject.SetActive(false);
            this.avatarContent.gameObject.SetActive(true);

            StartCoroutine(AvatarSettingsController.instance.Scrolldown());
        }
    }

}