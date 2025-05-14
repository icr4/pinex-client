using UnityEngine;
using UnityEngine.UI;
using Serializers.Api.Profile;
using System.Text.RegularExpressions;

public class UsernameSettingsController : MonoBehaviour
{
    public static UsernameSettingsController instance { get; private set; }
    public GameObject usernameObj;
    private GameObject valueObj, inputObj, changeObj;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void Start()
    {
        this.valueObj = this.usernameObj.transform.Find("Value").gameObject;
        this.inputObj = this.usernameObj.transform.Find("Input").gameObject;
        this.changeObj = this.usernameObj.transform.Find("ChangeBtn").gameObject;

        this.changeObj.GetComponent<Button>().onClick.AddListener(Edit);
        this.inputObj.GetComponent<InputField>().onSubmit.AddListener(Submit);
        this.Set();
    }

    public void Set()
    {
        this.valueObj.GetComponent<Text>().text = ClientService.instance.user.username;
        this.inputObj.GetComponent<InputField>().text = ClientService.instance.user.username;
        this.valueObj.gameObject.SetActive(true);
        this.inputObj.gameObject.SetActive(false);
        this.changeObj.GetComponent<Button>().interactable = true;
    }

    private void Edit()
    {
        this.valueObj.gameObject.SetActive(false);
        this.inputObj.gameObject.SetActive(true);

        inputObj.GetComponent<InputField>().Select();
        inputObj.GetComponent<InputField>().ActivateInputField();
        this.changeObj.GetComponent<Button>().interactable = false;
    }

    private void Submit(string username)
    {
        string text = TranslateService.instance.Translate("lobby.settings.account.username.confirm", new() { username });

        if (ValidateUsername(username) is string error)
        {
            this.DisplayError(error);
            this.Set();
            return;
        }
        else if (username == ClientService.instance.user.username)
        {
            this.Set();
            return;
        }

        ConfirmService.instance.Show(text, () =>
        {
            this.Set();
        }, () =>
        {
            this.UpdateUsername(username);
        });
    }

    private void UpdateUsername(string username)
    {
        UpdateUsernameRequest request = new UpdateUsernameRequest(username);
        StartCoroutine(ProfileService.Update(request, (result) =>
        {
            ClientService.instance.user = result.user;
            LobbyController.instance.LoadUser(() => { });
        }, (error) =>
        {
            this.DisplayError("generic");
        }, (end) =>
        {
            this.Set();
        }));
    }

    private string ValidateUsername(string username)
    {
        if (username.Length < 3 || username.Length > 14)
        {
            return "length";
        }
        else if (!Regex.IsMatch(username, @"^[a-zA-Z0-9_]{3,14}$"))
        {
            return "regex";
        }
        else
        {
            return null;
        }
    }

    private void DisplayError(string error)
    {
        ConfirmService.instance.Show(
            "lobby.settings.account.username.errors.title",
            TranslateService.instance.Translate($"lobby.settings.account.username.errors.{error}"),
            () => { }
        );
    }
}