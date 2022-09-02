using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CoinType { Coin, Gem, Key, GhostCoin }

public class CoinManager : MonoBehaviour
{
    public bool testCoinsOnly, testSpawnGem, testSpawnKey;
    
    public PlayManager playManager;
    public CoinPileManager coinPileManager;
    public InstantiationManagerPlay instantiationManager;
    public HandManager handManager;

    public Transform coinHolder;
    public int startingCoinStackAmount;

    public float xForce, yForce;
    public int rotationSpeed; 
    public float chargeSpeed;
    public float maxChargeTimeLight;

    public float minMoveToScore;
    public float perfectHitAfterTouchingTime;
    public float adjustHandMinDistance;

    [HideInInspector] public List<Coin> Coins = new List<Coin>();
    [HideInInspector] public int newCoinIndex;

    [HideInInspector] public int spawnedCoinsCount;
    [HideInInspector] public bool spawnNoMoreKeys;

    private float coinPileBasePos = 3.016871f;
    private int scoredCoinsPrevious;
    private int scoredCoinsCurrent;
    private Vector3 pileTopPosition;
    private Coin flippedCoin;
    private bool shouldAdjustHorizontally;
    private bool shouldDescend;
    private bool handAdjusting;
    private bool handAdjusted;
    private int perfectHitCombo;

    void Awake()
    {
        EventManager.LoadingScreenSlidOut.AddListener(OnLoadingScreenSlidOut);
        EventManager.CoinFlips.AddListener(OnCoinFlips);
        EventManager.CoinTouchesPile.AddListener(OnCoinTouchesPile);
        EventManager.StageInitialized.AddListener(OnStageInitialized);
        EventManager.GoingGameOver.AddListener(OnGoingGameOver);
        EventManager.GoneGameOver.AddListener(OnGoneGameOver);
    }
    
    void Update()
    {
        if (GameManager.I.isGameOver) return;
        
        if (flippedCoin && (flippedCoin.State.GetIsScored() || flippedCoin.State.GetIsFallen() && coinPileManager.IsPileStill()))
        {
            GetCoinPileInfo(out scoredCoinsCurrent, out pileTopPosition);
            playManager.SetScore(scoredCoinsCurrent);

            shouldAdjustHorizontally = ShouldAdjustXPos();
            if (!handAdjusting && !handAdjusted && (shouldAdjustHorizontally || ShouldDescendHand()))
            {
                handAdjusting = true;
                handManager.AdjustHandPosition(shouldAdjustHorizontally ? Mathf.Min(pileTopPosition.x, coinPileBasePos) : 0, 
                    scoredCoinsPrevious - scoredCoinsCurrent, () =>
                {
                    handAdjusting = false;
                    handAdjusted = true;
                });
            }

            if (handAdjusting) return;

            if (scoredCoinsCurrent > scoredCoinsPrevious)
            {
                EventManager.CoinScores.Invoke(flippedCoin);
            }

            if (playManager.score < playManager.nextStageTarget)
            {
                SpawnCoin();
            }
            else
            {
                handManager.AscendHand(flippedCoin.transform.position, SpawnCoin);
                playManager.SetState(new PlayInitializingNextStage(playManager));
            }
            
            flippedCoin = null;
            handAdjusted = false;
            scoredCoinsPrevious = scoredCoinsCurrent;
        }

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

        if (GameManager.I.previousScene == -1)
        {
            PrepareForNewStage(false);      
        }
    }

    private void OnLoadingScreenSlidOut()
    {
        PrepareForNewStage(true);
    }

    private void OnCoinFlips(Coin coin, float chargeTime)
    {
        flippedCoin = coin;
        newCoinIndex = spawnedCoinsCount;
    }

    private void OnCoinTouchesPile(Coin coin)
    {
        StartCoroutine(WaitForPerfectHit(coin));
    }

    private void OnStageInitialized()
    {		
        PrepareForNewStage(true);
    }

    private void OnGoingGameOver()
    {
        GetFinalScoredCoins();
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

        // Despawn the coin not yet flipped
        Coins[newCoinIndex].SetState(new CoinInactive(Coins[newCoinIndex]));
        EventManager.CoinDespawns.Invoke(Coins[newCoinIndex]);
    }

    private void PrepareForNewStage(bool spawnNow)
    {
        if (!spawnNoMoreKeys && !GetCanSpawnKeys())
        {
            ReplaceInactiveKeys();
            spawnNoMoreKeys = true;
        }
        
        instantiationManager.InstantiateCoins(Mathf.Max(20 - (Coins.Count - spawnedCoinsCount), 0));

        if (!spawnNow) return;
        
        SpawnCoin();
    }

    public void SpawnCoin()
    {
        // If there are no more coins, instantiate new ones
        if (Coins.Count - spawnedCoinsCount == 0)
        {
            instantiationManager.InstantiateCoins(5);
        }

        print("spawn");
        spawnedCoinsCount++;

        Coins[newCoinIndex].SetState(new CoinSpawned(Coins[newCoinIndex]));

        EventManager.CoinSpawns.Invoke(Coins[newCoinIndex]);
    }
    
    private void GetCoinPileInfo(out int amount, out Vector3 topPosition)
    {
        amount = 0;
        topPosition = Coins[0].transform.position;
        foreach (Coin coin in Coins)
        {
            if (!coin.State.GetIsScored()) continue;
            
            amount++;
            topPosition = coin.transform.position.y > topPosition.y ? coin.transform.position : topPosition;
        }
    }

    private void GetFinalScoredCoins()
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

    private bool ShouldAdjustXPos() => Mathf.Abs(handManager.hand.position.x - (Mathf.Min(pileTopPosition.x, coinPileBasePos) - 0.8f)) > adjustHandMinDistance;

    private bool ShouldDescendHand() => scoredCoinsCurrent < scoredCoinsPrevious;
}
