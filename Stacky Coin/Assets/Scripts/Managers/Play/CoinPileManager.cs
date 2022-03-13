using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinPileManager : MonoBehaviour
{
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private TutorialManager tutorialManager;

    public Collider[] invisibleWalls;

    public float coinDrag;
    [SerializeField] private int kinematicAt;
    [SerializeField] private float kinematicStraighteningMaxAngle;

    [HideInInspector] public float angleFalling;
    public float lenghtFalling, lenghtAboutToFall, fallDetectTime;
    [SerializeField] private int unstableCoinsAmountToGameOver;

    private float startTime;
    private int unstableCoinsAmount;
    private List<Coin> fallenCoins = new List<Coin>();
    private bool coinUnstable, coinFalling;
    private bool canBeStraightened;

    void Start()
    {
        EventManager.CoinTouchesPile.AddListener(OnCoinTouchesPile);
        EventManager.CoinFallsWhileTouchingPile.AddListener(OnCoinFallsWhileTouchingPile);
        EventManager.GoingGameOver.AddListener(OnGoingGameOver);

        startTime = Time.time;
    }

    void Update()
    {
        if (Time.time - startTime < fallDetectTime) return;
        startTime += fallDetectTime;

        unstableCoinsAmount = 0;
        fallenCoins.Clear();
        for (int i = 0; i < coinManager.spawnedCoinsCount; i++)
        {
            // Check the stability of coins on the pile
            coinManager.Coins[i].State.GetStabilityOnPile(out coinUnstable, out coinFalling);

            // Count the amount of unstable coins
            if (coinUnstable || coinFalling)
            {
                if (i > tutorialManager.tutorialObjectsSpawned)
                {
                    unstableCoinsAmount++;
                }

                // Check if the pile is going to fall
                if (unstableCoinsAmount >= unstableCoinsAmountToGameOver)
                {
                    EventManager.CoinPileFallsOver.Invoke();
                }
            }

            // Check if the coin should fall off
            if (coinFalling)
            {
                fallenCoins.Add(coinManager.Coins[i]);
            }
        }

        // Deal with fallen coins only after game over has been determined
        foreach (Coin fallenCoin in fallenCoins)
        {
            SetKinematic(false);
            
            fallenCoin.SetState(new CoinFalling(fallenCoin));
        }

        if (fallenCoins.Count == 0) return;
        
        EventManager.CoinsFallOffPile.Invoke(fallenCoins.ToArray());
    }

    private void OnCoinTouchesPile(Coin coin)
    {
        SetKinematic(true);
    }    

    private void OnCoinFallsWhileTouchingPile()
    {
        SetKinematic(false);
    }

    private void OnGoingGameOver()
    {
        EnableAllCoinColliders();
    }

    private void SetKinematic(bool addOrRemove)
    {
        int onPileCount = 0;
        for (int i = coinManager.spawnedCoinsCount - 1; i >= 0; i--)
        {
            // Count the coins on the pile from top to bottom
            if (!coinManager.Coins[i].State.GetIsPartOfPile()) continue;
            
            if (onPileCount == kinematicAt)
            {
                if (addOrRemove == true)
                {
                    canBeStraightened = coinManager.Coins[i].transform.eulerAngles.z > -180 - kinematicStraighteningMaxAngle &&
                                        coinManager.Coins[i].transform.eulerAngles.z < 180 + kinematicStraighteningMaxAngle;

                    coinManager.Coins[i].SetState(new CoinOnPileKinematic(coinManager.Coins[i], canBeStraightened, 
                        !coinManager.Coins[i].State.GetIsScored()));
                }
                else
                {
                    coinManager.Coins[i].SetState(new CoinOnPile(coinManager.Coins[i], !coinManager.Coins[i].State.GetIsScored()));
                }   

                // If there are no coins below this one, break
                if (i == 0) break;
            }
            else if (onPileCount == kinematicAt + 1)
            {
                // Disable or enable the collider of the coin below 
                coinManager.Coins[i].collider.enabled = !addOrRemove;

                break;
            }

            onPileCount++;
        }
    }

    private void EnableAllCoinColliders()
    {
        for (int i = 0; i < coinManager.spawnedCoinsCount; i++)
        {
            coinManager.Coins[i].collider.enabled = true;
        }
    }

    public bool GetIsCoinTooSteep(Coin coin)
    {
        return coin.transform.eulerAngles.z < 180 - angleFalling || coin.transform.eulerAngles.z > 180 + angleFalling;
    }
}
