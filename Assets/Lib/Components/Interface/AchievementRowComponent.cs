using UnityEngine;
using UnityEngine.UI;
using Models;
using System.Collections.Generic;
using System.Linq;

public class AchievementRowComponent : MonoBehaviour
{

    public Achievement achievement;

    public void Set(Achievement achievement)
    {
        this.achievement = achievement;

        this.SetTitle();
        this.SetExpRewards();
        this.SetCoinRewards();
        this.SetTokenRewards();
        this.SetAvatarRewards();

        this.gameObject.SetActive(true);
    }

    private void SetTitle()
    {
        string title = TranslateService.instance.Translate($"achievements.{this.achievement.name}");
        this.transform.Find("Achievement/Title").GetComponent<Text>().text = title;
    }

    private void SetExpRewards()
    {
        List<AchievementReward> expRewards = this.achievement.rewards.FindAll((r) => r.type == "exp");
        if (expRewards.Count > 0)
        {
            int exp = expRewards.ConvertAll((r) => r.value).Sum();
            this.transform.Find("Achievement/Rewards/ExpReward/Value").GetComponent<Text>().text = $"+{exp}";
            this.transform.Find("Achievement/Rewards/ExpReward").gameObject.SetActive(true);
        }
        else
        {
            this.transform.Find("Achievement/Rewards/ExpReward").gameObject.SetActive(false);

        }
    }

    private void SetCoinRewards()
    {
        List<AchievementReward> coinRewards = this.achievement.rewards.FindAll((r) => r.type == "coins");
        if (coinRewards.Count > 0)
        {
            int coins = coinRewards.ConvertAll((r) => r.value).Sum();
            this.transform.Find("Achievement/Rewards/CoinReward/Value").GetComponent<Text>().text = $"+{coins}";
            this.transform.Find("Achievement/Rewards/CoinReward").gameObject.SetActive(true);
        }
        else
        {
            this.transform.Find("Achievement/Rewards/CoinReward").gameObject.SetActive(false);

        }
    }

    private void SetTokenRewards()
    {
        List<AchievementReward> tokenRewards = this.achievement.rewards.FindAll((r) => r.type == "tokens");
        if (tokenRewards.Count > 0)
        {
            int tokens = tokenRewards.ConvertAll((r) => r.value).Sum();
            this.transform.Find("Achievement/Rewards/TokenReward/Value").GetComponent<Text>().text = $"+{tokens}";
            this.transform.Find("Achievement/Rewards/TokenReward").gameObject.SetActive(true);
        }
        else
        {
            this.transform.Find("Achievement/Rewards/TokenReward").gameObject.SetActive(false);

        }
    }

    private void SetAvatarRewards()
    {
        AchievementReward avatarReward = this.achievement.rewards.Find((r) => r.type == "avatar");
        if (avatarReward != null)
        {
            StartCoroutine(AvatarService.instance.Fetch(avatarReward.value_str, (result) =>
                {
                    this.transform.Find("Achievement/Rewards/AvatarReward/Frame/Avatar").GetComponent<RawImage>().texture = result as Texture2D;
                }, (error) => { }, (end) => { })
            );

            this.transform.Find("Achievement/Rewards/AvatarReward").gameObject.SetActive(true);
        }
        else
        {
            this.transform.Find("Achievement/Rewards/AvatarReward").gameObject.SetActive(false);

        }
    }
}