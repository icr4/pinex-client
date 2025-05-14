using UnityEngine;
using UnityEngine.UI;
using Models;

public class FriendModalController : MonoBehaviour
{
    public static FriendModalController instance { get; private set; }
    public GameObject friendModal;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void Set(Friend friend, Friendship friendship = null)
    {
        this.friendModal.transform.Find("Username").GetComponent<Text>().text = friend.username;
        this.friendModal.transform.Find("Content/Info/LevelContainer/Level").GetComponent<Text>().text = friend.level.ToString();

        // Scores
        this.friendModal.transform.Find("Content/Info/Scores/Victories/Value").GetComponent<Text>().text = friend.scores.victories.EnsureScored();
        this.friendModal.transform.Find("Content/Info/Scores/Losses/Value").GetComponent<Text>().text = friend.scores.losses.EnsureScored();
        this.friendModal.transform.Find("Content/Info/Scores/Total/Value").GetComponent<Text>().text = friend.scores.total_score.EnsureScored();

        // Actions
        this.friendModal.transform.Find("Content/Actions/InviteBtn").gameObject.SetActive(false);
        this.friendModal.transform.Find("Content/Actions/InviteBtn").GetComponent<Button>().onClick.RemoveAllListeners();

        this.friendModal.transform.Find("Content/Actions/AddFriendBtn").gameObject.SetActive(false);
        this.friendModal.transform.Find("Content/Actions/AddFriendBtn").GetComponent<Button>().onClick.RemoveAllListeners();

        this.friendModal.transform.Find("Content/Actions/RemoveFriendBtn").gameObject.SetActive(false);
        this.friendModal.transform.Find("Content/Actions/RemoveFriendBtn").GetComponent<Button>().onClick.RemoveAllListeners();

        this.friendModal.transform.Find("Content/Actions/AcceptFriendBtn").gameObject.SetActive(false);
        this.friendModal.transform.Find("Content/Actions/AcceptFriendBtn").GetComponent<Button>().onClick.RemoveAllListeners();

        this.friendModal.transform.Find("Content/Actions/DeclineFriendBtn").gameObject.SetActive(false);
        this.friendModal.transform.Find("Content/Actions/DeclineFriendBtn").GetComponent<Button>().onClick.RemoveAllListeners();

        this.SetActions(friend, friendship);

        this.friendModal.transform.Find("CloseBtn").GetComponent<Button>().onClick.AddListener(Hide);

        // Avatar
        this.friendModal.transform.Find("Content/Info/AvatarFrame/Avatar").GetComponent<RawImage>().texture = null;
        StartCoroutine(AvatarService.instance.Fetch(friend.avatar, (res) =>
        {
            this.friendModal.transform.Find("Content/Info/AvatarFrame/Avatar").GetComponent<RawImage>().texture = res as Texture2D;
        }, (error) =>
        {
        }, (end) =>
        {
            // Modal is available only after network request to prevent empty frames
            this.friendModal.gameObject.SetActive(true);
        }));
    }

    void SetActions(Friend friend, Friendship friendship = null)
    {
        if (friendship == null)
        {
            this.friendModal.transform.Find("Content/Actions/AddFriendBtn").gameObject.SetActive(true);
            this.friendModal.transform.Find("Content/Actions/AddFriendBtn").GetComponent<Button>().onClick.AddListener(() => OnAddFriend(friend));
        }
        else if (friendship.status == "confirmed")
        {
            if (friend.online)
            {
                this.friendModal.transform.Find("Content/Actions/InviteBtn").gameObject.SetActive(true);
                this.friendModal.transform.Find("Content/Actions/InviteBtn").GetComponent<Button>().onClick.AddListener(() => OnInviteFriend(friend));
            }

            if (friendship != null && friendship.ref_id != null)
            {
                this.friendModal.transform.Find("Content/Actions/RemoveFriendBtn").gameObject.SetActive(true);
                this.friendModal.transform.Find("Content/Actions/RemoveFriendBtn").GetComponent<Button>().onClick.AddListener(() => OnRemoveFriend(friendship));
            }

        }
        else if (friendship.status == "to_confirm")
        {
            this.friendModal.transform.Find("Content/Actions/AcceptFriendBtn").gameObject.SetActive(true);
            this.friendModal.transform.Find("Content/Actions/AcceptFriendBtn").GetComponent<Button>().onClick.AddListener(() => OnAcceptFriend(friendship));

            this.friendModal.transform.Find("Content/Actions/DeclineFriendBtn").gameObject.SetActive(true);
            this.friendModal.transform.Find("Content/Actions/DeclineFriendBtn").GetComponent<Button>().onClick.AddListener(() => OnDeclineFriend(friendship));
        }
    }

    void OnInviteFriend(Friend friend)
    {
        string msg = TranslateService.instance.Translate("lobby.social.confirm.invite_friend", new() { friend.username });
        ConfirmService.instance.Show(msg, () => { }, () =>
        {
            this.InviteFriend(friend);
        });
    }

    void OnAddFriend(Friend friend)
    {
        string msg = TranslateService.instance.Translate("lobby.social.confirm.add_friend", new() { friend.username });
        ConfirmService.instance.Show(msg, () => { }, () =>
        {
            this.AddFriend(friend);
        });
    }

    void OnRemoveFriend(Friendship friendship)
    {
        string msg = TranslateService.instance.Translate("lobby.social.confirm.remove_friend", new() { friendship.friend.username });
        ConfirmService.instance.Show(msg, () => { }, () =>
        {
            this.DeclineFriend(friendship);
        });
    }

    void OnAcceptFriend(Friendship friendship)
    {
        string msg = TranslateService.instance.Translate("lobby.social.confirm.accept_friend", new() { friendship.friend.username });
        ConfirmService.instance.Show(msg, () => { }, () =>
        {
            this.AcceptFriend(friendship);
        });
    }

    void OnDeclineFriend(Friendship friendship)
    {
        string msg = TranslateService.instance.Translate("lobby.social.confirm.decline_friend", new() { friendship.friend.username });
        ConfirmService.instance.Show(msg, () => { }, () =>
        {
            this.DeclineFriend(friendship);
        });
    }

    void InviteFriend(Friend friend)
    {
        SocketService.instance.Push("main", "start_matchmaking", Packets.Builder.StartMatchmaking(friend));
    }

    void AddFriend(Friend friend)
    {
        StartCoroutine(
            FriendshipService.Create(friend.id, (res) =>
            {
                Friendship fr = res.friendships.Find((f) => f.friend.id == friend.id);
                this.Set(fr.friend, fr);
            }, (error) =>
            {
                this.Hide();

                string description = TranslateService.instance.Translate("error.description");
                ConfirmService.instance.Show("error.title", description, () => { });
            }, (end) =>
            {
                SocialFriendsController.instance.Reload();
            })
        );
    }

    void AcceptFriend(Friendship friendship)
    {
        StartCoroutine(
            FriendshipService.Confirm(friendship.ref_id, (res) =>
            {
                Friendship fr = res.friendships.Find((f) => f.ref_id == friendship.ref_id);
                this.Set(fr.friend, fr);
            }, (error) =>
            {
                this.Hide();

                string description = TranslateService.instance.Translate("error.description");
                ConfirmService.instance.Show("error.title", description, () => { });
            }, (end) =>
            {
                SocialFriendsController.instance.Reload();
            })
        );
    }

    void DeclineFriend(Friendship friendship)
    {
        StartCoroutine(
            FriendshipService.Decline(friendship.ref_id, (res) =>
            {
            }, (error) =>
            {
                string description = TranslateService.instance.Translate("error.description");
                ConfirmService.instance.Show("error.title", description, () => { });
            }, (end) =>
            {
                this.Hide();
                SocialFriendsController.instance.Reload();
            })
        );
    }

    void Hide()
    {
        this.friendModal.gameObject.SetActive(false);
    }

}