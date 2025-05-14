using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Models;

public class RoomController : MonoBehaviour
{
    public static RoomController instance { get; private set; }
    public Player player;
    public Room room;

    public GameObject teamGround, opponentGround;

    public GameObject playerCardObj, groundObj;

    private IEnumerator turnCoroutine;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    // ###
    // # Room Packets
    // ###
    public void OnRoomJoin(Room room, Player player)
    {
        this.room = room;

        int roomPlayer = this.room.players.IndexOf(this.room.players.Find((p) => p.id == player.id));
        this.room.players[roomPlayer] = player;

        this.player = this.room.players[roomPlayer];
        this.player.cards.ForEach((card) => LoadDeckCard(card, room.status != "pending"));
        InterfaceController.instance.SortDeckCards();

        this.room.sets.ForEach((set) => this.LoadSet(set, true));
        this.room.grounded.ForEach((grounded) => this.LoadGroundCard(grounded, true));

        this.LoadAvatars(this.room, this.player);
        InterfaceController.instance.UpdateScores(this.room.scores);
        this.room.players.FindAll((p) => p.id != this.player.id).ForEach((p) =>
        {
            p.UpdateCardsCount(0);
        });

        InterfaceController.instance.UpdateDeckCardsCount();

        if (this.room.turn == this.player.id)
        {
            this.AddSystemMessage(this.player.id, "chat.turn", "chat.your_turn");
        }
    }

    public void OnRoomIsRunning(Room room)
    {
        this.room.status = room.status;
        this.room.turn = room.turn;
        this.room.turnState = room.turnState;
        this.room.deck_count = room.deck_count;

        StartCoroutine(
            AnimateController.instance.AnimateRoomStart(this.room.players, this.player)
        );

        this.room.players.FindAll((p) => p.id != this.player.id).ForEach((p) =>
        {
            p.cards_count = 15;
            p.UpdateCardsCount(0);
        });

        InterfaceController.instance.UpdateDeckCardsCount();
        this.OnTurnChange(this.room.turn, this.room.turnState, true);
    }

    public void OnRoomTerminate(string reason, List<Score> scores)
    {
        SoundController.instance.victoryAudioSource.Play();
        ScoreboardController.instance.Show(scores, reason);
        StopCoroutine(this.turnCoroutine);
    }

    public void OnPlayerDraw(string playerId, List<Card> drawCards, Nullable<int> groundIndex)
    {
        Player player = this.room.players.Find((p) => p.id == playerId);

        if (this.player.id == playerId)
        {
            this.player.cards = this.player.cards.Concat(drawCards).ToList();
            drawCards.ForEach((card) =>
            {
                LoadDeckCard(card);

                if (!this.room.grounded.Exists((c) => c.number == card.number && c.seed == card.seed) || groundIndex == null)
                {
                    AnimateController.instance.AnimatePlayerDraw(card);
                }
                else
                {
                    AnimateController.instance.AnimatePlayerGroundDraw(card, this.room.grounded);
                }
            });

            InterfaceController.instance.SortDeckCards();
        }
        else
        {
            Player enemy = this.room.players.Find((p) => p.id == playerId);
            enemy.UpdateCardsCount(drawCards.Count());
        }

        int deckCount = groundIndex == null ? 2 : 1;
        InterfaceController.instance.UpdateDeckCardsCount(deckCount);

        if (groundIndex != null)
        {
            this.room.grounded.GetRange((int)groundIndex, this.room.grounded.Count - (int)groundIndex).ForEach((card) =>
            {
                if (this.player.id != player.id)
                {
                    AnimateController.instance.AnimateGroundDraw(card, player, () =>
                    {
                        UnloadGroundCard(card);
                    });
                }
                else
                {
                    UnloadGroundCard(card);
                }
            });

            if (this.player.id != player.id)
            {
                AnimateController.instance.AnimateBackfoldDraw(player, 1);
                this.AddSystemMessage(playerId, "chat.draw_ground");
            }
            else
            {
                this.AddSystemMessage(playerId, "chat.your_draw", "chat.your_draw", drawCards);

            }
        }
        else
        {
            if (this.player.id != player.id)
            {
                AnimateController.instance.AnimateBackfoldDraw(player, 2);
                this.AddSystemMessage(playerId, "chat.draw");
            }
            else
            {
                this.AddSystemMessage(playerId, "chat.your_draw", "chat.your_draw", drawCards);

            }
        }
    }

    public void OnPlayerPutCard(string playerId, string setId, List<Card> cards, Position position, Card replaceCard)
    {
        Player player = this.room.players.Find((p) => p.id == playerId);
        Set set = this.room.sets.Find((s) => s.id == setId);

        if (position == Position.top)
        {
            set.cards = set.cards.Concat(cards).ToList();

            cards.ForEach((card) =>
            {
                this.LoadSetCard(set, card, position);
                AnimateController.instance.AnimateSetCard(card, player, this.player);
            });

            InterfaceController.instance.ReorderSetCards(set);
        }
        else if (position == Position.bottom)
        {
            set.cards = cards.Concat(set.cards).ToList();
            cards.Reverse();
            cards.ForEach((card) =>
            {
                this.LoadSetCard(set, card, position);
                AnimateController.instance.AnimateSetCard(card, player, this.player);
            });

            InterfaceController.instance.ReorderSetCards(set);
        }
        else if (position == Position.replace || position == Position.shift_bottom || position == Position.shift_top)
        {
            Card card = cards.First();
            Card replaceable = this.GetSetReplaceable(set, replaceCard, card);

            replaceable.number = card.number;
            replaceable.seed = card.seed;
            replaceable.obj.GetComponent<CardComponent>().SetCard(replaceable, Card.Type.Set, true, set.id);

            if (position == Position.replace)
            {
                if (this.player.id == playerId)
                {
                    this.player.cards.Add(replaceCard);
                    this.LoadDeckCard(replaceCard);

                    AnimateController.instance.AnimatePlayerReplaceDraw(replaceCard, replaceable);
                    InterfaceController.instance.SortDeckCards();
                }
                else
                {
                    AnimateController.instance.AnimateReplaceDraw(replaceCard, replaceable, player, () => { });
                }
            }
            else if (position == Position.shift_top)
            {
                set.cards = set.cards.Append(replaceCard).ToList();

                this.LoadSetCard(set, replaceCard, Position.top);
                AnimateController.instance.AnimateShiftCard(replaceCard, replaceable);

                InterfaceController.instance.ReorderSetCards(set);

            }
            else if (position == Position.shift_bottom)
            {
                set.cards = set.cards.Prepend(replaceCard).ToList();

                this.LoadSetCard(set, replaceCard, Position.bottom);
                AnimateController.instance.AnimateShiftCard(replaceCard, replaceable);

                InterfaceController.instance.ReorderSetCards(set);

            }

            AnimateController.instance.AnimateSetCard(replaceable, player, this.player);
        }

        if (this.player.id == playerId)
        {
            cards.ForEach((card) =>
            {
                this.UnloadDeckCard(card);
            });
        }
        else if (position != Position.replace)
        {
            player.UpdateCardsCount(-cards.Count());
        }
    }

    public void OnPlayerDrop(string playerId, Card card)
    {
        Player player = this.room.players.Find((p) => p.id == playerId);

        this.room.grounded.Add(card);
        this.LoadGroundCard(card);

        AnimateController.instance.AnimateDropCard(card, player, this.player);

        if (this.player.id == playerId)
        {
            this.UnloadDeckCard(card);
        }
        else
        {
            Player enemy = this.room.players.Find((p) => p.id == playerId);
            enemy.UpdateCardsCount(-1);
        }

        this.UnselectAllCards();
        this.AddSystemMessage(playerId, "chat.drop", "chat.drop", new() { card });
    }

    public void OnPlayerPutSet(string playerId, string setId, List<Card> cards)
    {
        Player player = this.room.players.Find((p) => p.id == playerId);

        int team = this.room.players.Find((p) => p.id == playerId).team;
        Set set = new Set(setId, team, cards);
        this.room.sets.Add(set);
        this.LoadSet(set);

        set.cards.ForEach((card) => AnimateController.instance.AnimateSetCard(card, player, this.player));

        if (playerId == this.player.id)
        {
            cards.ForEach(UnloadDeckCard);
        }
        else
        {
            player.UpdateCardsCount(-cards.Count());
        }
    }

    public void OnTurnChange()
    {
        this.OnTurnChange(this.room.turn, this.room.turnState, true);
    }

    public void OnTurnChange(string playerId, string turnState, bool sound = false)
    {
        this.room.turn = playerId;
        this.room.turnState = turnState;

        if (this.turnCoroutine != null)
        {
            this.StopCoroutine(this.turnCoroutine);
        }

        if (turnState == "draw" && AnimateController.instance.startReady)
        {
            if (this.room.turn == this.player.id)
            {
                this.AddSystemMessage(playerId, "chat.turn", "chat.your_turn");
            }
            else
            {
                this.AddSystemMessage(playerId, "chat.turn");
            }

            if (sound)
            {
                SoundController.instance.ringAudioSource.Play();
            }
        }

        this.room.players.ForEach((p) =>
        {
            p.ToggleTurn(p.id == playerId);

            if (p.id == playerId)
            {
                this.turnCoroutine = p.ReduceTurnTimer();
                StartCoroutine(this.turnCoroutine);
            }
        });
    }

    // ###
    // # Player
    // ###

    public bool IsPlayerTurn(string turnState = null)
    {
        return this.room.turn == this.player.id && (this.room.turnState == turnState || turnState == null);
    }

    // ###
    // # Utils
    // ###

    public List<Card> GetRawHandSelectedCards()
    {
        return new List<Card>(InterfaceController.instance.handSelected.ConvertAll((c) => c));
    }

    public List<Card> GetHandSelectedCards(Set set = null, Nullable<Position> pos = null)
    {
        List<Card> handSelected = new List<Card>(InterfaceController.instance.handSelected.ConvertAll((c) => c));

        // Skips logic if only 1 card selected
        if ((handSelected.Count < 2 && set != null) || (handSelected.Count < 3 && set == null)) return handSelected;

        // Extracts jollies & aces from selected cards
        List<Card> jollies = handSelected.FindAll((c) => c.number == 15);
        List<Card> aces = handSelected.FindAll((c) => c.number == 2 || c.number == 14);

        handSelected = handSelected.FindAll((c) => c.number != 15 && c.number != 2 && c.number != 14);
        handSelected = handSelected.OrderBy(c => c.number).ToList<Card>();

        // Try to order aces
        if (aces.Count > 0)
        {
            aces.ForEach((ace) =>
            {
                if (handSelected.Count > 0)
                {
                    // If ace is needed at bottom
                    if (handSelected[0].number == 3 || (handSelected[0].number == 4 && jollies.Count > 0))
                    {
                        handSelected = handSelected.Prepend(ace).ToList<Card>();
                    }
                    // If ace is needed at top
                    else if (handSelected[^1].number == 13 || (handSelected[^1].number == 12 && jollies.Count > 0))
                    {
                        handSelected = handSelected.Append(ace).ToList<Card>();
                    }
                    // No correct ace position found, probably wrong combination in hands
                    else
                    {
                        handSelected = handSelected.Prepend(ace).ToList<Card>();
                    }
                }
                else
                {
                    handSelected = handSelected.Prepend(ace).ToList<Card>();
                }
            });
        }

        // Try to order jollies
        if (jollies.Count > 0)
        {
            List<int> jollyToAppendIndexes = new List<int>();

            if (set != null)
            {
                // If set is present and needs jolly on top
                if (pos == Position.bottom && set.cards[0].number == (handSelected[^1].number + 2))
                {
                    jollyToAppendIndexes.Add(handSelected.Count);
                }
                // If set is present and needs jolly on bottom
                else if (pos == Position.top && (set.cards[^1].number == (handSelected[0].number - 2) || set.cards[^1].number == 12 && handSelected[0].number == 2))
                {
                    jollyToAppendIndexes.Add(0);
                }
            }

            // Try to put jollies between cards
            for (int i = 0; i < handSelected.Count; i++)
            {
                if (i == 0) continue;
                // If jolly is needed between current card and previous card (because sequence is wrong)
                // Store indexes where to append jollies (we can't touch enumerable indexes during the for loop)
                // Also consider reversed aces with jollies
                if (handSelected[i].number == (handSelected[i - 1].number + 2) || (handSelected[i].number == 4 && handSelected[i - 1].number == 14) || (handSelected[i].number == 2 && handSelected[i - 1].number == 12))
                {
                    jollyToAppendIndexes.Add(i);
                }
            }

            // Insert at stored indexes found jollies
            // Note: consider previous added (that's why we increment index by i)
            if (jollyToAppendIndexes.Count > 0)
            {
                for (int i = 0; i < jollyToAppendIndexes.Count; i++)
                {
                    if (jollies.Count < 1) break;

                    Card jolly = jollies[0];
                    handSelected.Insert(jollyToAppendIndexes[i] + i, jolly);
                    jollies.RemoveAt(0);
                }
            }
        }

        // If there are remaining jollies, try to distribute them
        while (jollies.Count > 0)
        {
            Card jolly = jollies[0];

            // If attaching to set, add to end
            if (set != null)
            {
                handSelected = handSelected.Append(jolly).ToList<Card>();
            }
            else
            {
                bool playerIntentTop = false;
                List<Card> handSelectedIntent = new List<Card>
                    (InterfaceController.instance.handSelected.ConvertAll((c) => c))
                    .FindAll((c) => c.number != 15 && c.number != 2 && c.number != 14);

                // If player intended to append card to top
                if (InterfaceController.instance.handSelected.IndexOf(jolly) > (InterfaceController.instance.handSelected.Count / 2))
                {
                    playerIntentTop = true;
                }

                // Detect default intent ordering
                if (handSelectedIntent.Count > 0 && handSelectedIntent[0].number > handSelectedIntent[^1].number)
                {
                    playerIntentTop = !playerIntentTop;
                }

                // If no set, and first isn't already jolly or ace, and player intent is bottom, prepend
                if (!playerIntentTop && (handSelected[0].number != 15 && handSelected[0].number != 14 && handSelected[0].number != 2))
                {
                    handSelected = handSelected.Prepend(jolly).ToList<Card>();
                }
                // If no set, and last isn't already jolly or ace, and player intent is top, append
                else if (playerIntentTop && (handSelected[^1].number != 15 && handSelected[^1].number != 14 && handSelected[^1].number != 2))
                {
                    handSelected = handSelected.Append(jolly).ToList<Card>();
                }
                // If player intention is not possible, put on most needed point

                else if (handSelected[^1].number > 11)
                {
                    handSelected = handSelected.Prepend(jolly).ToList<Card>();
                }
                else
                {
                    handSelected = handSelected.Append(jolly).ToList<Card>();
                }
            }

            jollies.RemoveAt(0);
        }

        return handSelected;
    }

    public void UnselectAllCards()
    {
        List<Card> selected = new List<Card>(InterfaceController.instance.handSelected);
        selected.ForEach((c) =>
        {
            c.ToggleSelected();
        });
    }

    private void LoadSet(Set set, bool persistent = false)
    {
        GameObject parent = set.team == this.player.team ? this.teamGround : this.opponentGround;
        set.obj = Instantiate(InterfaceController.instance.setResource, parent.transform);
        set.obj.transform.SetParent(parent.transform, false);

        int count = this.room.sets.FindAll((s) => s.team == set.team).Count();
        //parent.GetComponent<HorizontalLayoutGroup>().spacing = this.CalcSetContainerSpacing(count);

        set.cards.ForEach((c) => LoadSetCard(set, c, Position.top, persistent));
        InterfaceController.instance.ReorderSetCards(set);
    }

    private void LoadDeckCard(Card card, bool persistent = false)
    {
        card.obj = Instantiate(InterfaceController.instance.cardResource, playerCardObj.transform);
        card.obj.transform.SetParent(playerCardObj.transform, false);
        card.obj.transform.SetAsFirstSibling();
        card.obj.GetComponent<CardComponent>().SetCard(card, Card.Type.Deck, persistent);
        InterfaceController.instance.OnDeckUpdate();
    }

    private void UnloadDeckCard(Card card)
    {
        Card playerCard = this.player.cards.Find((c) => c.number == card.number && c.seed == card.seed);
        Destroy(playerCard.obj);
        this.player.cards.Remove(playerCard);
        InterfaceController.instance.OnDeckUpdate();
    }

    private void LoadGroundCard(Card card, bool persistent = false)
    {
        card.obj = Instantiate(InterfaceController.instance.cardResource, groundObj.transform);
        card.obj.transform.SetParent(groundObj.transform, false);
        card.obj.transform.SetAsLastSibling();
        card.obj.GetComponent<CardComponent>().SetCard(card, Card.Type.Ground, persistent);
        InterfaceController.instance.OnGroundUpdate();
    }

    private void UnloadGroundCard(Card card)
    {
        Destroy(card.obj);
        this.room.grounded.Remove(card);
        InterfaceController.instance.OnGroundUpdate();
    }

    private void LoadSetCard(Set set, Card card, Position position, bool persistent = false)
    {
        card.obj = Instantiate(InterfaceController.instance.cardResource, set.obj.transform);
        card.obj.transform.SetParent(set.obj.transform, false);

        if (position == Position.top)
        {
            card.obj.transform.SetAsFirstSibling();
        }
        else if (position == Position.bottom)
        {
            card.obj.transform.SetAsLastSibling();
        }

        //set.obj.GetComponent<VerticalLayoutGroup>().spacing = CalcSetSpacing(set.cards.Count());
        card.obj.GetComponent<CardComponent>().SetCard(card, Card.Type.Set, persistent, set.id);
    }

    private void AddSystemMessage(string playerId, string chatEvt, string tooltipEvt = null, List<Card> cards = null)
    {
        string chatCards = "";

        Player player = this.room.players.Find((p) => p.id == playerId);
        if (player == null) return;
        if (tooltipEvt == null) tooltipEvt = chatEvt;
        if (cards != null)
        {
            string[] utfCards = cards.ConvertAll((c) => c.GetChatValue()).ToArray();
            chatCards = String.Join(" ", utfCards.ToArray());
        }

        string chatMessage = TranslateService.instance.Translate(chatEvt, new() { player.username, chatCards });
        string tooltipMessage = TranslateService.instance.Translate(tooltipEvt, new() { player.username, chatCards });

        ChatController.instance.AddMessage("Pinex", chatMessage);
        InterfaceController.instance.LoadTooltip(tooltipMessage, false);
    }

    private void LoadAvatars(Room room, Player player)
    {
        GameObject userAvatar = GameObject.Find("PlayerTab/Player");
        GameObject teamAvatar = GameObject.Find("Avatars/Team");
        GameObject opponentAvatar = GameObject.Find("Avatars/Opponent");
        GameObject opponentTeamAvatar = GameObject.Find("Avatars/OpponentTeam");

        Player teamPlayer = this.room.players.Find((p) => p.id != player.id && p.team == player.team);
        List<Player> opponentPlayers = this.room.players.FindAll((p) => p.id != player.id && p.team != player.team);

        teamAvatar.SetActive(false);
        opponentTeamAvatar.SetActive(false);

        opponentPlayers[0].SetAvatar(opponentAvatar);
        if (teamPlayer != null) teamPlayer.SetAvatar(teamAvatar);
        if (opponentPlayers.Count > 1) opponentPlayers[1].SetAvatar(opponentTeamAvatar);

        this.room.players.ForEach((player) =>
        {
            StartCoroutine(AvatarService.instance.Fetch(player.avatar,
                (result) =>
                {
                    if (!player.avatarObj) return;
                    player.avatarObj.transform.Find("Profile/AvatarFrame/Avatar").GetComponent<RawImage>().texture = result as Texture2D;
                }, (error) =>
                {
                }, (end) => { }
            ));

            if (player.id != this.player.id)
            {
                player.avatarObj.transform.Find("Profile/AvatarFrame").GetComponent<Button>().onClick.AddListener(() =>
                {
                    PlayerProfileController.instance.Show(player);
                });
            }
        });

        this.OnTurnChange(room.turn, room.turnState);
    }

    public Card GetSetReplaceable(Set set, Card toReplace, Card replacedBy)
    {
        return set.cards.Find((r) =>
            r.number == toReplace.number && (
                (set.cards.IndexOf(r) != 0 && set.cards[set.cards.IndexOf(r) - 1].number == replacedBy.number - 1) ||
                (set.cards.Count - 1 >= set.cards.IndexOf(r) + 1 && set.cards[set.cards.IndexOf(r) + 1].number == replacedBy.number + 1) ||
                (set.cards.IndexOf(r) != 0 && set.cards[set.cards.IndexOf(r) - 1].number == 13 && replacedBy.number == 2) ||
                (set.cards.Count - 1 >= set.cards.IndexOf(r) + 1 && set.cards[set.cards.IndexOf(r) + 1].number == 3 && replacedBy.number == 14)
            )
        );
    }
}
