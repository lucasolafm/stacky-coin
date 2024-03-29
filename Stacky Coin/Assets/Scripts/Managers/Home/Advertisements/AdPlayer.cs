﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.Advertisements;
using UnityEngine.UI;
using GoogleMobileAds.Api;
using UnityEngine.Events;

public class AdPlayer : MonoBehaviour
{
    public static UnityEvent<bool> AdClosed = new UnityEvent<bool>();
    public static bool IsLoadingAd;

    [SerializeField] private bool testMode = true;
    [SerializeField] private GraphicRaycaster[] graphicRaycasters;

    private string gameIdAndroid = "3844855";
    public string adMobTestId = "ca-app-pub-3940256099942544/5224354917";
    private float timeLoadingAd;
    private bool adClosed;
    private bool isFinalAd;
    
    void Awake()
    {
        Ad.RewardedAdLoading.AddListener(OnRewardedAdLoading);
        Ad.InterstitialAdLoading.AddListener(OnInterstitialAdLoading);
        Ad.AdFailedToLoad.AddListener(OnAdFailedToLoad);

        MobileAds.Initialize(initStatus => {});
    }

    void Update()
    {
        if (!adClosed) return;

        AdClosed.Invoke(isFinalAd);

        ToggleGraphicRaycasters(true);

        adClosed = false;
    }

    private void OnRewardedAdLoading(RewardedAd ad, bool isFinalAd)
    {
        this.isFinalAd = isFinalAd;

        ToggleGraphicRaycasters(false);

        ad.OnAdClosed += HandleAdClosed;
        ad.OnAdFailedToShow += HandleAdFailedToShow;
    }

    private void OnInterstitialAdLoading(InterstitialAd ad)
    {
        isFinalAd = true;
        
        ToggleGraphicRaycasters(false);

        ad.OnAdClosed += HandleAdClosed;
        ad.OnAdFailedToShow += HandleAdFailedToShow;
    }

    private void OnAdFailedToLoad()
    {
        ToggleGraphicRaycasters(true);
    }

    private void HandleAdClosed(object sender, EventArgs args)
    {
        adClosed = true;
    }

    private void HandleAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        adClosed = true;
    }
    
    private void ToggleGraphicRaycasters(bool onOrOff)
    {
        IsLoadingAd = !onOrOff;
        foreach (GraphicRaycaster graphicRaycaster in graphicRaycasters)
        {
            graphicRaycaster.enabled = onOrOff;
        }
    }
}
