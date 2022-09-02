using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeManager : MonoBehaviour
{
    [SerializeField] private bool testGetCoins;
    
    public InstantiationManagerHome instantiationManager;
    public MiniCoinManager miniCoinManager;
    public CoinTubeManager coinTubeManager;
    [SerializeField] private ChestManager chestManager;
    [SerializeField] private AdManager adManager;
    public GameObject homeHolder;
    public new Camera camera;
    public ParticleSystem effectGemBonusCoinPrefab;
    public AudioSource tubeFillLoopAudioSource;
    public AudioSource chestOpenAudioSource;
    public AudioSource unlockAudioSource;
    public AudioClip tubeFillClip;
    public AudioClip tubeFillStartClip;
    public AudioClip tubeFillEndClip;
    public AudioClip tubeGemBonusClip;
    public AudioClip tubeKeyDropClip;
    public AudioClip chestOpenClip;
    public AudioClip unlockClip;
    public static float chestUnlockTime;

    [SerializeField] private Button collectionButton;

    public List<int> bonusCoins = new List<int>();

    public HomeState State;
    [HideInInspector] public float screenWorldWidth;
    [HideInInspector] public int[] startOriginalMiniCoins;
    [HideInInspector] public List<MiniCoin> oldMiniCoins, oldMiniGems, newMiniCoins = new List<MiniCoin>();
    [HideInInspector] public int newMiniCoinsCount;
    [HideInInspector] public float offSetBottomCoinTube, offSetSideCoinTube;
    [HideInInspector] public int visibleOldCoinsAmount, totalOldCoinsPaidCount, oldCoinsPaidCurrentScreen;
    [HideInInspector] public bool loadingScreenSlidOut;
    [HideInInspector] public bool bonusCoinsIsAvailable;
    [HideInInspector] public List<MiniCoin> miniCoinsToWaitForBeforeDropping = new List<MiniCoin>();
    [HideInInspector] public int droppingMiniCoinsInQueue;
    [HideInInspector] public int totalSpawnedMiniCoins;

    void Start()
    {
        EventManager.EnterDefaultHome.AddListener(OnEnterDefaultHome);
        EventManager.LoadingScreenSlidOut.AddListener(OnLoadingScreenSlidOut);
        EventManager.EntersHome.AddListener(OnEntersHome);
        EventManager.EnteredHome.AddListener(OnEnteredHome);
        EventManager.BuysChest.AddListener(OnBuysChest);
        EventManager.ChestArrivesAtPayingPosition.AddListener(OnChestArrivesAtPayingPosition);
        EventManager.SwipesScreen.AddListener(OnSwipesScreen);
        collectionButton.onClick.AddListener(OnPressCollectionButton);

        //GameManager.I.scoredCoins = new List<int>(new int[] { 0,0,0,0,0,0,0,39,39,39,39,39,39,39,0,0,0,0,0,0,0,0 ,39,39,39,39,39,39,39,39,39,39,39,39,0, 39, 0, 0, 0, 41, 0, 0, 39, 0, 0, 39, 0, 0, 0, 41, 0, 0, 39, 0, 0, 39, 0, 0, 0, 41, 0, 0, 39, 0, 0, 39, 0, 0, 0, 41, 0, 0, 39, 0, 0, 39, 0, 0, 0, 41, 0, 0, 39, 0,} /*new int[10000]*/);

        if (testGetCoins)
        {
            GameManager.I.scoredCoins = new List<int>(new int[9900]);
        }

        screenWorldWidth = camera.ScreenToWorldPoint(new Vector3(Screen.width, 0, 0)).x - camera.ScreenToWorldPoint(Vector3.zero).x;

        startOriginalMiniCoins = Data.miniCoins;
        
        coinTubeManager.GetCameraWorldBoundaries();
        MeasureMiniCoin();

        SetState(new HomeDefault(this));

        coinTubeManager.Initialize();
        miniCoinManager.SpawnOldMiniCoins();
        chestManager.Initialize();
        adManager.Initialize();

        State.OnHomeSceneLoaded();

        // If started playing from home scene
        if (GameManager.I.previousScene == -1)
        {
            OnLoadingScreenSlidOut();
        }
    }

    void Update()
    {
        State.Update();
    }

    private void OnEnterDefaultHome()
    {
        if (GameManager.I.scoredCoins.Count == 0 || GameManager.I.previousScene == -1) return;

        //GameManager.I.AddMiniCoinGhosts();
        //GameManager.I.AddGemBonusMiniCoins();

        // Drop scored coins
        SetState(new HomeDroppingMiniCoins(this, GameManager.I.scoredCoins, true));
    }

    private void OnLoadingScreenSlidOut()
    {
        loadingScreenSlidOut = true;
        
        foreach (MiniCoin miniCoin in newMiniCoins)
        {
            miniCoin.State.OnLoadingScreenSlidOut();
        }  
    }

    private void OnEntersHome()
    {
        homeHolder.SetActive(true);
    }

    private void OnEnteredHome()
    {
        SetState(new HomeDefault(this));
    }

    private void OnBuysChest(Chest chest, bool chestIsPaidByAd)
    {
        State.OnBuysChest(chest.price, chestIsPaidByAd);
    }

    private void OnChestArrivesAtPayingPosition(Chest chest, bool chestIsPaidByAd)
    {
        if (chestIsPaidByAd) return;

        SetState(new HomePayingMiniCoins(this, chest));
    }

    public void PressPlayAgainButton()
    {
        State.PressPlayAgainButton();
    }

    private void OnPressCollectionButton()
    {
        State.PressCollectionButton();
    }

    private void OnSwipesScreen(bool rightOrLeft)
    {
        State.SwipeScreen(rightOrLeft);
    }

    public void PressBonusCoinsButton()
    {
        EventManager.PressedBonusCoinsButton.Invoke();
    }

    public void SetState(HomeState state)
    {
        if (State != null) State.Exit();
        State = state;
        State.Enter();
    }

    public IEnumerator WaitForLastCoinBeforeDroppingMiniCoins(int indexInQueue, List<int> identifiers, int forceOriginalMinicoinsCount)
    {
        while (miniCoinsToWaitForBeforeDropping.Count < indexInQueue || !miniCoinsToWaitForBeforeDropping[indexInQueue - 1].State.GetIsFinishedDropping())
        {
            yield return null;
        } 

        yield return null;

        SetState(new HomeDroppingMiniCoins(this, identifiers, false, forceOriginalMinicoinsCount));
    }

    private void MeasureMiniCoin()
    {
        MiniCoin measuringCoin = Instantiate(GameManager.I.miniCoinPrefab);
        offSetBottomCoinTube = (measuringCoin.transform.TransformPoint(measuringCoin.meshFilter.mesh.vertices[2]).y - 
                                measuringCoin.transform.TransformPoint(measuringCoin.meshFilter.mesh.vertices[0]).y) / 2;
        offSetSideCoinTube = (measuringCoin.transform.TransformPoint(measuringCoin.meshFilter.mesh.vertices[1]).x -
                                measuringCoin.transform.TransformPoint(measuringCoin.meshFilter.mesh.vertices[0]).x) / 2;
        Destroy(measuringCoin.gameObject);
    }
}
