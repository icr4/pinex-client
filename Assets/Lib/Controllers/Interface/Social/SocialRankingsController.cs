using UnityEngine;
using Models;
using UnityEngine.UI;

public class SocialRankingsController : MonoBehaviour
{
    public static SocialRankingsController instance { get; private set; }
    public GameObject playersContainer;
    public ScrollRect scrollRect;
    private GameObject friendRow;

    private bool isLoading = false;
    private bool more = false;
    private int page = 1;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        this.friendRow = Resources.Load("Prefabs/FriendRow") as GameObject;
        this.scrollRect.onValueChanged.AddListener((value) =>
        {
            if (value.y <= 0.4 && this.more)
            {
                this.Set(this.page + 1);
            }
        });
    }

    public void Reload()
    {
        this.Clear();
        this.Set(1);
    }

    private void Set(int page)
    {
        if (this.isLoading) return;
        this.isLoading = true;

        StartCoroutine(
            RankingService.List(page, (result) =>
            {
                this.more = result.more;
                this.page = page;
                result.rankings.ForEach(LoadPlayer);
            },
                (error) => { },
                (end) =>
                {
                    this.isLoading = false;
                }
            )
        );
    }

    private void Clear()
    {
        foreach (Transform child in this.playersContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void LoadPlayer(Friend player)
    {
        GameObject obj = Instantiate(friendRow, this.playersContainer.transform);
        obj.transform.SetParent(this.playersContainer.transform);
        obj.GetComponent<FriendRowComponent>().Set(player);

        // Find possibly friendship
        if (ClientService.instance.friendships.Find((fr) => fr.friend.id == player.id) is Friendship fr)
        {
            obj.GetComponent<FriendRowComponent>().SetFriendship(fr, true);
        }
    }
}