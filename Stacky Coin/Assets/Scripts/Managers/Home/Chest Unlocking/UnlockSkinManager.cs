using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.PlayerLoop;

public class UnlockSkinManager : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    [SerializeField] private CollectionManager collectionManager;
    public new Camera camera;
    public Camera collectionCamera;
    [SerializeField] private Camera playAgainCoinCamera;
    [SerializeField] private MiniCoin miniCoinPrefab;
    [SerializeField] private Transform payingCoinsHolder;
    public Transform unlockChest;
    public SpriteRenderer unlockGlow;
    public Transform textNew;
    public SpriteRenderer unlockChestRenderer;
    public SpriteRenderer unlockChestChargeEffect;
    public Sprite[] chestSprites, chestChargeEffectSprites;
    [SerializeField] private int instantiatePayingCoinsAmount;
    [SerializeField] private float duplicateBonusCoinsPercent;

    public ChestChargeUpInfo info;
    public Renderer background;
    public RectTransform collectionButton;
    public Camera collectionUICamera;
    public Camera mainCamera;
    public GameObject collectionLightsFull, collectionLightsGreyscale;
    public Transform duplicateScreen;
    public TextMeshPro duplicateBonusCoinsText;
    public Material duplicateCoinMaterial;
    public Color[] glowColors;
    
    private UnlockChestPreparer chestPreparer;
    private UnlockMiniCoinPayer miniCoinPayer;
    private UnlockSkinPreviewer skinPreviewer;
    [HideInInspector] public Vector3 chestPayingPosition;
    [HideInInspector] public MiniCoin[] payingCoins;
    [HideInInspector] public Vector3 payingCoinStartScale, payingCoinEndScale;
    private int currentChestPrice;
    private float amountPaid;
    [HideInInspector] public Vector3 chestStartScale;
    private Vector3 expandingStartScale;
    private bool isChargingChest;
    private float scalingProgress;
    private Coroutine pivotingChestRoutine;
    [HideInInspector] public Skin previewSkinCoin; 
    private int unlockedSkinId;
    private bool isDuplicateSkin;
    private float timeUntilClipClimax = 2.313f;
    public Chest boughtChest;
    private bool donePayingMiniCoins;
    private bool chestPreparedForOpening;

    void Start()
    {
        chestPreparer = new UnlockChestPreparer(this);
        miniCoinPayer = new UnlockMiniCoinPayer(this);
        skinPreviewer = new UnlockSkinPreviewer(this);

        EventManager.BuysChest.AddListener(OnBuysChest);
        EventManager.MiniCoinRemovedFromTube.AddListener(OnMiniCoinRemovedFromTube);
        EventManager.PaidForChest.AddListener(OnPaidForChest);
        HomePayingMiniCoins.DonePayingMiniCoins.AddListener(OnDonePayingMiniCoins);

        InstantiatePayingCoins();

        chestPayingPosition = camera.ScreenToWorldPoint(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 5));
        unlockGlow.transform.position = chestPayingPosition;
        textNew.position = new Vector3(chestPayingPosition.x, chestPayingPosition.y - 0.35f, textNew.position.z);
        
        payingCoinStartScale = payingCoins[0].transform.localScale;
        payingCoinEndScale = payingCoinStartScale * info.payingCoinEndScalePercent;

        chestStartScale = unlockChest.localScale;
    }
    
    private void OnBuysChest(Chest boughtChest, bool chestIsPaidByAd)
    {
        this.boughtChest = boughtChest;
        currentChestPrice = boughtChest.price;
        amountPaid = 0;
        donePayingMiniCoins = false;
        isChargingChest = false;
        chestPreparedForOpening = false;

        StartCoroutine(chestPreparer.PrepareChest(boughtChest, chestIsPaidByAd, () =>
        {
            chestPreparedForOpening = true;
        }));

        unlockedSkinId = (!chestIsPaidByAd ? GetRandomSkin(boughtChest.level) : GetRandomUnownedSkin(boughtChest.level));

        skinPreviewer.InstantiatePreviewSkinCoin(collectionManager.GetCoinSkin(unlockedSkinId), isDuplicateSkin);
    }

    private void OnPaidForChest(Chest chest)
    {
        UnlockSkin(chest);
    }

    private void OnMiniCoinRemovedFromTube(MiniCoin miniCoin, int paidCount, float percentageOfPaidThisFrame)
    {
        miniCoinPayer.CoinEnteringChest(miniCoin, paidCount, percentageOfPaidThisFrame);
    }

    public void OnMiniCoinEntersChest(CoinType type)
    {
        amountPaid += type == CoinType.Gem ? GameManager.I.gemBonusAmount : 1;

        if (!isChargingChest)
        {
            StartCoroutine(PlayOpenChestClips());
            
            StartCoroutine(ChargeToOpenChest());
        }
    }
    
    private void OnDonePayingMiniCoins()
    {
        donePayingMiniCoins = true;
        OpenChest(false);
    }

    public void UnlockSkin(Chest boughtChest)
    {
        Data.AddOwnedCoinSkin(unlockedSkinId, boughtChest.level < 3, out isDuplicateSkin);

        if (isDuplicateSkin) return;

        EventManager.UnlockedNewCoinSkin.Invoke(unlockedSkinId);
    }

    public Vector3 GetChestPositionOnScreen(Chest chest)
    {
        Vector2 screenPosOtherCam = playAgainCoinCamera.WorldToScreenPoint(chest.transform.position);
        return camera.ScreenToWorldPoint(screenPosOtherCam);
    }

    private IEnumerator ChargeToOpenChest()
    {
        yield return StartCoroutine(ChargingChest(() => 
        {
            //OpenChest(false);
        }));
    }

    private IEnumerator ChargingChest(Action completed)
    {
        isChargingChest = true;

        StartCoroutine(ShakingChest());

        pivotingChestRoutine = StartCoroutine(PivotingChest());

        yield return StartCoroutine(ExpandingChest());

        StopCoroutine(pivotingChestRoutine);
        unlockChest.eulerAngles = Vector3.zero;

        completed();
    }

    public void OpenChest(bool chestIsPaidByAd)
    {
        StartCoroutine(WaitForChestPreparation(() =>
        {
            if (chestIsPaidByAd)
            {
                homeManager.chestOpenAudioSource.Stop();
                homeManager.chestOpenAudioSource.clip = homeManager.chestOpenAdClip;
                homeManager.chestOpenAudioSource.time = 0;
                homeManager.chestOpenAudioSource.volume = 1f;
                homeManager.chestOpenAudioSource.Play();
            }

            unlockChestRenderer.enabled = false;

            StartCoroutine(ExplodingChest());
            StartCoroutine(ChestFadingOut());

            StartCoroutine(PreviewingSkinCoin());
        }));
    }

    private IEnumerator WaitForChestPreparation(Action completed)
    {
        while (!chestPreparedForOpening) yield return null;

        completed();
    }

    private IEnumerator PreviewingSkinCoin()
    {
        if (isDuplicateSkin)
        {
            skinPreviewer.SetGreyscaleMaterial();
        }

        skinPreviewer.ToggleLights(true, isDuplicateSkin);

        int duplicateBonusCoins = Mathf.RoundToInt(currentChestPrice * duplicateBonusCoinsPercent);

        if (isDuplicateSkin) StartCoroutine(skinPreviewer.DisplayDuplicateScreen(duplicateBonusCoins));
        
        StartCoroutine(skinPreviewer.PivotPreviewSkinCoin());

        yield return StartCoroutine(skinPreviewer.EnlargePreviewSkinCoin(isDuplicateSkin));

        yield return new WaitForSeconds(info.previewCoinMoveDelay);

        StartCoroutine(chestPreparer.BackgroundFade(false));
        StartCoroutine(chestPreparer.GlowFade());

        homeManager.SetState(new HomeWithdrawingSkin(homeManager));

        if (!isDuplicateSkin)
        {
            yield return StartCoroutine(skinPreviewer.MovePreviewSkinCoin());

            StartCoroutine(skinPreviewer.CollectionButtonBounce());

            EventManager.SkinPreviewEntersCollectionButton.Invoke();

            homeManager.SetState(new HomeDefault(homeManager));
        }
        else
        {
            yield return StartCoroutine(skinPreviewer.ShrinkPreviewSkinCoin());

            homeManager.SetState(new HomeDroppingMiniCoins(homeManager, new List<int>(new int[duplicateBonusCoins]), true));
        }

        skinPreviewer.ToggleLights(false, isDuplicateSkin);
    }

    private IEnumerator ExpandingChest()
    {        
        float expandTime;
        float expandScale;
        float t;

        while (!donePayingMiniCoins)
        {
            scalingProgress = amountPaid / currentChestPrice * (amountPaid / currentChestPrice);

            if (scalingProgress >= 1) break;

            expandingStartScale = chestStartScale * (1 + (info.chestScaleMin + scalingProgress * (info.chestScaleMax - info.chestScaleMin)));

            expandTime = info.chestExpandTimeMin + scalingProgress * (info.chestExpandTimeMax - info.chestExpandScaleMin);
            expandScale = info.chestExpandScaleMin + scalingProgress * (info.chestExpandScaleMax - info.chestExpandScaleMin);

            t = 0;
            while (t < 1 && !donePayingMiniCoins)
            {
                t = Mathf.Min(t + Time.deltaTime / expandTime, 1);

                unlockChest.localScale = expandingStartScale * (1 + (t * expandScale)); 

                yield return null;
            }
        }
    }

    private IEnumerator ShakingChest()
    {
        float progress;

        while (amountPaid < currentChestPrice && !donePayingMiniCoins)
        {
            progress = amountPaid / currentChestPrice;

            unlockChest.position = chestPayingPosition + (Vector3)UnityEngine.Random.insideUnitCircle * (info.chestShakeSizeMin + 
                                    progress * (info.chestShakeSizeMax - info.chestShakeSizeMin));

            unlockChestChargeEffect.color = new Color(1, 1, 1, progress);

            yield return null;
        }
    }

    private IEnumerator PivotingChest()
    {
        float pivotingTime;
        bool leftOrRight = false;
        float pivotTime;
        float pivotLength;

        while (amountPaid < currentChestPrice && !donePayingMiniCoins)
        {
            pivotingTime = 0;
            leftOrRight = !leftOrRight;

            pivotTime = info.chestPivotTimeMin + amountPaid / currentChestPrice * (info.chestPivotTimeMax - info.chestPivotTimeMin);
            pivotLength = info.chestPivotLengthMin + amountPaid / currentChestPrice * (info.chestPivotLengthMax - info.chestPivotLengthMin);

            while (pivotingTime < 1 && !donePayingMiniCoins)
            {
                pivotingTime = Mathf.Min(pivotingTime + Time.deltaTime / pivotTime, 1);

                unlockChest.eulerAngles = new Vector3(0, 0, (leftOrRight ? pivotLength : -pivotLength) * 0.5f +
                                                        pivotLength * (leftOrRight ? -1 : 1) * (-(Mathf.Cos(Mathf.PI * pivotingTime) - 1) / 2));

                yield return null;
            }
        }
    }

    private IEnumerator ExplodingChest()
    {        
        float t = 0;

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / info.chestExplodeTime, 1);

            unlockChest.localScale = chestStartScale * (1 + info.chestScaleMax) * (1 + (t * info.chestExplodeScale));

            yield return null;
        }
    }

    private IEnumerator ChestFadingOut()
    {
        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / info.chestFadeOutTime, 1);

            unlockChestChargeEffect.color = new Color(1, 1, 1, 1 - t);

            yield return null;
        }
    }

    private void InstantiatePayingCoins()
    {
        payingCoins = new MiniCoin[instantiatePayingCoinsAmount];

        for (int i = 0; i < instantiatePayingCoinsAmount; i++)
        {
            payingCoins[i] = Instantiate(miniCoinPrefab, payingCoinsHolder);
            payingCoins[i].unlockSkinManager = this;
            payingCoins[i].SetState(new MiniCoinInactive(payingCoins[i]));
            payingCoins[i].renderer.enabled = true;
            payingCoins[i].gameObject.layer = 28;
        }
    }

    private IEnumerator PlayOpenChestClips()
    {
        AudioSettings.GetDSPBufferSize(out int bufferLength, out int numBuffers);
        float latency = (float) bufferLength / AudioSettings.outputSampleRate;

        float clipStartTime = Mathf.Max(-Mathf.Min(HomeManager.chestUnlockTime - (timeUntilClipClimax + latency), 0), 0);
        print(clipStartTime);
        
        homeManager.chestOpenAudioSource.Stop();
        homeManager.chestOpenAudioSource.clip = homeManager.chestOpenClip;
        homeManager.chestOpenAudioSource.time = clipStartTime;
        homeManager.chestOpenAudioSource.volume = 1f;

        homeManager.unlockAudioSource.Stop();
        homeManager.unlockAudioSource.clip = homeManager.unlockClip;
        homeManager.unlockAudioSource.time = clipStartTime;
        homeManager.unlockAudioSource.volume = 1f;

        homeManager.tubeFillLoopAudioSource.Stop();
        homeManager.tubeFillLoopAudioSource.clip = homeManager.tubeFillClip;
        homeManager.tubeFillLoopAudioSource.volume = 1f;
        homeManager.tubeFillLoopAudioSource.Play();

        yield return new WaitForSeconds(HomeManager.chestUnlockTime - (timeUntilClipClimax + latency));

        if (clipStartTime > 0)
        {
            StartCoroutine(IncreaseOpenChestClipVolumeGradually());
        }
        
        homeManager.tubeFillLoopAudioSource.Stop();
        homeManager.chestOpenAudioSource.Play();
        homeManager.unlockAudioSource.Play();
    }

    private IEnumerator IncreaseOpenChestClipVolumeGradually()
    {
        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / 0.5f, 1);

            homeManager.chestOpenAudioSource.volume = 0.8f * t;

            yield return null;
        }
    }

    private int GetRandomSkin(int chestLevel)
    {
        switch (chestLevel)
        {
            case 1:
                return collectionManager.skins[UnityEngine.Random.Range(1, GameManager.I.commonSkinAmount)].number;
            case 2:
                return collectionManager.skins[UnityEngine.Random.Range(0, GameManager.I.rareSkinAmount) + GameManager.I.commonSkinAmount].number;
            default:
                return collectionManager.skins[UnityEngine.Random.Range(1, GameManager.I.epicSkinAmount) + GameManager.I.commonSkinAmount + GameManager.I.rareSkinAmount].number;
        }
    }

    private int GetRandomUnownedSkin(int chestLevel)
    {
        List<int> unownedSkins = new List<int>();

        if (chestLevel == 2)
        {
            unownedSkins = GetUnownedSkins(Data.ownedCoins, GameManager.I.commonSkinAmount, GameManager.I.commonSkinAmount + GameManager.I.rareSkinAmount);
        }
        else
        {
            unownedSkins = GetUnownedSkins(Data.ownedGems, GameManager.I.commonSkinAmount + GameManager.I.rareSkinAmount, 
                                            GameManager.I.commonSkinAmount + GameManager.I.rareSkinAmount + GameManager.I.epicSkinAmount);
        }

        if (unownedSkins.Count > 0)
        {
            return unownedSkins[UnityEngine.Random.Range(0, unownedSkins.Count)];
        }
        else
        {
            return GetRandomSkin(chestLevel);
        }
    }   

    private List<int> GetUnownedSkins(int[] ownedSkins, int startValue, int endValue)
    {
        List<int> unownedSkins = new List<int>();
        bool owned;

        for (int i = startValue; i < endValue; i++)
        {
            owned = false;
            for (int z = 0; z < ownedSkins.Length; z++)
            {
                if (collectionManager.skins[i].number != ownedSkins[z]) continue;

                owned = true;
                break;
            }

            if (owned) continue;

            unownedSkins.Add(collectionManager.skins[i].number);
        }

        return unownedSkins;
    }
}
