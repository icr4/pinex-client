using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using Models;
using System.Linq;

public class InputController : MonoBehaviour
{
    public static InputController instance { get; private set; }

    public Button drawBtn;
    public GameObject groundObj, setObj;

    void Start()
    {
        this.drawBtn.onClick.AddListener(DrawCard);
        this.groundObj.GetComponent<Button>().onClick.AddListener(DropCard);
        this.setObj.GetComponent<Button>().onClick.AddListener(PutSet);
    }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void LeaveRoom(System.Action onEnd = null)
    {
        AdsService.instance.showInterstitial = true;
        SocketService.instance.roomChannel.Leave();
        SocketService.instance.roomChannel = null;
        ClientService.instance.LoadLobby(onEnd);
    }

    private void DrawCard()
    {
        // Prevent drawing before animations are completed
        if (!AnimateController.instance.startReady) return;

        if (RoomController.instance.IsPlayerTurn("draw"))
        {

            RoomController.instance.UnselectAllCards();
            SocketService.instance.RoomPush("draw_card", Packets.Builder.DrawCard(null));
        }
        else if (RoomController.instance.IsPlayerTurn("drop")) InterfaceController.instance.LoadTooltip("room.tooltip.already_drawn");
        else InterfaceController.instance.LoadTooltip("room.tooltip.invalid_turn");
    }

    public void DropCard()
    {
        List<Card> cards = RoomController.instance.GetHandSelectedCards();

        if (!RoomController.instance.IsPlayerTurn("drop")) InterfaceController.instance.LoadTooltip("room.tooltip.invalid_turn");
        else if (cards.Count > 1) InterfaceController.instance.LoadTooltip("room.tooltip.drop_one");
        else if (cards.Count < 1) InterfaceController.instance.LoadTooltip("room.tooltip.already_drawn");
        else
        {
            SocketService.instance.RoomPush("drop_card", Packets.Builder.DropCard(cards.First()));
            RoomController.instance.UnselectAllCards();
        }
    }

    public void PutSet()
    {
        List<Card> cards = RoomController.instance.GetHandSelectedCards();

        if (cards.Count < 1) return;
        else if (!RoomController.instance.IsPlayerTurn("drop")) InterfaceController.instance.LoadTooltip("room.tooltip.invalid_turn");
        else if (cards.Count < 3) InterfaceController.instance.LoadTooltip("room.tooltip.set_min");
        else
        {
            SocketService.instance.RoomPush("put_set", Packets.Builder.PutSet(cards));
            RoomController.instance.UnselectAllCards();
        }
    }
}