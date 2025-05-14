using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Models;
using System.Collections.Generic;

public class AnimateController : MonoBehaviour
{
    public static AnimateController instance { get; private set; }
    private GameObject backfoldResource;
    private Vector3 deckPosition;

    public bool startReady = true;

    void Start()
    {
        this.deckPosition = GameObject.Find("PlayerTab/Draw/Button").transform.position;
        this.backfoldResource = Resources.Load("Prefabs/Backfold", typeof(GameObject)) as GameObject;
        this.startReady = true;
    }

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    //
    // Animate room start cards from deck to avatars (if enemies) or player cards (if player)
    //
    public IEnumerator AnimateRoomStart(List<Player> players, Player player)
    {
        int i = 0;
        this.startReady = false;

        while (i < 15)
        {
            players.ForEach((p) =>
            {
                if (p.id == player.id)
                {
                    this.AnimatePlayerDraw(player.cards[i]);
                }
                else
                {
                    this.AnimateBackfoldDraw(p, 1);
                }
            });

            i += 1;
            yield return new WaitForSeconds(0.4f);
        }

        this.startReady = true;
        RoomController.instance.OnTurnChange();
    }

    //
    // Animate cards from deck to player cards
    //
    public void AnimatePlayerDraw(Card card)
    {
        card.obj.GetComponent<Button>().interactable = false;
        card.obj.GetComponent<RawImage>().maskable = false;

        StartCoroutine(LerpCard(card, this.deckPosition));
    }

    //
    // Animate cards from ground to player cards
    //
    public void AnimatePlayerGroundDraw(Card card, List<Card> grounded)
    {
        card.obj.GetComponent<Button>().interactable = false;
        card.obj.GetComponent<RawImage>().maskable = false;
        Vector3 from = grounded.Find((c) => c.number == card.number && c.seed == card.seed).obj.transform.position;

        StartCoroutine(LerpCard(card, from));
    }

    //
    // Animate card from set to player cards
    //
    public void AnimatePlayerReplaceDraw(Card card, Card setCard)
    {
        card.obj.GetComponent<Button>().interactable = false;
        card.obj.GetComponent<RawImage>().maskable = false;
        Vector3 from = setCard.obj.transform.position;

        StartCoroutine(LerpCard(card, from));
    }

    //
    // Animate card from set to avatar
    //
    public void AnimateReplaceDraw(Card replaceCard, Card replaceable, Player player, System.Action onEnd)
    {
        GameObject card = InstantiateCard(replaceCard.seed, replaceCard.number);
        card.GetComponent<Button>().interactable = false;
        card.GetComponent<RawImage>().maskable = false;
        card.transform.position = replaceable.obj.transform.position;

        Vector3 from = card.transform.position;
        Vector3 to = player.avatarObj.transform.position;
        StartCoroutine(LerpTransform(card.transform, from, to, true, onEnd));
    }

    //
    // Animate cards from ground to avatar
    //
    public void AnimateGroundDraw(Card card, Player player, System.Action onEnd)
    {
        card.obj.GetComponent<Button>().interactable = false;
        //card.obj.GetComponent<RawImage>().maskable = false;

        Vector3 from = card.obj.transform.position;
        Vector3 to = player.avatarObj.transform.position;
        StartCoroutine(LerpTransform(card.obj.transform, from, to, false, onEnd));
    }

    //
    // Animate cards from deck to avatar (backfolded)
    //
    public void AnimateBackfoldDraw(Player player, int count)
    {
        StartCoroutine(
            Repeat(count, () =>
            {
                GameObject card = InstantiateBackfold();
                card.transform.position = this.deckPosition;
                Vector3 to = player.avatarObj.transform.position;

                StartCoroutine(
                    LerpTransform(card.transform, card.transform.position, to, true)
                );
            }
        ));
    }

    //
    // Animate cards from set to set (shift)
    //
    public void AnimateShiftCard(Card shiftCard, Card replacedCard)
    {
        shiftCard.obj.GetComponent<Button>().interactable = false;
        Vector3 from = replacedCard.obj.transform.position;

        StartCoroutine(LerpCard(shiftCard, from));
    }

    //
    // Animate cards from deck (if player) or avatar (if enemy) to set
    //
    public void AnimateSetCard(Card card, Player player, Player current)
    {
        card.obj.GetComponent<Button>().interactable = false;
        //card.obj.GetComponent<RawImage>().maskable = false;
        Vector3 from = new Vector3(0, 0, 0);

        if (current.id != player.id)
        {
            from = player.avatarObj.transform.position;
        }
        else
        {
            from = current.cards.Find((c) => c.seed == card.seed && c.number == card.number).obj.transform.position;
        }

        StartCoroutine(LerpCard(card, from));
    }

    //
    // Animate cards from deck (if player) or avatar (if enemy) to ground
    //
    public void AnimateDropCard(Card card, Player player, Player current)
    {
        card.obj.GetComponent<Button>().interactable = false;
        card.obj.GetComponent<RawImage>().maskable = false;
        Vector3 from = new Vector3(0, 0, 0);

        if (current.id != player.id)
        {
            from = player.avatarObj.transform.position;
        }
        else
        {
            from = current.cards.Find((c) => c.seed == card.seed && c.number == card.number).obj.transform.position;
        }

        StartCoroutine(LerpCard(card, from));
    }

    //
    // Instantiates Backfold Transform
    //
    private GameObject InstantiateBackfold()
    {
        Transform parent = GameObject.Find("AnimatedContainer").transform;
        GameObject card = Instantiate(this.backfoldResource, parent.transform);
        card.gameObject.transform.SetParent(parent, false);

        return card;
    }

    //
    // Instantiates Backfold Transform
    //
    private GameObject InstantiateCard(int seed, int number)
    {
        Transform parent = GameObject.Find("AnimatedContainer").transform;
        GameObject card = Instantiate(InterfaceController.instance.cardResource, parent.transform);
        card.gameObject.transform.SetParent(parent, false);
        card.GetComponent<CardComponent>().SetCard(new Card(seed, number), Card.Type.Animate);

        return card;
    }

    //
    // Coroutines used to LERP cards transitions
    //
    private IEnumerator LerpCard(Card card, Vector3 from)
    {
        float t = 0;
        SoundController.instance.cardAudioSource.Play();

        yield return new WaitForSeconds(0.005f);
        Vector3 to = card.obj.transform.position;

        while (t < 1)
        {
            t += 0.1f;
            Vector3 pos = Vector2.Lerp(from, to, t);
            card.obj.transform.position = pos;
            card.obj.GetComponent<CardComponent>().SetPersistent();

            yield return new WaitForSeconds(0.05f);
        }

        if (card.obj != null)
        {
            card.obj.GetComponent<Button>().interactable = true;
            card.obj.GetComponent<RawImage>().maskable = true;
        }

        yield break;
    }

    private IEnumerator LerpTransform(Transform card, Vector3 from, Vector3 to, bool destroy = false, System.Action onEnd = null)
    {
        float t = 0;

        SoundController.instance.cardAudioSource.Play();

        while (t < 1)
        {
            if (card == null) yield break;

            t += 0.1f;
            Vector3 pos = Vector2.Lerp(from, to, t);
            card.position = pos;

            if (card.GetComponent<CardComponent>() != null)
            {
                card.GetComponent<CardComponent>().SetPersistent();
            }

            yield return new WaitForSeconds(0.05f);
        }

        if (destroy && card != null) GameObject.Destroy(card.gameObject);
        if (onEnd != null) onEnd();
        yield break;
    }

    private IEnumerator Repeat(int count, System.Action action)
    {
        int i = 0;

        while (i < count)
        {
            i += 1;
            action();
            yield return new WaitForSeconds(0.4f);
        }

        yield break;
    }
}