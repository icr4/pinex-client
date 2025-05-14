using System.Linq;
using System.Collections.Generic;
using Models;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class InterfaceController : MonoBehaviour
{
    public static InterfaceController instance { get; private set; }
    public List<Card> handSelected = new();
    public GameObject cardResource, setResource, tooltipObj, scoresObj;
    private IEnumerator tooltipCoroutine;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    void Start()
    {
        this.cardResource = Resources.Load("Prefabs/Card", typeof(GameObject)) as GameObject;
        this.setResource = Resources.Load("Prefabs/Set", typeof(GameObject)) as GameObject;
    }

    public void UpdateDeckCardsCount(int value = 0)
    {
        Transform countObj;
        int count;

        if (gameObject.GetComponent<RoomController>() != null)
        {
            count = RoomController.instance.room.deck_count -= value;
            countObj = InputController.instance.drawBtn.transform.Find("Count").transform;
        }
        else
        {
            countObj = TutorialController.instance.drawBtn.transform.Find("Count").transform;
            count = value;
        }

        countObj.GetComponent<Text>().text = $"{count}";
        countObj.gameObject.SetActive(true);
    }

    public void OnGroundUpdate()
    {
        int childCount;
        HorizontalLayoutGroup groundObj;

        if (gameObject.GetComponent<RoomController>() != null)
        {
            childCount = RoomController.instance.room.grounded.Count;
            groundObj = RoomController.instance.groundObj.GetComponent<HorizontalLayoutGroup>();
        }
        else
        {
            childCount = TutorialController.instance.grounded.Count;
            groundObj = TutorialController.instance.groundObj.GetComponent<HorizontalLayoutGroup>();
        }

        groundObj.spacing = (620 - (childCount * 18) + 84) * -1;
    }

    public void OnDeckUpdate()
    {
        int childCount;
        HorizontalLayoutGroup playerCardObj;

        if (gameObject.GetComponent<RoomController>() != null)
        {
            childCount = RoomController.instance.player.cards.Count;
            playerCardObj = RoomController.instance.playerCardObj.GetComponent<HorizontalLayoutGroup>();
        }
        else
        {
            childCount = TutorialController.instance.player.cards.Count;
            playerCardObj = TutorialController.instance.playerCardObj.GetComponent<HorizontalLayoutGroup>();
        }

        playerCardObj.spacing = (620 - (childCount * 16) + 48) * -1;
    }

    public void SortDeckCards()
    {
        if (gameObject.GetComponent<RoomController>() != null)
        {
            RoomController.instance.player.cards = RoomController.instance.player.cards.OrderBy(c => c.seed).ThenBy(c => c.number).ToList<Card>();
            RoomController.instance.player.cards.ForEach((card) => card.obj.transform.SetSiblingIndex(RoomController.instance.player.cards.IndexOf(card)));
        }
        else
        {
            TutorialController.instance.player.cards = TutorialController.instance.player.cards.OrderBy(c => c.seed).ThenBy(c => c.number).ToList<Card>();
            TutorialController.instance.player.cards.ForEach((card) => card.obj.transform.SetSiblingIndex(TutorialController.instance.player.cards.IndexOf(card)));
        }
    }

    public void ReorderSetCards(Set set)
    {
        List<Card> reversedSet = new List<Card>(set.cards.ConvertAll(c => c));
        reversedSet.Reverse();
        set.cards.ForEach((c) => c.obj.transform.position = new Vector3(c.obj.transform.position.x, c.obj.transform.position.y, reversedSet.IndexOf(c)));
    }

    public void LoadTooltip(string text, bool translate = true)
    {
        if (translate)
        {
            text = TranslateService.instance.Translate(text);
        }

        this.tooltipObj.transform.Find("Text").GetComponent<Text>().text = text;
        this.tooltipObj.GetComponent<Image>().color = new Color(0, 0, 0, 0.9f);
        this.tooltipObj.transform.Find("Text").GetComponent<Text>().color = new Color(255, 255, 255, 1f);
        this.tooltipObj.SetActive(true);

        if (this.tooltipCoroutine != null)
        {
            StopCoroutine(this.tooltipCoroutine);
        }

        this.tooltipCoroutine = HideTooltip();
        StartCoroutine(this.tooltipCoroutine);
    }

    public void UpdateScores(List<Score> scores)
    {
        scores.ForEach((score) =>
        {
            if (RoomController.instance.player.team == score.team)
            {
                this.scoresObj.transform.Find("Container/Score").GetComponent<Text>().text = score.score.ToString();
            }
            else
            {
                this.scoresObj.transform.Find("Container/EnemyScore").GetComponent<Text>().text = score.score.ToString();

            }
        });
    }

    private IEnumerator HideTooltip()
    {
        Color tooltipColor = this.tooltipObj.GetComponent<Image>().color;
        Color textColor = this.tooltipObj.transform.Find("Text").GetComponent<Text>().color;

        while (tooltipColor.a > 0.0f)
        {
            tooltipColor.a -= 0.01f;
            textColor.a -= 0.01f;

            this.tooltipObj.GetComponent<Image>().color = tooltipColor;
            this.tooltipObj.transform.Find("Text").GetComponent<Text>().color = textColor;

            yield return new WaitForSeconds(0.05f);
        }

        this.tooltipObj.SetActive(false);
        yield break;
    }
}