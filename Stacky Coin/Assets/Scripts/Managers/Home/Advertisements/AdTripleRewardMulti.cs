using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class AdTripleRewardMulti : Ad
{
    [SerializeField] private Renderer darkScreen;
    [SerializeField] private GameObject popup;
    [SerializeField] private ParticleSystem rewardEffect;
    [SerializeField] private AudioClip coinExplosionClip;
    [SerializeField] private Transform playAgainCoin;
    [SerializeField] private Button watch1Button;
    [SerializeField] private Button watch2Button;
    [SerializeField] private Button watch3Button;
    [SerializeField] private Button cancelButton;
    [SerializeField] private int minimumScoredCoins;
    [SerializeField] private float darkScreenFadeInTime;
    [SerializeField] private float darkScreenFadeOutTime;
    [SerializeField] private float popupEntertime;
    [SerializeField] private float popupExitTime;
    [SerializeField] private float popupEnterDelay;
    
    private RewardedAd[] ads = new RewardedAd[3];
    private int adAmountChosen;
    private int adsPlayed;
    private bool earnedReward;

    public override bool PassesCondition()
    {
        return GameManager.I.scoredCoins.Count >= minimumScoredCoins && Data.tripleAdRewardUsed >= 2;
    }

    public override void Initialize()
    {
        EventManager.LoadingScreenSlidOut.AddListener(OnLoadingScreenSlidOut);

        homeManager.SetState(new HomeTripleReward(homeManager));

        for (int i = 0; i < ads.Length; i++)
        {
            ads[i] = new RewardedAd(adPlayer.adMobTestId);
            AdRequest request = new AdRequest.Builder().Build();
            ads[i].LoadAd(request);
        }
    }

    private void OnLoadingScreenSlidOut()
    {
        StartCoroutine(FadeDarkScreenIn());
        StartCoroutine(PanelEnterAnimation(() =>
        {
            watch1Button.onClick.AddListener(PressWatch1Button);
            watch2Button.onClick.AddListener(PressWatch2Button);
            watch3Button.onClick.AddListener(PressWatch3Button);
            cancelButton.onClick.AddListener(PressCancelButton);
        }));
    }

    public override void Tick()
    {
        if (!adClosed) return;
            
        if (!earnedReward)
        {
            Complete();
            ReturnToDefaultHome();
            adClosed = false;
        }
        else 
        {
            if (adsPlayed < adAmountChosen)
            {
                PlayAd();
                adClosed = false;
            }
            else
            {
                if (timeAfterClosingAd == 0)
                {
                    Complete();
                }
        
                timeAfterClosingAd += Time.deltaTime;
                if (timeAfterClosingAd < adManager.delayAfterClosingAd) return;
                
                adClosed = false;
                adsPlayed = 0;

                MultiplyScoredCoins();
        
                RewardEffect();
        
                ReturnToDefaultHome();
            }
        }
    }
    
    private void PressWatch1Button()
    {
        adAmountChosen = 1;
        
        PlayAd();
    }
    
    private void PressWatch2Button()
    {
        adAmountChosen = 2;
        
        PlayAd();
    }

    private void PressWatch3Button()
    {
        adAmountChosen = 3;
        
        PlayAd();
    }

    private void PressCancelButton()
    {
        ResetTimer();

        ReturnToDefaultHome();
        
        watch1Button.onClick.RemoveAllListeners();
        watch2Button.onClick.RemoveAllListeners();
        watch3Button.onClick.RemoveAllListeners();
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
        Vector3 targetScale = popup.transform.localScale;
        float t = 0;

        yield return new WaitForSeconds(popupEnterDelay);
        popup.SetActive(true);

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / popupEntertime, 1);

            popup.transform.localScale = Utilities.EaseOutBack(t) * targetScale;

            yield return null;
        }

        completed();
    }

    private IEnumerator PanelExitAnimation()
    {
        Vector3 currentScale = popup.transform.localScale;
        float t = 0;

        while (t < 1)
        {
            t = Math.Min(t + Time.deltaTime / popupExitTime, 1);
            
            popup.transform.localScale = (1 - Utilities.EaseOutSine(t)) * currentScale;

            yield return null;
        }
        
        popup.SetActive(false);
    }

    private void PlayAd()
    {
        StartCoroutine(WaitUntilRewardedAdIsLoaded(ads[adsPlayed], () =>
        {
            ads[adsPlayed].OnUserEarnedReward += HandleUserEarnedReward;
            ads[adsPlayed].OnAdClosed += HandleAdClosed;
            ads[adsPlayed].OnAdFailedToShow += HandleAdFailedToShow;
            
            ads[adsPlayed].Show();
        }, adsPlayed == adAmountChosen - 1));
    }

    private void HandleUserEarnedReward(object sender, Reward args)
    {
        earnedReward = true;
        adsPlayed++;
    }
    
    private void HandleAdClosed(object sender, EventArgs args)
    {
        adClosed = true;
    }
    
    private void HandleAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        adClosed = true;
    }

    private void MultiplyScoredCoins()
    {
        int originalCount = GameManager.I.scoredCoins.Count;
        
        for (int i = 0; i < adAmountChosen * 3 - 1; i++)
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
    
    private void ReturnToDefaultHome()
    {
        homeManager.SetState(new HomeDefault(homeManager));

        EventManager.EnterDefaultHome.Invoke();
    }
    
    private void Complete()
    {
        darkScreen.gameObject.SetActive(false);
        popup.SetActive(false);

        ResetTimer();
    }
}
