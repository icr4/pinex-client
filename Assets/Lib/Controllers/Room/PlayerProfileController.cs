using UnityEngine;
using UnityEngine.UI;
using Models;

public class PlayerProfileController : MonoBehaviour
{
    public static PlayerProfileController instance { get; private set; }

    public GameObject profileObj;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void Show(Player player)
    {
        profileObj.transform.Find("Content/Actions/AddFriendBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        profileObj.transform.Find("Content/Actions/ReportBtn").GetComponent<Button>().onClick.RemoveAllListeners();

        profileObj.transform.Find("Content/Username").GetComponent<Text>().text = player.username;
        profileObj.transform.Find("Content/LevelContainer/Level").GetComponent<Text>().text = player.level.ToString();
        profileObj.transform.Find("Content/AvatarFrame/Avatar").GetComponent<RawImage>().texture = player.avatarObj.transform.Find("Profile/AvatarFrame/Avatar").GetComponent<RawImage>().texture;
        this.profileObj.GetComponent<SideModalController>().Show();

        if (ClientService.instance.friendships.Find((fr) => fr.friend.id == player.id) == null)
        {
            profileObj.transform.Find("Content/Actions/AddFriendBtn").gameObject.SetActive(true);
            profileObj.transform.Find("Content/Actions/AddFriendBtn").GetComponent<Button>().onClick.AddListener(() => this.OnAddFriend(player));
        }
        else
        {
            profileObj.transform.Find("Content/Actions/AddFriendBtn").gameObject.SetActive(false);
        }

        profileObj.transform.Find("Content/Actions/ReportBtn").GetComponent<Button>().onClick.AddListener(() => this.OnReportPlayer(player));
    }

    void OnReportPlayer(Player player)
    {
        ReportController.instance.Show(player);
        this.profileObj.GetComponent<SideModalController>().Hide();
    }

    void OnAddFriend(Player player)
    {
        string msg = TranslateService.instance.Translate("lobby.social.confirm.add_friend", new() { player.username });
        ConfirmService.instance.Show(msg, () => { }, () =>
        {
            this.AddFriend(player);
        });
    }

    void AddFriend(Player friend)
    {
        StartCoroutine(
            FriendshipService.Create(friend.id, (res) =>
            {
                profileObj.transform.Find("Content/Actions/AddFriendBtn").gameObject.SetActive(false);
            }, (error) =>
            {
                string description = TranslateService.instance.Translate("error.description");
                ConfirmService.instance.Show("error.title", description, () => { });
            }, (end) =>
            {
            })
        );
    }
}