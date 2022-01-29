using System;
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

    [SerializeField] private bool testMode = true;
    [SerializeField] private GraphicRaycaster[] graphicRaycasters;

    private string gameIdAndroid = "3844855";
    public string adMobTestId = "ca-app-pub-3940256099942544/5224354917";
    private float timeLoadingAd;
    private bool adClosed;
    private bool isFinalAd;
    
    void Awake()
    {
        Ad.AdLoading.AddListener(OnAdLoading);
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

    private void OnAdLoading(RewardedAd ad, bool isFinalAd)
    {
        this.isFinalAd = isFinalAd;

        ToggleGraphicRaycasters(false);

        ad.OnAdClosed += HandleAdClosed;
        ad.OnAdFailedToShow += HandleAdFailedToShow;
    }

    private void OnAdFailedToLoad()
    {
        ToggleGraphicRaycasters(true);
    }

    public void HandleAdClosed(object sender, EventArgs args)
    {
        adClosed = true;
    }

    public void HandleAdFailedToShow(object sender, AdErrorEventArgs args)
    {
        adClosed = true;
    }
    
    private void ToggleGraphicRaycasters(bool onOrOff)
    {
        foreach (GraphicRaycaster graphicRaycaster in graphicRaycasters)
        {
            graphicRaycaster.enabled = onOrOff;
        }
    }
}
