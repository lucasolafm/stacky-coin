using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class AdInterstitial : Ad
{
    private InterstitialAd ad;

    public override bool PassesCondition()
    {
        return true;
    }

    public override void Initialize()
    {
        homeManager.SetState(new HomeInterstitialAd(homeManager));
        
        ResetTimer();
        
        ad = new InterstitialAd(adPlayer.adMobTestId);
        AdRequest request = new AdRequest.Builder().Build();
        ad.LoadAd(request);
        
        StartCoroutine(WaitUntilInterstitialAdIsLoaded(ad, couldLoad =>
        {
            if (couldLoad)
            {
                ad.OnAdFailedToShow += HandleAdFailedToShow;
                ad.OnAdClosed += HandleAdClosed;

                if (Application.isEditor)
                {
                    GameManager.I.loadingScreenCanvas.SetActive(false);
                }

                ad.Show();
            }
            else
            {
                Completed();
            }
        }));
    }

    public override void Tick()
    {
        if (!adClosed) return;
        adClosed = false;
        
        Invoke(nameof(Completed), 0.5f);
    }

    private void HandleAdFailedToShow(object sender, AdErrorEventArgs e)
    {
        adClosed = true;
    }

    private void HandleAdClosed(object sender, EventArgs e)
    {
        adClosed = true;
    }

    private void Completed()
    {
        homeManager.SetState(new HomeDefault(homeManager));
        homeManager.State.OnHomeSceneLoaded();
    }
}
