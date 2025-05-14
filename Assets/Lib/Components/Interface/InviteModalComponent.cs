using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class InviteModalComponent : MonoBehaviour
{
    public void Set(string username, string poolId)
    {
        string text = TranslateService.instance.Translate("invite.text", new() { username });
        this.transform.Find("Text").GetComponent<Text>().text = text;

        this.transform.Find("Actions/AcceptBtn/Text").GetComponent<Text>().text = TranslateService.instance.Translate("invite.accept");
        this.transform.Find("Actions/DeclineBtn/Text").GetComponent<Text>().text = TranslateService.instance.Translate("invite.decline");

        this.transform.Find("Actions/AcceptBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            this.OnAccept(poolId);
        });

        this.transform.Find("Actions/DeclineBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            this.Hide();
        });
    }

    private void OnAccept(string poolId)
    {
        string current = SceneManager.GetActiveScene().name.ToLower();
        string text = TranslateService.instance.Translate($"invite.confirm.{current}");

        ConfirmService.instance.Show(text, () =>
        {
            this.Hide();
        }, () =>
        {
            if (current == "lobby")
            {
                SocketService.instance.Push("main", "start_matchmaking", Packets.Builder.StartMatchmaking(poolId));
            }
            else if (current == "room")
            {
                AdsService.instance.showInterstitial = false;

                InputController.instance.LeaveRoom(() =>
                {
                    SocketService.instance.Push("main", "start_matchmaking", Packets.Builder.StartMatchmaking(poolId));
                });
            }
        });
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
        GameObject.Destroy(this.gameObject);
    }
}