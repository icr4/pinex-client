using System.Linq;
using System.Collections;
using System.Collections.Generic;
using Models;
using UnityEngine;
using UnityEngine.UI;
using System;
using Unity.VisualScripting;

public class TutorialController : MonoBehaviour
{
    public static TutorialController instance { get; private set; }

    public enum Position { top, bottom, replace };
    public Button drawBtn, leaveBtn;
    public GameObject playerCardObj, groundObj, setObj;
    public GameObject teamGround, opponentGround;

    public Player player, opponent;
    public List<Card> grounded = new();
    private Set teamSet;
    private string step = "start";

    public GameObject modalObj, deckHighlight, drawHighlight, setHighlight;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        // Instantiates fake opponent
        this.opponent = new Player();
        opponent.id = "opponent";
        opponent.cards_count = 15;
        opponent.SetAvatar(GameObject.Find("Avatars/Opponent"));

        // Instantiates fake player
        this.player = new Player();
        player.cards = new();
        player.id = "current";

        // Add listeners for player's actions
        this.drawBtn.onClick.AddListener(DrawCard);
        this.leaveBtn.onClick.AddListener(Leave);
        this.groundObj.GetComponent<Button>().onClick.AddListener(DropCard);
        this.setObj.GetComponent<Button>().onClick.AddListener(PutSet);

        // Initialize player's deck
        this.SetModal();
        this.SetStep("init");
    }

    private void SetModal(string text = null, string actionName = null, System.Action action = null)
    {
        this.modalObj.SetActive(false);
        this.modalObj.transform.Find("Body/Text").GetComponent<Text>().text = "";
        this.modalObj.transform.Find("Body/Button").gameObject.SetActive(false);
        this.modalObj.transform.Find("Body/Button").GetComponent<Button>().onClick.RemoveAllListeners();

        if (text != null)
        {
            string translatedText = TranslateService.instance.Translate($"tutorial.info.{text}");
            this.modalObj.transform.Find("Body/Text").GetComponent<Text>().text = translatedText;

            if (action != null && actionName != null)
            {
                string translatedAction = TranslateService.instance.Translate($"tutorial.btn.{actionName}");
                this.modalObj.transform.Find("Body/Button/Text").GetComponent<Text>().text = translatedAction;
                this.modalObj.transform.Find("Body/Button").GetComponent<Button>().onClick.AddListener(() => action());
                this.modalObj.transform.Find("Body/Button").gameObject.SetActive(true);
            }

            this.modalObj.SetActive(true);
        }
    }

    private void DisableHighlight()
    {
        this.drawHighlight.SetActive(false);
        this.deckHighlight.SetActive(false);
        this.setHighlight.SetActive(false);

        Resources.FindObjectsOfTypeAll<CardComponent>()
            .ToList<CardComponent>()
            .ForEach((c) =>
            {
                if (c.card != null)
                {
                    c.card.obj.transform.Find("Highlight").gameObject.SetActive(false);
                }
            });

        this.StopAllCoroutines();
    }

    private void SetHighlight(GameObject obj, float min = 0.4f, float max = 0.8f)
    {

        obj.SetActive(true);
        obj.GetComponent<RawImage>().color = new Color(1f, 1f, 1f, 1f);
        StartCoroutine(HighlightCoroutine(obj, true, min, max));
    }

    private void SetCardHighlight(GameObject card)
    {
        GameObject obj = card.transform.Find("Highlight").gameObject;
        obj.GetComponent<RawImage>().color = new Color(1f, 1f, 1f, 0.4f);
        this.SetHighlight(obj, 0.1f, 0.3f);
    }

    private IEnumerator HighlightCoroutine(GameObject obj, bool decr, float min, float max)
    {
        while (true)
        {
            RawImage img = obj.GetComponent<RawImage>();
            float opacity = img.color.a;

            if (opacity >= min && decr)
            {
                img.color = new Color(1f, 1f, 1f, opacity - 0.05f);
            }
            else if (opacity <= max && !decr)
            {
                img.color = new Color(1f, 1f, 1f, opacity + 0.05f);
            }
            else
            {
                StartCoroutine(HighlightCoroutine(obj, !decr, min, max));
                yield break;
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    private void SetStep(string step)
    {
        this.step = step;

        // Disable modal
        this.SetModal();
        this.DisableHighlight();

        if (step == "init")
        {
            this.SetModal("init_1", "continue", () =>
            {
                this.SetModal("init_2", "continue", () =>
                {
                    this.SetModal("init_3", "continue", () =>
                    {
                        this.SetModal("init_4", "start", () =>
                        {
                            this.DistributeDeck();
                        });
                    });
                });
            });
        }
        else if (step == "draw")
        {
            this.SetModal("draw");
            this.SetHighlight(this.drawHighlight);

            SoundController.instance.ringAudioSource.Play();
        }
        else if (step == "select")
        {
            this.SetModal("select");
            this.player.cards.FindAll((c) => c.seed == 1 && c.number <= 8).ForEach((c) =>
            {
                this.SetCardHighlight(c.obj);
            });
        }
        else if (step == "select_drop")
        {
            this.player.cards.FindAll((c) => c.seed == 1 && c.number == 10).ForEach((c) =>
            {
                this.SetCardHighlight(c.obj);
            });
            this.SetModal("select_drop");
        }
        else if (step == "enemy_draw")
        {
            this.SetModal("enemy_draw", "continue", () =>
            {
                SoundController.instance.ringAudioSource.Play();

                StartCoroutine(ClientService.instance.TimeoutCoroutine(2, () =>
                {
                    this.EnemyDraw();
                }));

                this.SetModal();
            });
        }
        else if (step == "select_2")
        {
            SoundController.instance.ringAudioSource.Play();

            this.SetModal("select_2");
            this.grounded.FindAll((c) => c.seed == 1 && c.number == 10).ForEach((c) =>
            {
                this.SetCardHighlight(c.obj);
            });
        }
        else if (step == "select_replace")
        {
            this.SetModal("select_replace", "continue", () =>
            {
                this.SetModal("select_replace_2", "continue", () =>
                {
                    this.SetModal("select_replace_3", "close", () =>
                    {
                        this.SetModal();

                        this.player.cards.FindAll((c) => c.seed == 2 && c.number == 10).ForEach((c) =>
                        {
                            this.SetCardHighlight(c.obj);
                        });
                    });
                });
            });
        }
        else if (step == "select_3")
        {
            this.SetModal("select_3", "continue", () =>
            {
                this.SetModal("select_3_2");

                this.player.cards.FindAll((c) => c.seed == 1 && c.number >= 10 || c.seed == 5).ForEach((c) =>
                {
                    this.SetCardHighlight(c.obj);
                });
            });
        }
        else if (step == "select_5")
        {
            this.SetModal("select_5");

            this.player.cards.FindAll((c) => c.seed == 3 && c.number == 7).ForEach((c) =>
            {
                this.SetCardHighlight(c.obj);
            });
        }
        else if (step == "end")
        {
            SoundController.instance.victoryAudioSource.Play();

            this.SetModal("end_1", "continue", () =>
            {
                this.SetModal("end_2", "continue", () =>
                {
                    this.SetModal("end_3", "play", () =>
                    {
                        ClientService.instance.LoadLobby();
                    });
                });
            });
        }
    }

    private void DistributeDeck()
    {
        List<Card> cards = new() { new Card(1, 5), new Card(1, 6), new Card(1, 7), new Card(2, 10) };
        cards.ForEach((c) =>
        {
            LoadDeckCard(c);
            AnimateController.instance.AnimatePlayerDraw(c);
        });

        AnimateController.instance.AnimateBackfoldDraw(this.opponent, cards.Count);
        InterfaceController.instance.UpdateDeckCardsCount(74);

        this.SetStep("draw");
    }

    private void DrawCard()
    {
        if (step == "draw")
        {
            List<Card> cards = new() { new Card(1, 8), new Card(1, 10) };
            cards.ForEach((c) =>
            {
                LoadDeckCard(c);
                AnimateController.instance.AnimatePlayerDraw(c);
            });

            InterfaceController.instance.UpdateDeckCardsCount(72);
            this.SetStep("select");
        }
    }

    private void DrawGroundCard()
    {
        if (step == "draw_ground")
        {
            List<Card> groundCards = new() { new Card(1, 10), new Card(3, 7) };
            groundCards.ForEach((c) =>
            {
                LoadDeckCard(c);
                AnimateController.instance.AnimatePlayerGroundDraw(c, this.grounded);
            });

            Card card = new Card(1, 11);
            LoadDeckCard(card);
            AnimateController.instance.AnimatePlayerDraw(card);

            new List<Card>(this.grounded.ConvertAll((c) => c)).ForEach(UnloadGroundCard);
            InterfaceController.instance.UpdateDeckCardsCount(69);
            this.SetStep("select_replace");
        }
    }

    private void DropCard()
    {
        if (this.step == "drop")
        {
            Card card = new Card(1, 10);
            LoadGroundCard(card);
            AnimateController.instance.AnimateDropCard(card, this.player, this.player);
            UnloadDeckCard(this.player.cards.Find((c) => c.number == 10 && c.seed == 1));

            this.DisableHighlight();
            this.SetStep("enemy_draw");
        }
        else if (this.step == "drop_5")
        {
            Card card = new Card(3, 7);
            LoadGroundCard(card);
            AnimateController.instance.AnimateDropCard(card, this.player, this.player);
            UnloadDeckCard(this.player.cards.Find((c) => c.seed == 3 && c.number == 7));

            this.DisableHighlight();
            this.SetStep("end");
        }
    }

    private void ReplaceCard()
    {
        if (this.step == "replace_card")
        {
            Card replaceCard = Resources.FindObjectsOfTypeAll<CardComponent>()
                .ToList<CardComponent>()
                .Find((c) => c.card.number == 15 && c.card.seed == 5).card;

            Card card = new Card(5, 15);
            LoadDeckCard(card);

            Card replaceable = this.player.cards.Find((c) => c.number == 10 && c.seed == 2);

            AnimateController.instance.AnimatePlayerReplaceDraw(card, replaceCard);

            replaceCard.number = replaceable.number;
            replaceCard.seed = replaceable.seed;
            replaceCard.obj.GetComponent<CardComponent>().SetCard(replaceCard, Card.Type.Set, true, "2");

            AnimateController.instance.AnimateSetCard(replaceCard, this.player, this.player);

            this.UnloadDeckCard(replaceable);
            this.SetStep("select_3");
        }
    }

    private void PutSet()
    {
        if (this.step == "put_set")
        {
            List<Card> setCards = this.player.cards.FindAll((c) => new List<int> { 5, 6, 7, 8 }.Contains(c.number));
            teamSet = new Set("1", 0, new() { new Card(1, 5), new Card(1, 6), new Card(1, 7), new Card(1, 8) });
            this.LoadSet(teamSet, this.player);
            this.SetStep("select_drop");
            setCards.ForEach(UnloadDeckCard);
        }
    }

    private void PutCards()
    {
        if (this.step == "put_4")
        {
            List<Card> setCards = new() { new Card(5, 15), new Card(1, 10), new Card(1, 11) };
            setCards.ForEach((c) =>
            {
                this.LoadSetCard(this.teamSet, c, Position.top);
                AnimateController.instance.AnimateSetCard(c, this.player, this.player);
            });

            List<Card> playerCards = this.player.cards.FindAll((c) => new List<int> { 15, 10, 11 }.Contains(c.number));
            playerCards.ForEach(UnloadDeckCard);
            this.SetStep("select_5");
        }
    }

    private void EnemyDraw()
    {
        AnimateController.instance.AnimateBackfoldDraw(this.opponent, 2);
        InterfaceController.instance.UpdateDeckCardsCount(70);
        this.opponent.UpdateCardsCount(2);
        this.step = "enemy_put_set";

        StartCoroutine(ClientService.instance.TimeoutCoroutine(2, () =>
        {
            this.EnemyPutSet();
        }));
    }

    private void EnemyPutSet()
    {
        this.step = "enemy_drop_card";
        Set set = new Set("2", 1, new() { new Card(2, 8), new Card(2, 9), new Card(5, 15), new Card(2, 11) });
        this.LoadSet(set, this.opponent);

        StartCoroutine(ClientService.instance.TimeoutCoroutine(2, () =>
        {
            this.EnemyDropCard();
        }));

        this.opponent.UpdateCardsCount(-4);
    }

    private void EnemyDropCard()
    {
        Card card = new Card(3, 7);
        LoadGroundCard(card);
        AnimateController.instance.AnimateDropCard(card, this.opponent, this.player);
        this.opponent.UpdateCardsCount(-1);
        this.SetStep("select_2");
    }

    private void HandleCardClick(Card card)
    {
        if (this.step == "select" && new List<int> { 5, 6, 7, 8 }.Contains(card.number) && card.seed == 1)
        {
            card.ToggleSelected();

            if (this.ListSelected().Count == 4)
            {
                this.step = "put_set";
                this.DisableHighlight();
                this.SetHighlight(this.setHighlight);
            }
            else
            {
                this.step = "select";
            }
        }
        else if (this.step == "select_drop" && card.number == 10)
        {
            this.DisableHighlight();
            this.SetHighlight(this.deckHighlight);
            card.ToggleSelected();
            this.step = "drop";
        }
        else if (this.step == "select_2" && card.seed == 1 && card.number == 10)
        {
            this.DisableHighlight();
            this.step = "draw_ground";
            this.DrawGroundCard();
        }
        else if (this.step == "select_replace" && card.seed == 2 && card.number == 10)
        {
            card.ToggleSelected();
            this.step = "replace_card";

            Resources.FindObjectsOfTypeAll<CardComponent>()
                .ToList<CardComponent>()
                .FindAll((c) => c.card != null && c.card.seed == 5 && c.card.number == 15).ForEach((c) =>
                {
                    this.SetCardHighlight(c.card.obj);
                });
        }
        else if (this.step == "replace_card" && card.seed == 5 && card.number == 15)
        {
            this.DisableHighlight();
            this.ReplaceCard();
        }
        else if (this.step == "select_3" && (card.number == 15 || (card.seed == 1 && new List<int> { 10, 11 }.Contains(card.number))))
        {
            card.ToggleSelected();

            if (this.ListSelected().Count == 3)
            {
                this.DisableHighlight();
                Resources.FindObjectsOfTypeAll<CardComponent>()
                .ToList<CardComponent>()
                .FindAll((c) => c.card != null && c.card.seed == 1 && c.card.number == 8).ForEach((c) =>
                {
                    this.SetCardHighlight(c.card.obj);
                });
                this.step = "put_3";
            }
            else
            {
                this.step = "select_3";
            }
        }
        else if (this.step == "put_3" && card.number == 8 && card.seed == 1)
        {
            this.step = "put_4";
            this.DisableHighlight();
            this.PutCards();
        }
        else if (this.step == "select_5" && card.number == 7)
        {
            this.SetHighlight(this.deckHighlight);
            card.ToggleSelected();
            this.step = "drop_5";
        }
    }

    private List<Card> ListSelected()
    {
        return this.player.cards.FindAll((c) => c.handSelected);
    }

    // Deck Cards

    private void LoadDeckCard(Card card)
    {
        this.player.cards.Add(card);

        card.obj = Instantiate(InterfaceController.instance.cardResource, playerCardObj.transform);
        card.obj.transform.SetParent(playerCardObj.transform, false);
        card.obj.transform.SetAsFirstSibling();
        card.obj.GetComponent<CardComponent>().SetCard(card, Card.Type.Deck);
        card.obj.GetComponent<Button>().onClick.RemoveAllListeners();
        card.obj.GetComponent<Button>().onClick.AddListener(() => HandleCardClick(card));
        InterfaceController.instance.OnDeckUpdate();
        InterfaceController.instance.SortDeckCards();
    }

    private void UnloadDeckCard(Card card)
    {
        Card playerCard = this.player.cards.Find((c) => c.number == card.number && c.seed == card.seed);
        Destroy(playerCard.obj);
        this.player.cards.Remove(playerCard);
        InterfaceController.instance.OnDeckUpdate();
    }

    // Set Cards

    private void LoadSet(Set set, Player player)
    {
        GameObject parent = set.team == 0 ? this.teamGround : this.opponentGround;
        set.obj = Instantiate(InterfaceController.instance.setResource, parent.transform);
        set.obj.transform.SetParent(parent.transform, false);

        set.cards.ForEach((c) =>
        {
            LoadSetCard(set, c, Position.top);
            AnimateController.instance.AnimateSetCard(c, player, this.player);
        });
    }

    private void LoadSetCard(Set set, Card card, Position position)
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

        card.obj.GetComponent<CardComponent>().SetCard(card, Card.Type.Set, false, set.id);
        card.obj.GetComponent<Button>().onClick.RemoveAllListeners();
        card.obj.GetComponent<Button>().onClick.AddListener(() => HandleCardClick(card));
    }

    //
    // Grounded Cards
    //

    private void LoadGroundCard(Card card)
    {
        this.grounded.Add(card);
        card.obj = Instantiate(InterfaceController.instance.cardResource, groundObj.transform);
        card.obj.transform.SetParent(groundObj.transform, false);
        card.obj.transform.SetAsLastSibling();
        card.obj.GetComponent<CardComponent>().SetCard(card, Card.Type.Ground);
        card.obj.GetComponent<Button>().onClick.RemoveAllListeners();
        card.obj.GetComponent<Button>().onClick.AddListener(() => HandleCardClick(card));

        InterfaceController.instance.OnGroundUpdate();
    }

    private void UnloadGroundCard(Card card)
    {
        Destroy(card.obj);
        this.grounded.Remove(card);
        InterfaceController.instance.OnGroundUpdate();
        InterfaceController.instance.SortDeckCards();
    }

    //
    // Misc
    //

    void Leave()
    {
        ConfirmService.instance.Show(
            TranslateService.instance.Translate("room.settings.confirm.leave"),
            () => { },
            () =>
            {
                ClientService.instance.LoadLobby();
            });
    }
}