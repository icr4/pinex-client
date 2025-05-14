using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Models;
using System;

public class ScoreboardController : MonoBehaviour
{
    public static ScoreboardController instance { get; private set; }

    public GameObject scoreboardObj;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    public void Show(List<Score> scores, string reason)
    {
        scoreboardObj.transform.Find("Modal/Title").GetComponent<Text>().text = this.ResultText(scores);
        scoreboardObj.transform.Find("Modal/Panel/Content/Description").GetComponent<Text>().text = this.DescriptionText(scores, reason);
        scoreboardObj.transform.Find("Modal/Panel/Content/LeaveBtn").GetComponent<Button>().onClick.AddListener(() =>
        {
            AdsService.instance.showInterstitial = false;

            InputController.instance.LeaveRoom();
        });

        List<string> teamPlayers =
            RoomController.instance.room.players
                .FindAll((p) => p.team == RoomController.instance.player.team)
                .ConvertAll((p) => p.username);

        List<string> opponentPlayers =
            RoomController.instance.room.players
                .FindAll((p) => p.team != RoomController.instance.player.team)
                .ConvertAll((p) => p.username);

        scoreboardObj.transform.Find("Modal/Panel/Content/Results/Team/Label").GetComponent<Text>().text = String.Join("\n\r", teamPlayers);
        scoreboardObj.transform.Find("Modal/Panel/Content/Results/Opponent/Label").GetComponent<Text>().text = String.Join("\n\r", opponentPlayers);

        scores.ForEach((score) =>
        {
            string scoreBox = score.team == RoomController.instance.player.team ? "Team" : "Opponent";
            scoreboardObj.transform.Find($"Modal/Panel/Content/Results/{scoreBox}/Score").GetComponent<Text>().text = score.score.ToString();
            scoreboardObj.transform.Find($"Modal/Panel/Content/Results/{scoreBox}/ToPay").GetComponent<Text>().text = score.toPay.ToString();
            scoreboardObj.transform.Find($"Modal/Panel/Content/Results/{scoreBox}/Total").GetComponent<Text>().text = score.total.ToString();
        });

        PlayerProfileController.instance.profileObj.GetComponent<SideModalController>().Hide();
        ChatController.instance.chatModalObj.transform.GetComponent<SideModalController>().Hide();
        RoomSettingsController.instance.Hide();

        scoreboardObj.SetActive(true);
    }

    private string ResultText(List<Score> scores)
    {
        int teamTotal = this.TeamTotal(scores);
        int opponentTotal = this.OpponentTotal(scores);

        if (teamTotal > opponentTotal) return TranslateService.instance.Translate("scoreboard.results.victory");
        else if (opponentTotal > teamTotal) return TranslateService.instance.Translate("scoreboard.results.defeat");
        else return TranslateService.instance.Translate("scoreboard.results.draw");
    }

    private string DescriptionText(List<Score> scores, string reason)
    {
        int teamTotal = this.TeamTotal(scores);
        int opponentTotal = this.OpponentTotal(scores);

        if (teamTotal > opponentTotal) return TranslateService.instance.Translate($"scoreboard.descriptions.win_{reason}");
        else if (opponentTotal > teamTotal) return TranslateService.instance.Translate($"scoreboard.descriptions.defeat_{reason}");
        else return TranslateService.instance.Translate($"scoreboard.descriptions.draw_{reason}");
    }

    private int TeamTotal(List<Score> scores)
    {
        return scores
            .Find((score) => score.team == RoomController.instance.player.team)
            .total;
    }

    private int OpponentTotal(List<Score> scores)
    {
        return scores
            .Find((score) => score.team != RoomController.instance.player.team)
            .total;
    }
}