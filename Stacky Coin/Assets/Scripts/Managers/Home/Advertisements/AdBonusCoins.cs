using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using GoogleMobileAds.Api;

public class AdBonusCoins : Ad
{
    [SerializeField] private BonusCoinsController bonusCoinsController;
    [SerializeField] private SpriteRenderer bonusCoinsButton;
    [SerializeField] private Collider buttonCollider;
    [SerializeField] private AudioClip coinExplosionClip;
    [SerializeField] private List<int> bonusCoins = new List<int>();
    [SerializeField] private float chanceToPassConditionAnyways;
    [SerializeField] private int particleAmount;
    [SerializeField] private ButtonShakeSettings _shakeSettings;

    private RewardedAd ad;

    public override bool PassesCondition()
    {
        GetScoredCoinsValue();

        GetCurrentPotentialMiniCoinsAmount();

        foreach (int chest in Data.chests)
        {
            if (!(currentPotentialMiniCoinsAmount < chest && currentPotentialMiniCoinsAmount + bonusCoins.Count >= chest)) continue;

            return true;
        }

        return UnityEngine.Random.Range(0, 1f) < chanceToPassConditionAnyways;
    }

    public override void Initialize()
    {
        bonusCoinsButton.enabled = true;
        buttonCollider.enabled = true;
        StartCoroutine(ShakeButton());

        ResetTimer();

        ad = new RewardedAd(adPlayer.adMobTestId);
        AdRequest request = new AdRequest.Builder().Build();
        ad.LoadAd(request);
    }

    public override void Tick()
    {
        if (!adClosed || !rewardEarned) return;

        timeAfterClosingAd += Time.deltaTime;
        if (timeAfterClosingAd < adManager.delayAfterClosingAd) return;

        OnFinishAd();

        adClosed = false;
    }

    private void HandleUserEarnedReward(object sender, Reward args)
    {
        rewardEarned = true;
    }

    private void HandleAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        adClosed = true;
    }

    private void HandleAdClosed(object sender, EventArgs args)
    {
        adClosed = true;
        buttonCollider.enabled = false;
    }

    public void PressBonusCoinsButton()
    {
        if (!homeManager.State.CanDropMiniCoins()) return;

        ad.OnAdFailedToShow += HandleAdFailedToShow;
        ad.OnUserEarnedReward += HandleUserEarnedReward;
        ad.OnAdClosed += HandleAdClosed;

        StartCoroutine(WaitUntilRewardedAdIsLoaded(ad, () =>
        {
            ad.Show();
        }));
    }

    private void OnFinishAd()
    {
        StartCoroutine(bonusCoinsController.ButtonPressedAnimation(bonusCoinsButton, particleAmount));
        
        GameManager.I.audioSource.PlayOneShot(coinExplosionClip, 1);

        homeManager.State.DropMiniCoins(bonusCoins);
    }
    
    private IEnumerator ShakeButton()
    {
        float t;
        Quaternion startRotation;
        float rotationValue;
        float shakeRatio;
        float angle;
        bool firstShake;
        bool lastShake;
        float lastShakeAngle = 0;
        Transform buttonTransform = bonusCoinsButton.transform;

        while (true)
        {
            yield return new WaitForSeconds(_shakeSettings.Delay);
            
            for (int i = 0; i < _shakeSettings.ShakeAmount; i++)
            {
                t = 0;
                startRotation = buttonTransform.rotation;
                firstShake = i == 0;
                lastShake = i == _shakeSettings.ShakeAmount - 1;
                if (lastShake) lastShakeAngle = buttonTransform.eulerAngles.z;
                shakeRatio = (_shakeSettings.ShakeAmount - i) / _shakeSettings.ShakeAmount;

                while (t < 1)
                {
                    t = Mathf.Min(t + Time.deltaTime / (_shakeSettings.Time * (firstShake ? 0.5f : 1)), 1);

                    angle = _shakeSettings.AngleValue * shakeRatio;

                    rotationValue = (lastShake ? lastShakeAngle : angle * (firstShake ? 0.5f : 1)) *
                                    (i % 2 == 0 ? 1 : -1) *
                                    Utilities.EaseInOutSine(t);

                    buttonTransform.rotation = startRotation * Quaternion.Euler(0, 0, rotationValue);

                    yield return null;
                }
            }
        }
    }
}

