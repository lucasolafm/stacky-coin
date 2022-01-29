using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdInterstitial : Ad
{
    public override bool PassesCondition()
    {
        return true;
    }

    public override void Initialize()
    {
        if (adPlayer == null) adPlayer = new AdPlayer();

        //adPlayer.PlayAd(placementId);

        homeManager.SetState(new HomeInterstitialAd(homeManager));
    }

    /*
    public override void OnAdStarted()
    {
        ResetTimer();

        EventManager.EnterDefaultHome.Invoke();
    }

    public override void OnAdEnded()
    {
        homeManager.SetState(new HomeDefault(homeManager));
    }

    public override void OnAdError()
    {
        homeManager.SetState(new HomeDefault(homeManager));

        EventManager.EnterDefaultHome.Invoke();
    }
    */
}
