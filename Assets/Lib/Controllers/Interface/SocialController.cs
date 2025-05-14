using UnityEngine;
using UnityEngine.UI;

public class SocialController : MonoBehaviour
{
    public static SocialController instance { get; private set; }

    public GameObject socialModal, modal;
    public Button openBtn, closeBtn;
    public Button friendsTab, rankingsTab;
    public GameObject friendsContent, rankingsContent;

    void Start()
    {
        this.Load();

        this.closeBtn.onClick.AddListener(Hide);
        this.friendsTab.onClick.AddListener(() => OpenTab("Friends"));
        this.rankingsTab.onClick.AddListener(() => OpenTab("Rankings"));
        this.openBtn.onClick.AddListener(Show);

        // Load friendships before to ensure access to room & rankings
        SocialFriendsController.instance.Reload();
    }

    public void Show()
    {
        SocialFriendsController.instance.Reload();
        SocialRankingsController.instance.Reload();

        this.socialModal.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.socialModal.gameObject.SetActive(false);
    }

    private void Load() { }

    private void OpenTab(string tab)
    {
        if (tab == "Friends")
        {
            this.friendsTab.transform.Find("Active").gameObject.SetActive(true);
            this.friendsTab.transform.Find("Inactive").gameObject.SetActive(false);

            this.rankingsTab.transform.Find("Active").gameObject.SetActive(false);
            this.rankingsTab.transform.Find("Inactive").gameObject.SetActive(true);

            this.rankingsContent.gameObject.SetActive(false);
            this.friendsContent.gameObject.SetActive(true);
        }
        else if (tab == "Rankings")
        {
            this.friendsTab.transform.Find("Active").gameObject.SetActive(false);
            this.friendsTab.transform.Find("Inactive").gameObject.SetActive(true);

            this.rankingsTab.transform.Find("Active").gameObject.SetActive(true);
            this.rankingsTab.transform.Find("Inactive").gameObject.SetActive(false);

            this.rankingsContent.gameObject.SetActive(true);
            this.friendsContent.gameObject.SetActive(false);
        }
    }

}