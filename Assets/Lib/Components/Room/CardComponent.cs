using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Models;
using System.Linq;

public class CardComponent : MonoBehaviour
{
    public Card card;

    // ###
    // # Constructor
    // ###

    public void SetCard(Card card, Card.Type type, bool persistent = false, string setId = null)
    {
        this.card = card;
        this.card.type = type;
        this.card.setId = setId;

        Texture2D cardTexture = Resources.Load($@"Cards/{card.seed}/{card.number}") as Texture2D;
        this.transform.GetComponent<RawImage>().texture = cardTexture;
        this.SetPersistent(persistent);

        this.GetComponent<Button>().onClick.AddListener(HandleClick);
    }


    // ###
    // # Utils
    // ###

    public void SetPersistent(bool persistent = true)
    {
        if (persistent)
        {
            this.GetComponent<RawImage>().color = new Color32(255, 255, 255, 255);
        }
        else
        {
            this.GetComponent<RawImage>().color = new Color32(255, 255, 255, 0);
        }
    }

    private bool IsReplaceable(Set set, Card card, Card jolly)
    {
        return RoomController.instance.GetSetReplaceable(set, jolly, card) == null ? false : true;
    }

    // ###
    // # Actions
    // ###

    private void HandleClick()
    {
        if (this.card.type == Card.Type.Ground)
        {
            if (RoomController.instance.IsPlayerTurn("draw"))
            {
                int cardIndex = RoomController.instance.room.grounded.IndexOf(this.card);
                SocketService.instance.RoomPush("draw_card", Packets.Builder.DrawCard(cardIndex));
            }
            else if (RoomController.instance.IsPlayerTurn("drop")) InputController.instance.DropCard();
            else InterfaceController.instance.LoadTooltip("room.tooltip.invalid_turn");
        }
        else if (this.card.type == Card.Type.Deck)
        {
            if (RoomController.instance.IsPlayerTurn("drop")) this.card.ToggleSelected();
            else if (RoomController.instance.IsPlayerTurn("draw")) InterfaceController.instance.LoadTooltip("room.tooltip.must_draw");
            else InterfaceController.instance.LoadTooltip("room.tooltip.invalid_turn");
        }
        else if (this.card.type == Card.Type.Set)
        {
            Set set = RoomController.instance.room.sets.Find((s) => s.id == this.card.setId);
            List<Card> cards = RoomController.instance.GetRawHandSelectedCards();

            if (!RoomController.instance.IsPlayerTurn("drop"))
            {
                if (!RoomController.instance.IsPlayerTurn("draw")) InterfaceController.instance.LoadTooltip("room.tooltip.invalid_turn");
                else InterfaceController.instance.LoadTooltip("room.tooltip.must_draw");

                return;
            }

            if (this.card.IsJolly() && cards.Count == 1 && this.IsReplaceable(set, cards.First(), this.card))
            {
                SocketService.instance.RoomPush("put_card", Packets.Builder.PutCard(cards, set.id, Position.replace));
                RoomController.instance.UnselectAllCards();
            }
            else if (set.team == RoomController.instance.player.team)
            {
                if (set.cards.IndexOf(this.card) > (set.cards.Count / 2))
                {
                    List<Card> pushCards = RoomController.instance.GetHandSelectedCards(set, Position.top);
                    SocketService.instance.RoomPush("put_card", Packets.Builder.PutCard(pushCards, set.id, Position.top));
                    RoomController.instance.UnselectAllCards();
                }
                else
                {
                    List<Card> pushCards = RoomController.instance.GetHandSelectedCards(set, Position.bottom);
                    SocketService.instance.RoomPush("put_card", Packets.Builder.PutCard(pushCards, set.id, Position.bottom));
                    RoomController.instance.UnselectAllCards();
                }
            }
        }
    }
}