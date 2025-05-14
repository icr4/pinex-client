using System;
using Constants;
using GoogleMobileAds;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdsService : MonoBehaviour
{
    public static AdsService instance { get; private set; }
    private RewardedAd rewardedAdInstance;
    private InterstitialAd interstitialInstance;
    private bool rewardedConfirm = false;

    public bool showInterstitial = false;

    void Awake()
    {
        if (instance != null && instance != this) Destroy(this);
        else instance = this;
    }

    // ####################
    // Interstitial
    // ####################

    public void LoadInterstitialAd(bool show = false)
    {
        this.interstitialInstance = null;

        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            AdRequest adRequest = new AdRequest();

            InterstitialAd.Load(AdsConstants.INTERSTITIAL_ID, adRequest, (InterstitialAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null) return;
                this.interstitialInstance = ad;

                if (show) this.interstitialInstance.Show();
            });
        });
    }

    public void ShowInterstitialAd()
    {
        this.showInterstitial = false;

        if (this.interstitialInstance == null)
        {
            this.LoadInterstitialAd(true);
            return;
        }

        if (this.interstitialInstance != null && this.interstitialInstance.CanShowAd())
        {
            this.interstitialInstance.Show();
        }
    }

    // ####################
    // Rewarded
    // ####################

    public void RewardedAdModal(string title = "lobby.ads.title", string descr = "lobby.ads.text")
    {
        this.rewardedConfirm = false;
        this.LoadRewardedAd();

        string text = TranslateService.instance.Translate(descr);
        ConfirmService.instance.Show(title, text, () => { }, () =>
        {
            this.rewardedConfirm = true;
            PlayerPrefs.SetString("advertisement", DateTime.UtcNow.ToString());

            this.ShowRewardedAd();
        });
    }

    private void LoadRewardedAd()
    {
        this.rewardedConfirm = false;

        MobileAds.Initialize((InitializationStatus initStatus) =>
        {
            if (rewardedAdInstance != null)
            {
                rewardedAdInstance.Destroy();
                rewardedAdInstance = null;
            }

            AdRequest adRequest = new AdRequest();
            RewardedAd.Load(AdsConstants.REWARDED_ID, adRequest, (RewardedAd ad, LoadAdError error) =>
            {
                if (error != null || ad == null) return;

                this.rewardedAdInstance = ad;

                ServerSideVerificationOptions options = new ServerSideVerificationOptions(new()
                {
                    UserId = ClientService.instance.user.id,
                    CustomData = ClientService.instance.user.id
                });

                this.rewardedAdInstance.SetServerSideVerificationOptions(options);
                this.ShowRewardedAd();
            });
        });
    }

    private void ShowRewardedAd()
    {
        if (this.rewardedAdInstance != null && this.rewardedAdInstance.CanShowAd() && rewardedConfirm)
        {
            this.rewardedAdInstance.Show((Reward reward) =>
            {
                LobbyController.instance.LoadUser(() => { });
            });
        }
    }
}