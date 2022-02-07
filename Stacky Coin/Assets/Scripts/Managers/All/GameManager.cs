using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static GameManager I => instance;

    public bool testEnableAllSkins;
    public AudioSource audioSource;
    [SerializeField] private int FPSCap;
    [SerializeField] private float delayAfterGameOver;
    [SerializeField] private float delayBeforePlayingAgain;

    public float sceneTransitionTime;
    public float collectionHomeTransitionTime;
    public int gemBonusAmount, gemBonusCoinDelay;

    [HideInInspector] public Coin[] coinPrefabs;
    [HideInInspector] public Gem[] gemPrefabs;
    [HideInInspector] public Key[] keyPrefabs;
    [HideInInspector] public MiniCoinCoin miniCoinPrefab;
    [HideInInspector] public MiniCoinGhost miniCoinGhostPrefab;
    [HideInInspector] public MiniGem[] miniGemsPrefabs;
    [HideInInspector] public MiniKey[] miniKeysPrefabs;
    public int coinSkinAmount, commonSkinAmount, rareSkinAmount, epicSkinAmount;

    [HideInInspector] public int previousScene = -1;
    [HideInInspector] public int finalScore;
    [HideInInspector] public float finalPileHeight;
    [HideInInspector] public List<int> scoredCoins = new List<int>();
    [HideInInspector] public bool newHighScore;
    [HideInInspector] public float timePlayedLastPlay;
    [HideInInspector] public bool isGameOver;

    private int miniCoinsCount;
    private int[] chestsInData;
    private float timeStartedPlay;

    void Awake()
    {
        // Singleton pattern
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        } 

        SceneManager.sceneLoaded += OnSceneLoaded;
        EventManager.GoneGameOver.AddListener(OnGoneGameOver);
        EventManager.EnterDefaultHome.AddListener(OnEnterDefaultHome);
        EventManager.PlayingAgain.AddListener(OnPlayingAgain);

        instance = this;
        DontDestroyOnLoad(gameObject);

        // Set FPS cap
        Application.targetFrameRate = FPSCap;

        // Get all coin, gem and key prefabs
        coinPrefabs = Resources.LoadAll<Coin>("Prefabs/Play/Coins");
        gemPrefabs = Resources.LoadAll<Gem>("Prefabs/Play/Gems");
        keyPrefabs = Resources.LoadAll<Key>("Prefabs/Play/Keys");

        miniCoinPrefab = Resources.Load<MiniCoinCoin>("Prefabs/Home/Mini Coin");
        miniCoinGhostPrefab = Resources.Load<MiniCoinGhost>("Prefabs/Home/Mini Coin Ghost");
        miniGemsPrefabs = Resources.LoadAll<MiniGem>("Prefabs/Home/Gems");
        miniKeysPrefabs = Resources.LoadAll<MiniKey>("Prefabs/Home/Keys");

        chestsInData = Data.chests;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 0)
        {
            OnPlaySceneLoaded();
        }
        else if (scene.buildIndex == 1)
        {
            OnHomeSceneLoaded();
        }
    }

    private void OnPlaySceneLoaded()
    {
        // Check if it is the first time the player opens the game
        if (IsFirstTimePlaying())
        {
            print("New Game");

            Data.InitializePlayerPrefs();

            EventManager.FirstTimePlaying.Invoke();

            Data.firstTimePlaying = 0;
        }

        if (testEnableAllSkins)
        {
            Data.EnableAllCoinSkins();
        }

        timeStartedPlay = Time.time;
        isGameOver = false;
    }

    private void OnHomeSceneLoaded()
    {
        if (IsFirstTimeInHome())
        {
            EventManager.FirstTimeInHome.Invoke();
            
            Data.firstTimeInHome = 0;
        }

        timePlayedLastPlay = Time.time - timeStartedPlay;
    }

    private void OnEnterDefaultHome()
    {
        if (!newHighScore) return;

        EventManager.NewHighScore.Invoke();
        newHighScore = false;
    }

    private void OnGoneGameOver(bool manualGameOver)
    {
        StartCoroutine(TransitionToHome(!manualGameOver ? delayAfterGameOver : 0));

        CheckIfNewHighScore();

        CheckIfNewHighPileHeight();
    }

    private void OnPlayingAgain()
    {
        chestsInData = Data.chests;

        StartCoroutine(TransitionToPlay());
    }

    private IEnumerator TransitionToHome(float delay)
    {
        yield return new WaitForSeconds(delay);

        EventManager.TransitioningScenes.Invoke(sceneTransitionTime);

        yield return new WaitForSeconds(sceneTransitionTime);

        LoadScene(1);
    }

    private IEnumerator TransitionToPlay()
    {
        yield return new WaitForSeconds(delayBeforePlayingAgain);

        EventManager.TransitioningScenes.Invoke(sceneTransitionTime);

        yield return new WaitForSeconds(sceneTransitionTime);

        LoadScene(0);
    }

    public void LoadScene(int buildIndex)
    {
        previousScene = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadSceneAsync(buildIndex);
    }

    private void CheckIfNewHighScore()
    {
        if (finalScore <= Data.highScore) return;

        if (Data.highScore != 0)
        {
            newHighScore = true;
        }

        Data.highScore = finalScore;
    }

    private void CheckIfNewHighPileHeight()
    {
        if (finalPileHeight <= Data.highPileHeight) return;

        Data.highPileHeight = finalPileHeight;
    }

    public void AddMiniCoinGhosts()
    {
        miniCoinsCount = scoredCoins.Count;
        for (int i = miniCoinsCount - 1; i >= 0; i--)
        {
            if (scoredCoins[i] >= coinSkinAmount)
            {
                int ghostCount = Mathf.Max(gemBonusAmount + gemBonusCoinDelay - (miniCoinsCount - 1 - i), 0);

                for (int z = 0; z < ghostCount; z++)
                {
                    scoredCoins.Add(-1);
                }
            } 
        }
    } 

    public void AddGemBonusMiniCoins()
    {
        miniCoinsCount = scoredCoins.Count;
        for (int i = miniCoinsCount - 1; i >= 0; i--)
        {
            if (scoredCoins[i] >= coinSkinAmount)
            {
                for (int z = 0; z < gemBonusAmount; z++)
                {
                    scoredCoins.Insert(i, 0);
                }
            }
        }
    }  

    public void RemoveMiniKeysAndGhostCoins(List<int> identifiers)
    {
        miniCoinsCount = identifiers.Count;
        for (int i = miniCoinsCount - 1; i >= 0; i--)
        {
            if (identifiers[i] != 0 && identifiers[i] < coinSkinAmount || identifiers[i] == -1)
            {
                identifiers.RemoveAt(i);
            }
        }
    }

    public int GetAvailableChestSlotsAmount()
    {
        int count = 0;
        foreach (int chest in chestsInData)
        {
            if (chest > 0) continue;

            count++;
        }

        return count;
    }

    public bool IsFirstTimePlaying()
    {
        return !PlayerPrefs.HasKey("firstTimePlaying") || Data.firstTimePlaying == 1;
    }

    public bool IsFirstTimeInHome()
    {
        return !PlayerPrefs.HasKey("firstTimeInHome") || Data.firstTimeInHome == 1;
    }
}
