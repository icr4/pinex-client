using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ChatController : MonoBehaviour
{
    public static ChatController instance { get; private set; }

    public GameObject chatModalObj, contentObj, inputObj, scrollBarObj, unreadAlertObj, openBtn;
    private GameObject messageResource;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        this.messageResource = Resources.Load("Prefabs/ChatMessage", typeof(GameObject)) as GameObject;
        this.openBtn.GetComponent<Button>().onClick.AddListener(Show);
        this.inputObj.GetComponent<InputField>().onSubmit.AddListener(Submit);
    }

    private void Show()
    {
        this.ToggleUnread(false);
        this.chatModalObj.GetComponent<SideModalController>().Show();
        StartCoroutine(Scrolldown());
    }

    private void Submit(string message)
    {
        this.inputObj.GetComponent<InputField>().text = "";
        SocketService.instance.RoomPush("chat", Packets.Builder.Chat(message));
    }

    public void ToggleUnread(bool value)
    {
        if (this.chatModalObj.activeInHierarchy) return;
        this.unreadAlertObj.SetActive(value);
    }

    public void AddMessage(string username, string message)
    {
        GameObject messageObj = Instantiate(this.messageResource, this.contentObj.transform);
        messageObj.transform.SetParent(this.contentObj.transform, false);

        messageObj.transform.Find("Username").GetComponent<Text>().text = username;
        messageObj.transform.Find("Message").GetComponent<Text>().text = message;
        this.ToggleUnread(true);
        StartCoroutine(Scrolldown());
    }

    private IEnumerator Scrolldown()
    {
        yield return new WaitForSeconds(0.15f);
        this.scrollBarObj.GetComponent<Scrollbar>().value = 0;
    }
}