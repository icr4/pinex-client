using System;
using Constants;
using Models;
using UnityEngine;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    public static LobbyController instance { get; private set; }

    public GameObject leftPanel, playPanel;
    public GameObject tutorialBtn, adBtn, poolConfirmModal, websiteBtn, versionObj;
    public Button rejoinPanel;

    private GameObject poolResource;

    public void ToggleRejoin(string uid)
    {
        this.rejoinPanel.gameObject.SetActive(true);
        this.rejoinPanel.onClick.RemoveAllListeners();
        this.rejoinPanel.onClick.AddListener(() => this.Rejoin(uid));
    }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        this.poolResource = Resources.Load("Prefabs/Pool", typeof(GameObject)) as GameObject;
        this.LookupLastRoom();
        this.LoadPools();

        this.tutorialBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            string text = TranslateService.instance.Translate("lobby.tutorial.text");
            ConfirmService.instance.Show("lobby.tutorial.title", text, () => { }, () =>
            {
                ClientService.instance.LoadTutorial();
            });
        });


        this.versionObj.GetComponent<Text>().text = $"v{ClientConstants.VERSION}";
        this.websiteBtn.transform.Find("Website").GetComponent<Text>().text = ClientConstants.WEBSITE_DISPLAY;
        this.websiteBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            Application.OpenURL(ClientConstants.WEBSITE);
        });

        this.adBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            AdsService.instance.RewardedAdModal();
        });

    }

    // ###
    // # Actions
    // ###
    public void JoinMatchmaking(string poolId)
    {
        SocketService.instance.Push("main", "start_matchmaking", Packets.Builder.StartMatchmaking(poolId));
    }

    public void ToggleMatchmaking()
    {
        ConfirmService.instance.Show("lobby.match.title", TranslateService.instance.Translate("lobby.match.text"), () =>
        {
            this.playPanel.SetActive(true);
            SocketService.instance.Push("main", "stop_matchmaking", null);
        });

        this.playPanel.SetActive(false);
    }

    void Rejoin(string uid)
    {
        ClientService.instance.room = uid;
        LoadingService.instance.enable();
        SocketService.instance.JoinRoom(uid);
    }

    void LookupLastRoom()
    {
        SocketService.instance.Push("main", "lookup_last_room", null);
    }

    // ###
    // # User
    // ###
    public void LoadUser(System.Action onEnd)
    {
        StartCoroutine(
            ProfileService.Show((response) =>
            {
                // Load player info
                this.leftPanel.transform.Find("User/Level").GetComponent<Text>().text = ClientService.instance.user.level.EnsureBoxed(2);
                this.leftPanel.transform.Find("Coins/Value").GetComponent<Text>().text = ClientService.instance.user.coins.EnsureBoxed();
                this.leftPanel.transform.Find("Tokens/Value").GetComponent<Text>().text = ClientService.instance.user.tokens.EnsureBoxed();

                // Load avatar
                StartCoroutine(AvatarService.instance.Fetch(
                    ClientService.instance.user.avatar, (result) =>
                    {
                        this.leftPanel.transform.Find("User/AvatarFrame/Avatar").GetComponent<RawImage>().texture = result as Texture2D;
                    }, (error) =>
                    {
                    }, (end) =>
                    {
                        onEnd();
                    }
                ));

                // Toggle tutorial modal
                if (!PlayerPrefs.HasKey("tutorial"))
                {
                    PlayerPrefs.SetInt("tutorial", 1);

                    string text = TranslateService.instance.Translate("lobby.welcome.text");
                    ConfirmService.instance.Show("lobby.welcome.title", text, () => { }, () =>
                    {
                        ClientService.instance.LoadTutorial();
                    });
                }

                // Toggle ads button based on last time
                if (PlayerPrefs.HasKey("advertisement"))
                {
                    string lastAd = PlayerPrefs.GetString("advertisement");
                    DateTime lastAdAt = DateTime.Parse(lastAd);

                    if (lastAdAt.AddMinutes(30.0) > DateTime.UtcNow)
                    {
                        this.adBtn.SetActive(false);
                    }
                }

                // Show interstitial after room
                if (AdsService.instance.showInterstitial)
                {
                    AdsService.instance.ShowInterstitialAd();
                }

                // Toggle review modal
                int loadCount = PlayerPrefs.GetInt("loadCount", 0);
                if (loadCount == 5)
                {
                    string text = TranslateService.instance.Translate("lobby.review.content");
                    ConfirmService.instance.Show("lobby.review.title", text, () => { }, () =>
                    {
                        Application.OpenURL(ClientConstants.SHARE_URL);
                    });
                }

                PlayerPrefs.SetInt("loadCount", loadCount + 1);
            },
                (error) => { },
                (end) => { }
            )
        );
    }

    public void VerifyAchievements(System.Action onEnd)
    {
        StartCoroutine(
            ProfileService.VerifyAchievements((response) =>
                {
                    AchievementModalController.instance.Show(response.achievements, response.level);
                },
                (error) => { },
                () => { onEnd(); }
            )
        );
    }

    // ###
    // # Pools
    // ###
    public void LoadPools()
    {
        ClientService.instance.pools.ForEach((pool) =>
        {
            Transform poolContainer = this.playPanel.transform.Find("Pools/Content").transform;
            GameObject panel = Instantiate(poolResource, poolContainer);
            panel.GetComponent<PoolComponent>().Set(pool);
        });
    }

    public void SelectPoolMode(Pool pool)
    {
        poolConfirmModal.SetActive(true);

        poolConfirmModal.transform.Find("Panel/Content/Footer/DuoBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        poolConfirmModal.transform.Find("Panel/Content/Footer/SquadBtn").GetComponent<Button>().onClick.RemoveAllListeners();
        poolConfirmModal.transform.Find("Panel/CloseBtn").GetComponent<Button>().onClick.RemoveAllListeners();

        poolConfirmModal.transform.Find("Panel/Content/Footer/DuoBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            SelectPool(pool, pool.id);
        });

        poolConfirmModal.transform.Find("Panel/Content/Footer/SquadBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            SelectPool(pool, pool.squad_id);
        });

        poolConfirmModal.transform.Find("Panel/CloseBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            poolConfirmModal.SetActive(false);
        });
    }

    void SelectPool(Pool pool, string poolId)
    {
        poolConfirmModal.SetActive(false);

        if (ClientService.instance.user.coins < pool.coins)
        {
            AdsService.instance.RewardedAdModal("lobby.match.error.title", "lobby.match.error.text");
            return;
        }

        instance.JoinMatchmaking(poolId);
    }
}