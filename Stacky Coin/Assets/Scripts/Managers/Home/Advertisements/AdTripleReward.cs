using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class AdTripleReward : Ad
{
    [SerializeField] private GameObject darkScreen;
    [SerializeField] private GameObject tripleAdRewardPopup;
    [SerializeField] private Button watchButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private ParticleSystem rewardEffect;
    [SerializeField] private Transform playAgainCoin;
    [SerializeField] private int minimumScoredCoins;
    [SerializeField] private float chanceToPassConditionAnyways;

    private RewardedAd ad;

    public override bool PassesCondition()
    {
        if (GameManager.I.scoredCoins.Count < minimumScoredCoins || Data.tripleAdRewardUsed >= 2) return false;

        GetScoredCoinsValue();

        GetCurrentPotentialMiniCoinsAmount();

        foreach (int chest in Data.chests)
        {
            if (!(currentPotentialMiniCoinsAmount < chest && currentPotentialMiniCoinsAmount + scoredCoinsValue * 2 >= chest)) continue;

            return true;
        }

        return UnityEngine.Random.Range(0, 1f) < chanceToPassConditionAnyways;
    }

    public override void Initialize()
    {
        watchButton.onClick.AddListener(PressWatchButton);
        cancelButton.onClick.AddListener(PressCancelButton);

        homeManager.SetState(new HomeTripleReward(homeManager));

        darkScreen.SetActive(true);
        tripleAdRewardPopup.SetActive(true);

        ad = new RewardedAd(adPlayer.adMobTestId);
        AdRequest request = new AdRequest.Builder().Build();
        ad.LoadAd(request);
    }

    public override void Tick()
    {
        if (!adClosed) return;

        if (timeAfterClosingAd == 0)
        {
            Complete();
        }

        if (!rewardEarned)
        {
            ReturnToDefaultHome();
            return;
        }

        timeAfterClosingAd += Time.deltaTime;
        if (timeAfterClosingAd < adManager.delayAfterClosingAd) return;

        TripleScoredCoins();

        RewardEffect();

        ReturnToDefaultHome();

        adClosed = false;
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        rewardEarned = true;
    }

    public void HandleAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        adClosed = true;
    }

    public void HandleAdClosed(object sender, EventArgs args)
    {
        adClosed = true;
    }

    private void PressWatchButton()
    {
        ad.OnUserEarnedReward += HandleUserEarnedReward;
        ad.OnAdFailedToShow += HandleAdFailedToShow;
        ad.OnAdClosed += HandleAdClosed;

        StartCoroutine(WaitUntilAdIsLoaded(ad, () =>
        {
            Data.tripleAdRewardUsed = Data.tripleAdRewardUsed + 1;
            
            ad.Show();
        }));
    }

    private void PressCancelButton()
    {
        Complete();

        ReturnToDefaultHome();
    }

    private void TripleScoredCoins()
    {
        int originalCount = GameManager.I.scoredCoins.Count;

        for (int i = 0; i < 2; i++)
        {
            for (int z = 0; z < originalCount; z++)
            {
                if (GameManager.I.scoredCoins[z] > 0 && GameManager.I.scoredCoins[z] < GameManager.I.coinSkinAmount) continue;

                GameManager.I.scoredCoins.Add(GameManager.I.scoredCoins[z]);
            }
        }
    }

    private void RewardEffect()
    {
        rewardEffect.transform.position = playAgainCoin.position;
        rewardEffect.Play();
    }

    private void Complete()
    {
        darkScreen.SetActive(false);
        tripleAdRewardPopup.SetActive(false);

        ResetTimer();
    }

    private void ReturnToDefaultHome()
    {
        homeManager.SetState(new HomeDefault(homeManager));

        EventManager.EnterDefaultHome.Invoke();
    }
}
