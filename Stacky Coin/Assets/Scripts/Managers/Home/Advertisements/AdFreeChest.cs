using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;

public class AdFreeChest : Ad
{
    [SerializeField] private ChestManager chestManager;
    [SerializeField] private UnlockSkinManager unlockSkinManager;
    [SerializeField] private int coinsBelowChestMinimum;
    [SerializeField] private float delayAfterAd;
    private Chest chestWithAd;

    private RewardedAd ad;

    public override bool PassesCondition()
    {
        GetScoredCoinsValue();

        GetCurrentPotentialMiniCoinsAmount();

        foreach (int id in GameManager.I.scoredCoins)
        {
            if ((id == 2 || id == 3) && currentPotentialMiniCoinsAmount < chestManager.priceMinimums[id - 1] - coinsBelowChestMinimum)
            {
                return true;
            }
        }

        return false;
    }

    public override void Initialize()
    {
        EventManager.EnabledNewChest.AddListener(OnEnabledNewChest);
        EventManager.BuysChestWithAd.AddListener(OnBuysChestWithAd);

        ad = new RewardedAd(adPlayer.adMobTestId);
        AdRequest request = new AdRequest.Builder().Build();
        ad.LoadAd(request);
    }

    public override void Tick()
    {
        // if (adOpened)
        // {
        //     chestWithAd.state.StartOpeningProcess();
        //
        //     adOpened = false;
        // }

        if (!adClosed) return;

        timeAfterClosingAd += Time.deltaTime;
        if (timeAfterClosingAd < adManager.delayAfterClosingAd) return;

        if (rewardEarned)
        {
            chestWithAd.state.StartOpeningProcess();
            OpenChest();
        }
        
        adClosed = false;
    }

    public void HandleUserEarnedReward(object sender, Reward args)
    {
        rewardEarned = true;
    }

    public void HandleOnAdOpening(object sender, EventArgs args)
    {
        adOpened = true;
    }

    public void HandleAdClosed(object sender, EventArgs args)
    {
        adClosed = true;
        chestWithAd.collider.enabled = false;
    }

    private void OnEnabledNewChest(Chest chest)
    {
        if (chestWithAd != null) return;

        if (chest.level == 1 || currentPotentialMiniCoinsAmount > chest.price - coinsBelowChestMinimum) return;

        chestWithAd = chest;

        chest.SetState(new ChestAd(chest));

        ResetTimer();
    }

    private void OnBuysChestWithAd(Chest chest)
    {
        ad.OnUserEarnedReward += HandleUserEarnedReward;
        ad.OnAdOpening += HandleOnAdOpening;
        ad.OnAdClosed += HandleAdClosed;
        
        StartCoroutine(WaitUntilRewardedAdIsLoaded(ad, () =>
        {
            ad.Show();
        }));
    }

    private void OpenChest()
    {
        EventManager.PaidForChest.Invoke(chestWithAd);

        unlockSkinManager.OpenChest(true);
    }
}
