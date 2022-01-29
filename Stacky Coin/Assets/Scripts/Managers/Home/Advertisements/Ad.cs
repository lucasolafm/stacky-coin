using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using UnityEngine.Events;

public class Ad : MonoBehaviour
{
    public static UnityEvent<RewardedAd, bool> AdLoading = new UnityEvent<RewardedAd, bool>();
    public static UnityEvent AdFailedToLoad = new UnityEvent();

    [SerializeField] protected HomeManager homeManager;
    public float timerTime;
    public string placementId;

    protected AdManager adManager;
    protected AdPlayer adPlayer;
    protected int scoredCoinsValue = 0;
    protected int currentPotentialMiniCoinsAmount;
    protected bool adClosed;
    protected bool adOpened;
    protected bool rewardEarned;
    protected float timeAfterClosingAd;
    private float adLoadingTime;

    void Awake()
    {
        adManager = GetComponent<AdManager>();
        adPlayer = GetComponent<AdPlayer>();

        AdPlayer.AdClosed.AddListener(OnAdClosed);
    }

    public virtual bool PassesCondition() { return false; }

    public virtual void Initialize() {}

    public virtual void Tick() {}

    public virtual void OnAdStarted() {}

    public virtual void OnAdEnded() {}

    public virtual void OnAdFinished() {}

    public virtual void OnAdError() {}

    protected void GetScoredCoinsValue()
    {
        foreach (int coin in GameManager.I.scoredCoins)
        {
            if (coin == 0)
            {
                scoredCoinsValue++;
            }
            else if (coin >= GameManager.I.coinSkinAmount)
            {
                scoredCoinsValue += GameManager.I.gemBonusAmount;
            }
        }
    }

    public void ResetTimer()
    {
        for (int i = 0; i < adManager.ads.Length; i++)
        {
            if (adManager.ads[i] != this) continue;

            float[] timersFromData = Data.adTimers;
            timersFromData[i] = timerTime;
            Data.adTimers = timersFromData;

            break;
        }
    }

    protected void GetCurrentPotentialMiniCoinsAmount()
    {
        currentPotentialMiniCoinsAmount = homeManager.startOriginalMiniCoins.Length + scoredCoinsValue + 
                                            (homeManager.bonusCoinsIsAvailable ? homeManager.bonusCoins.Count : 0);
    }

    protected IEnumerator WaitUntilAdIsLoaded(RewardedAd ad, Action completed, bool isFinalAd = true)
    {
        AdLoading.Invoke(ad, isFinalAd);

        adManager.loadingSpinnerAnimator.SetActive(true);

        adLoadingTime = 0;
        while (!ad.IsLoaded())
        {
            yield return null;
            adLoadingTime += Time.unscaledDeltaTime;

            if (adLoadingTime > adManager.maxTimeLoadingAd)
            {
                StartCoroutine(adManager.DisplayNoInternetPopup());
                adManager.loadingSpinnerAnimator.SetActive(false);

                AdFailedToLoad.Invoke();

                yield break;
            }
        }

        completed();
    }

    public void OnAdClosed(bool isFinalAd)
    {
        if (!isFinalAd) return;
        
        adManager.loadingSpinnerAnimator.SetActive(false);
    }
}
