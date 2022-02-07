using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class AdTripleReward : Ad
{
    [SerializeField] private Renderer darkScreen;
    [SerializeField] private GameObject tripleAdRewardPopup;
    [SerializeField] private Button watchButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private ParticleSystem rewardEffect;
    [SerializeField] private AudioClip coinExplosionClip;
    [SerializeField] private Transform playAgainCoin;
    [SerializeField] private int minimumScoredCoins;
    [SerializeField] private float chanceToPassConditionAnyways;
    [SerializeField] private float darkScreenFadeInTime;
    [SerializeField] private float darkScreenFadeOutTime;
    [SerializeField] private float popupEntertime;
    [SerializeField] private float popupExitTime;
    [SerializeField] private float popupEnterDelay;

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
        EventManager.LoadingScreenSlidOut.AddListener(OnLoadingScreenSlidOut);
        
        homeManager.SetState(new HomeTripleReward(homeManager));
        
        ad = new RewardedAd(adPlayer.adMobTestId);
        AdRequest request = new AdRequest.Builder().Build();
        ad.LoadAd(request);
    }

    private void OnLoadingScreenSlidOut()
    {
        StartCoroutine(FadeDarkScreenIn());
        StartCoroutine(PanelEnterAnimation(() =>
        {
            watchButton.onClick.AddListener(PressWatchButton);
            cancelButton.onClick.AddListener(PressCancelButton);
        }));
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
        ResetTimer();

        ReturnToDefaultHome();
        
        watchButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
        StartCoroutine(FadeDarkScreenOut());
        StartCoroutine(PanelExitAnimation());
    }

    private IEnumerator FadeDarkScreenIn()
    {
        darkScreen.gameObject.SetActive(true);
        float targetAlpha = darkScreen.material.color.a;
        float t = 0;

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / darkScreenFadeInTime, 1);
            
            darkScreen.material.color = new Color(0, 0, 0, Utilities.EaseOutSine(t) * targetAlpha);

            yield return null;
        }
    }
    
    private IEnumerator FadeDarkScreenOut()
    {
        float currentAlpha = darkScreen.material.color.a;
        float t = 0;

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / darkScreenFadeOutTime, 1);
            
            darkScreen.material.color = new Color(0, 0, 0, (1 - Utilities.EaseOutSine(t)) * currentAlpha);

            yield return null;
        }
        
        darkScreen.gameObject.SetActive(false);
    }

    private IEnumerator PanelEnterAnimation(Action completed)
    {
        Vector3 targetScale = tripleAdRewardPopup.transform.localScale;
        float t = 0;

        yield return new WaitForSeconds(popupEnterDelay);
        tripleAdRewardPopup.SetActive(true);

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / popupEntertime, 1);

            tripleAdRewardPopup.transform.localScale = Utilities.EaseOutBack(t) * targetScale;

            yield return null;
        }

        completed();
    }

    private IEnumerator PanelExitAnimation()
    {
        Vector3 currentScale = tripleAdRewardPopup.transform.localScale;
        float t = 0;

        while (t < 1)
        {
            t = Math.Min(t + Time.deltaTime / popupExitTime, 1);
            
            tripleAdRewardPopup.transform.localScale = (1 - Utilities.EaseOutSine(t)) * currentScale;

            yield return null;
        }
        
        tripleAdRewardPopup.SetActive(false);
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
        
        GameManager.I.audioSource.PlayOneShot(coinExplosionClip);
    }

    private void Complete()
    {
        darkScreen.gameObject.SetActive(false);
        tripleAdRewardPopup.gameObject.SetActive(false);
        
        ResetTimer();
    }

    private void ReturnToDefaultHome()
    {
        homeManager.SetState(new HomeDefault(homeManager));

        EventManager.EnterDefaultHome.Invoke();
    }
}
