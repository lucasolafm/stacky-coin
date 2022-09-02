using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class  AdManager : MonoBehaviour
{
    [SerializeField] private GameObject noInternetPopup;
    public GameObject loadingSpinnerAnimator;
    public Ad[] ads;
    public float maxTimeLoadingAd;
    [SerializeField] private float timeUntilAdMin, timeUntilAdMax;
    public float delayAfterClosingAd;
    [SerializeField] private float noInternetPopupTime;

    private List<Ad> availableAds = new List<Ad>();
    private Ad currentAd;

    void Awake()
    {
        EventManager.FirstTimeInHome.AddListener(OnFirstTimeInHome);
    }

    void Update()
    {
        if (!currentAd) return;

        currentAd.Tick();
    }

    public void Initialize()
    {
        RecordTimePlayed();

        if (IsTimeForAd() && GetAvailableAds() > 0)
        {
            currentAd = GetAd();

            currentAd.Initialize();

            ResetPlayAdTimer();
        }
    }

    private void OnFirstTimeInHome()
    {
        InitializeTimers();
    }

    private bool IsTimeForAd()
    {
        return Data.playAdTimer <= 0;
    }

    private Ad GetAd()
    {
        return availableAds[UnityEngine.Random.Range(0, availableAds.Count)];
    }

    private void RecordTimePlayed()
    {
        Data.playAdTimer -= GameManager.I.timePlayedLastPlay;

        float[] tempTimers = new float[Data.adTimers.Length];
        for (int i = 0; i < tempTimers.Length; i++)
        {
            tempTimers[i] = Data.adTimers[i] - GameManager.I.timePlayedLastPlay;
        }
        Data.adTimers = tempTimers;
    }

    private int GetAvailableAds()
    {
        for (int i = 0; i < ads.Length; i++)
        {
            if (Data.adTimers[i] > 0 || !ads[i].PassesCondition()) continue;

            availableAds.Add(ads[i]);
        }

        return availableAds.Count;
    }

    private void ResetPlayAdTimer()
    {
        Data.playAdTimer = UnityEngine.Random.Range(timeUntilAdMin, timeUntilAdMax);
    }

    private void InitializeTimers()
    {
        ResetPlayAdTimer();

        float[] tempTimers = new float[ads.Length];

        for (int i = 0; i < ads.Length; i++)
        {
            tempTimers[i] = ads[i].timerTime;
        }

        Data.adTimers = tempTimers;
    }

    public IEnumerator DisplayNoInternetPopup()
    {
        noInternetPopup.SetActive(true);

        yield return new WaitForSeconds(noInternetPopupTime);

        noInternetPopup.SetActive(false);
    }
}
