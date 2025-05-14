using UnityEngine;
using UnityEngine.UI;
using Models;

public class FriendRowComponent : MonoBehaviour
{
    public Friend friend = null;
    public Friendship friendship = null;

    public void Set(Friend friend, int position = 0)
    {
        this.friend = friend;
        this.transform.Find("Position").GetComponent<Text>().text = position > 0 ? position.ToString() : friend.position.ToString();
        this.transform.Find("Username").GetComponent<Text>().text = friend.username;
        this.transform.Find("Victories").GetComponent<Text>().text = friend.scores.victories.EnsureScored();
        this.transform.Find("Score").GetComponent<Text>().text = friend.scores.total_score.EnsureScored();

        if (friend.online)
        {
            this.transform.Find("Username").GetComponent<Text>().color = new Color32(173, 248, 97, 255);
        }

        if (friend.id == ClientService.instance.user.id)
        {
            this.transform.GetComponent<Image>().color = new Color32(164, 163, 163, 255);
        }

        this.GetComponent<Button>().onClick.AddListener(() =>
        {
            Friendship fr = ClientService.instance.friendships.Find((fr) => fr.friend.id == friend.id);
            FriendModalController.instance.Set(this.friend, fr);
        });
    }

    public void SetFriendship(Friendship friendship, bool isRanking = false)
    {
        this.friendship = friendship;

        if (friendship.status == "to_confirm" && !isRanking)
        {
            this.gameObject.transform.Find("Actions/Pending/Text").GetComponent<Text>().text = TranslateService.instance.Translate("lobby.social.friends.pending");
            this.gameObject.transform.Find("Actions/Pending").gameObject.SetActive(true);
            this.gameObject.GetComponent<Image>().color = new Color32(236, 205, 128, 100);
        }
    }

    void Start() { }
}
