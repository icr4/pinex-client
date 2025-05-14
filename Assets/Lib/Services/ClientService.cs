using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Services.Core;
using Unity.Services.Core.Environments;
using Models;
using Constants;

public class ClientService : MonoBehaviour
{
    public static ClientService instance { get; private set; }
    public User user = null;
    public List<Pool> pools = null;
    public List<Friendship> friendships = null;
    public string room = null;
    public string auth = null;
    private GameObject inviteModalResource;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        Screen.sleepTimeout = 0;
        LoadingService.instance.enable();

        UnityServices.InitializeAsync(new InitializationOptions().SetEnvironmentName(EnvConstants.ENV_NAME));

        this.inviteModalResource = Resources.Load("Prefabs/InviteModal", typeof(GameObject)) as GameObject;

        StartCoroutine(LoadSceneCoroutine("Login", () =>
        {
            TranslateService.instance.TranslateScene("Login");
            LoadingService.instance.disable();
        }));
    }

    void Update() { }

    public void LoadRoom(Room room, Player player)
    {
        LoadingService.instance.enable();

        StartCoroutine(UnloadScenesCoroutine(() =>
        {
            StartCoroutine(LoadSceneCoroutine("Room", () =>
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("Room"));
                TranslateService.instance.TranslateScene("Room");
                AdsService.instance.LoadInterstitialAd();

                RoomController.instance.OnRoomJoin(room, player);
                LoadingService.instance.disable();

                SocketService.instance.RoomPush("client_ready", null);
            }));
        }));
    }

    public void LoadLobby(System.Action onEnd = null)
    {
        LoadingService.instance.enable();

        StartCoroutine(UnloadScenesCoroutine(() =>
        {
            StartCoroutine(LoadSceneCoroutine("Lobby", () =>
            {
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("Lobby"));
                TranslateService.instance.TranslateScene("Lobby");

                LobbyController.instance.VerifyAchievements(() =>
                {
                    LobbyController.instance.LoadUser(() =>
                    {
                        LoadingService.instance.disable();
                        if (onEnd != null) onEnd();
                    });
                });
            }));
        }));
    }

    public void LoadTutorial(System.Action onEnd = null)
    {
        LoadingService.instance.enable();

        StartCoroutine(UnloadScenesCoroutine(() =>
        {
            StartCoroutine(LoadSceneCoroutine("Tutorial", () =>
            {
                TranslateService.instance.TranslateScene("Tutorial");
                SceneManager.SetActiveScene(SceneManager.GetSceneByName("Tutorial"));
                LoadingService.instance.disable();

                if (onEnd != null) onEnd();
            }));
        }));
    }

    public void LoadInvite(string username, string poolId)
    {
        GameObject modal = Instantiate(inviteModalResource, this.MainCanvas().transform);
        modal.GetComponent<InviteModalComponent>().Set(username, poolId);
    }

    public void Logout()
    {
        LoadingService.instance.enable();
        SocketService.instance.Disconnect();

        StartCoroutine(UnloadScenesCoroutine(() =>
        {
            StartCoroutine(LoadSceneCoroutine("Login", () =>
            {
                LoadingService.instance.disable();
            }));
        }));
    }

    IEnumerator UnloadScenesCoroutine(System.Action onResult)
    {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            Scene s = SceneManager.GetSceneByBuildIndex(i);

            if (s.name != "Manager" && s.isLoaded)
            {
                AsyncOperation asyncUnload = SceneManager.UnloadSceneAsync(s.name);
                while (!asyncUnload.isDone) yield return null;
            }
        }

        onResult();
    }

    IEnumerator LoadSceneCoroutine(string scene, System.Action onResult)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);

        while (!asyncLoad.isDone) yield return null;
        onResult();
    }

    public IEnumerator TimeoutCoroutine(float seconds, System.Action onResult)
    {
        yield return new WaitForSeconds(seconds);
        onResult();
    }

    // Utils

    public Canvas MainCanvas()
    {
        return Resources.FindObjectsOfTypeAll<Canvas>()
             .ToList<Canvas>()
             .Find((canvas) => canvas.tag == "Canvas");
    }
}
