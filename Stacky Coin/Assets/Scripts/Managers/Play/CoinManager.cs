using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CoinType { Coin, Gem, Key, GhostCoin }

public class CoinManager : MonoBehaviour
{
    public bool testCoinsOnly, testSpawnGem, testSpawnKey;
    
    public PlayManager playManager;
    public InstantiationManagerPlay instantiationManager;
    public HandManager handManager;

    public Transform coinHolder;
    public int startingCoinStackAmount;
    public float spawnTime;

    public float xForce, yForce;
    public int rotationSpeed; 
    public float chargeSpeed;
    public float maxChargeTimeLight;

    public float minMoveToScore;
    public float perfectHitAfterTouchingTime;

    [HideInInspector] public List<Coin> Coins = new List<Coin>();
    [HideInInspector] public int newCoinIndex;

    [HideInInspector] public int spawnedCoinsCount;
    [HideInInspector] public Coroutine spawnCoinRoutine;
    [HideInInspector] public bool spawnNoMoreKeys;

    private Coin flippedCoin;
    private int perfectHitCombo;
    private WaitForSeconds spawnCoinWait;

    void Awake()
    {
        EventManager.LoadingScreenSlidOut.AddListener(OnLoadingScreenSlidOut);
        EventManager.CoinFlips.AddListener(OnCoinFlips);
        EventManager.CoinTouchesPile.AddListener(OnCoinTouchesPile);
        EventManager.CoinScores.AddListener(OnCoinScores);
        EventManager.CoinFalls.AddListener(OnCoinFalls);
        EventManager.StageInitialized.AddListener(OnStageInitialized);
        EventManager.GoingGameOver.AddListener(OnGoingGameOver);
        EventManager.GoneGameOver.AddListener(OnGoneGameOver);
    }

    void Update()
    {
        for (int i = 0; i < instantiationManager.instantiateCoinsAmount; i++)
        {
            Coins[i].State.Update();
        }
        
        if (testSpawnGem)
        {
            Coin gem = instantiationManager.InstantiateObject(CoinType.Gem);
            gem.SetState(new CoinInactive(gem));
            Coins.Remove(gem);
            Coins.Insert(spawnedCoinsCount, gem);
            testSpawnGem = false;    
        }
        else if (testSpawnKey)
        {
            Coin key = instantiationManager.InstantiateObject(CoinType.Key);
            key.SetState(new CoinInactive(key));
            Coins.Remove(key);
            Coins.Insert(spawnedCoinsCount, key);
            testSpawnKey = false; 
        }
    }

    public void Initialize()
    {
        instantiationManager.GetCoinSkinData();
        instantiationManager.InitializeStartingCoinStack();

        if (!GetCanSpawnKeys())
        {
            spawnNoMoreKeys = true;
        }
        instantiationManager.InstantiateCoins(instantiationManager.instantiateCoinsAmount);

        newCoinIndex = startingCoinStackAmount;  

        spawnCoinWait = new WaitForSeconds(spawnTime);

        if (GameManager.I.previousScene == -1)
        {
            PrepareForNewStage();      
        }
    }

    private void OnLoadingScreenSlidOut()
    {
        PrepareForNewStage();
    }

    private void OnCoinFlips(Coin coin, float chargeTime)
    {
        flippedCoin = coin;
        newCoinIndex = spawnedCoinsCount;
    }

    private void OnCoinScores(Coin coin)
    {
        if (coin != flippedCoin) return;
        
        if (playManager.score < playManager.nextStageTarget - 1)
        {
            spawnCoinRoutine = StartCoroutine(SpawnCoin(0));
        }
        else
        {
            playManager.SetState(new PlayLastCoinToNextStage(playManager, coin));
        }
    }

    private void OnCoinFalls(Coin coin)
    {
        if (coin != flippedCoin) return;
        
        spawnCoinRoutine = StartCoroutine(SpawnCoin(0));
    }

    private void OnCoinTouchesPile(Coin coin)
    {
        StartCoroutine(WaitForPerfectHit(coin));
    }

    private void OnStageInitialized()
    {		
        PrepareForNewStage();
    }

    private void OnGoingGameOver()
    {
        GetScoredCoins();
    }

    private void OnGoneGameOver(bool manualGameOver)
    {
        if (!manualGameOver)
        {
            // Make all coins fall
            for (int i = 0; i < spawnedCoinsCount; i++)
            {
                Coins[i].SetState(new CoinFalling(Coins[i]));
            }
        }

        if (spawnCoinRoutine != null) 
        {
            // Stop the next coin from spawning
            StopCoroutine(spawnCoinRoutine); 
        }

        // Despawn the coin not yet flipped
        Coins[newCoinIndex].SetState(new CoinInactive(Coins[newCoinIndex]));
        EventManager.CoinDespawns.Invoke(Coins[newCoinIndex]);
    }

    private void PrepareForNewStage()
    {
        if (!spawnNoMoreKeys && !GetCanSpawnKeys())
        {
            ReplaceInactiveKeys();
            spawnNoMoreKeys = true;
        }

        // Instantiate new coins until you have enough
        instantiationManager.InstantiateCoins(Mathf.Max(20 - (Coins.Count - spawnedCoinsCount), 0));

        // Spawn coin
        StartCoroutine(SpawnCoin(0));
    }

    public IEnumerator SpawnCoin(float delay)
    {
        // If there are no more coins, instantiate new ones
        if (Coins.Count - spawnedCoinsCount == 0)
        {
            instantiationManager.InstantiateCoins(5);
        }
        
        if (delay == spawnTime)
        {
            yield return spawnCoinWait;
        }
        else
        {
            yield return new WaitForSeconds(delay);
        }

		spawnedCoinsCount++;

        Coins[newCoinIndex].SetState(new CoinSpawned(Coins[newCoinIndex]));

        EventManager.CoinSpawns.Invoke(Coins[newCoinIndex]);
    }

    private void GetScoredCoins()
    {
        // Count all scored coins
        GameManager.I.scoredCoins.Clear();
        for (int i = startingCoinStackAmount; i < spawnedCoinsCount; i++)
        {
            if (!Coins[i].State.GetIsScored()) continue;

            // Get identifiers based on coin type
            switch (Coins[i].type)
            {
                case CoinType.Coin:
                    GameManager.I.scoredCoins.Add(0); break;
                case CoinType.Gem:
                    GameManager.I.scoredCoins.Add(GameManager.I.coinSkinAmount + Coins[i].GetId()); break;
                case CoinType.Key:
                    GameManager.I.scoredCoins.Add(Coins[i].GetId()); break;                    
            }
        }
    }

    private IEnumerator WaitForPerfectHit(Coin coin)
    {
        float startTime = Time.time;

        // Wait until the coin has gotten the chance to fully fall down after touching the pile
        while (!coin.perfectHit.didPerfectHit && Time.time - startTime < perfectHitAfterTouchingTime)
        {
            yield return null;
        }

        if (coin.perfectHit.didPerfectHit)
        {
            EventManager.PerfectHit.Invoke(coin, perfectHitCombo);

            perfectHitCombo++;
        }
        else
        {
            perfectHitCombo = 0;
        }
    } 

    public int GetScoredKeysAmount()
    {
        int count = 0;
        foreach (Coin coin in Coins)
        {
            if (coin.type != CoinType.Key || !coin.State.GetIsScored()) continue;

            count++;
        }

        return count;
    }

    private void ReplaceInactiveKeys()
    {
        Coin coin;

        for (int i = spawnedCoinsCount, count = Coins.Count; i < count; i++)
        {
            if (Coins[i].type != CoinType.Key) continue;

            Coins.RemoveAt(i);

            coin = instantiationManager.InstantiateObject(instantiationManager.GetRandomCoinType(), i);
            coin.SetState(new CoinInactive(coin));
        }
    }

    private bool GetCanSpawnKeys()
    {
        return GameManager.I.GetAvailableChestSlotsAmount() > 0 && 
                GameManager.I.GetAvailableChestSlotsAmount() - GetScoredKeysAmount() > 0;
    }
}
