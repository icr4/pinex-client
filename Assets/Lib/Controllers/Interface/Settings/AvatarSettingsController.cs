using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Serializers.Api.Profile;
using System.Text.RegularExpressions;

public class AvatarSettingsController : MonoBehaviour
{
    public static AvatarSettingsController instance { get; private set; }
    public GameObject avatarsObj, scrollBarObj;
    private GameObject avatarResource;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void Start()
    {
        this.avatarResource = Resources.Load("Prefabs/AvatarContainer", typeof(GameObject)) as GameObject;
        this.Set();
    }

    public void Set()
    {
        foreach (Transform child in avatarsObj.transform)
        {
            Destroy(child.gameObject);
        }

        ClientService.instance.user.avatars.ForEach((av) =>
        {
            StartCoroutine(
                AvatarService.instance.Fetch(av, (result) =>
                {
                    GameObject avt = Instantiate(this.avatarResource, avatarsObj.transform);
                    avt.transform.SetParent(avatarsObj.transform, false);
                    avt.transform.Find("Frame/Avatar").GetComponent<RawImage>().texture = result as Texture2D;

                    if (av == ClientService.instance.user.avatar)
                    {
                        avt.transform.Find("Active").gameObject.SetActive(true);
                    }
                    else
                    {
                        avt.transform.Find("Active").gameObject.SetActive(false);
                        avt.transform.Find("Frame").GetComponent<Button>().onClick.AddListener(() => Submit(av));
                    }
                },
                (error) => { },
                (end) => { }
            ));
        });
    }

    private void Submit(string avatar)
    {
        string text = TranslateService.instance.Translate("lobby.settings.avatar.confirm");

        ConfirmService.instance.Show(text, () => { }, () =>
        {
            this.UpdateAvatar(avatar);
        });
    }

    private void UpdateAvatar(string url)
    {
        string avatar = Regex.Match(url, @"/avatars/([a-zA-Z0-9_]*).([a-zA-Z]*)").Groups[1].Value;

        UpdateAvatarRequest request = new UpdateAvatarRequest(avatar);
        StartCoroutine(ProfileService.Update(request, (result) =>
        {
            ClientService.instance.user = result.user;
            LobbyController.instance.LoadUser(() => { });
            this.Set();
        }, (error) =>
        {
            this.DisplayError("generic");
        }, (end) =>
        {
        }));
    }

    private void DisplayError(string error)
    {
        ConfirmService.instance.Show(
            "lobby.settings.avatar.errors.title",
            TranslateService.instance.Translate($"lobby.settings.avatar.errors.{error}"),
            () => { }
        );
    }

    public IEnumerator Scrolldown()
    {
        yield return new WaitForSeconds(0.15f);
        this.scrollBarObj.GetComponent<Scrollbar>().value = 1;
    }
}