using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GoogleMobileAds.Api;
using UnityEngine.Events;

public class Ad : MonoBehaviour
{
    public static UnityEvent<RewardedAd, bool> RewardedAdLoading = new UnityEvent<RewardedAd, bool>();
    public static UnityEvent<InterstitialAd> InterstitialAdLoading = new UnityEvent<InterstitialAd>();
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

    protected void ResetTimer()
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
        currentPotentialMiniCoinsAmount = homeManager.startOriginalMiniCoinsValue + scoredCoinsValue + 
                                            (homeManager.bonusCoinsIsAvailable ? homeManager.bonusCoinsController.bonusCoins.Count : 0);
    }

    protected IEnumerator WaitUntilRewardedAdIsLoaded(RewardedAd ad, Action completed, bool isFinalAd = true)
    {
        RewardedAdLoading.Invoke(ad, isFinalAd);

        GameManager.I.loadingSpinner.SetActive(true);

        adLoadingTime = 0;
        while (!ad.IsLoaded())
        {
            yield return null;
            adLoadingTime += Time.unscaledDeltaTime;

            if (adLoadingTime > adManager.maxTimeLoadingAd)
            {
                StartCoroutine(adManager.DisplayNoInternetPopup());
                GameManager.I.loadingSpinner.SetActive(false);

                AdFailedToLoad.Invoke();

                yield break;
            }
        }

        completed();
    }
    
    protected IEnumerator WaitUntilInterstitialAdIsLoaded(InterstitialAd ad, Action<bool> completed)
    {
        InterstitialAdLoading.Invoke(ad);

        GameManager.I.loadingSpinner.SetActive(true);

        adLoadingTime = 0;
        while (!ad.IsLoaded())
        {
            yield return null;
            adLoadingTime += Time.unscaledDeltaTime;

            if (!(adLoadingTime > adManager.maxTimeLoadingAd)) continue;
            
            GameManager.I.loadingSpinner.SetActive(false);

            AdFailedToLoad.Invoke();

            completed(false);
            yield break;
        }

        completed(true);
    }

    private void OnAdClosed(bool isFinalAd)
    {
        if (!isFinalAd) return;
        
        GameManager.I.loadingSpinner.SetActive(false);
    }
}
