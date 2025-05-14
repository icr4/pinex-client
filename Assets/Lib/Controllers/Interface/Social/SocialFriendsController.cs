using UnityEngine;
using UnityEngine.UI;
using Models;

public class SocialFriendsController : MonoBehaviour
{
    public static SocialFriendsController instance { get; private set; }
    public GameObject friendsContainer, searchInputObj;
    private GameObject friendRow;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        this.friendRow = Resources.Load("Prefabs/FriendRow") as GameObject;
        this.searchInputObj.GetComponent<InputField>().onSubmit.AddListener(Search);
    }

    public void Reload()
    {
        this.Search(this.searchInputObj.GetComponent<InputField>().text);
    }

    private void Set()
    {
        this.Clear();

        StartCoroutine(
            FriendshipService.List((result) =>
            {
                result.friendships.FindAll((fr) => fr.status != "pending").ForEach((fr) =>
                {
                    this.LoadFriend(fr.friend, fr);
                });
            },
                (error) => { },
                (end) => { }
            )
        );
    }

    private void Clear()
    {
        foreach (Transform child in this.friendsContainer.transform)
        {
            GameObject.Destroy(child.gameObject);
        }
    }

    private void LoadFriend(Friend friend, Friendship friendship = null)
    {
        GameObject obj = Instantiate(friendRow, this.friendsContainer.transform);
        obj.transform.SetParent(this.friendsContainer.transform);

        // If friend is loaded from search, use rankings position, else friends ranking
        if (friendship == null)
        {
            obj.GetComponent<FriendRowComponent>().Set(friend);
        }
        else
        {
            obj.GetComponent<FriendRowComponent>().Set(friend, this.friendsContainer.transform.childCount);
        }

        // If friend is loaded from search, find friend in friendships
        if (friendship == null && ClientService.instance.friendships.Find((fr) => fr.friend.id == friend.id) is Friendship fr)
        {
            friendship = fr;
        }

        // If friend is loaded from friendships or is present in friendships, mark it as friend
        if (friendship != null)
        {
            obj.GetComponent<FriendRowComponent>().SetFriendship(friendship);
        }
    }

    private void Search(string query)
    {
        if (query.Trim() == "")
        {
            this.Set();
            return;
        }

        this.Clear();
        StartCoroutine(
            FriendshipService.Search(query, (result) =>
            {
                result.users.ForEach((fr) =>
                {
                    this.LoadFriend(fr);
                });
            },
                (error) => { },
                (end) => { }
            )
        );
    }
}