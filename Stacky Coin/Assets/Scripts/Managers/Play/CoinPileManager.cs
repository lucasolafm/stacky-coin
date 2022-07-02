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
    public float lenghtFalling, lenghtAboutToFall;
    [SerializeField] private int unstableCoinsAmountToGameOver;
    [SerializeField] private float timeRequirePileStillness;

    private int unstableCoinsAmount;
    private bool pileIsStill;
    private float timePileIsStill;
    private List<Coin> fallenCoins = new List<Coin>();
    private bool coinUnstable, coinFalling;
    private bool canBeStraightened;

    void Start()
    {
        EventManager.CoinTouchesPile.AddListener(OnCoinTouchesPile);
        EventManager.CoinFallsWhileTouchingPile.AddListener(OnCoinFallsWhileTouchingPile);
        EventManager.GoingGameOver.AddListener(OnGoingGameOver);
    }

    void Update()
    {
        unstableCoinsAmount = 0;
        pileIsStill = true;
        fallenCoins.Clear();
        for (int i = 0; i < coinManager.spawnedCoinsCount; i++)
        {
            if (!coinManager.Coins[i].State.GetIsStillOnPile())
            {
                pileIsStill = false;
            }

            coinManager.Coins[i].State.GetStabilityOnPile(out coinUnstable, out coinFalling);
            
            if (!coinUnstable && !coinFalling) continue;

            unstableCoinsAmount += i > tutorialManager.tutorialObjectsSpawned ? 1 : 0;
            if (unstableCoinsAmount >= unstableCoinsAmountToGameOver)
            {
                EventManager.CoinPileFallsOver.Invoke();
                return;
            }
            
            if (!coinFalling) continue;
            
            fallenCoins.Add(coinManager.Coins[i]);
        }

        foreach (Coin fallenCoin in fallenCoins)
        {
            SetKinematic(false);
            
            fallenCoin.SetState(new CoinFalling(fallenCoin));
        }

        if (pileIsStill)
        {
            timePileIsStill += Time.deltaTime;
        }
        else
        {
            timePileIsStill = 0;
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

    public bool IsPileStill()
    {
        return timePileIsStill > timeRequirePileStillness;
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
                if (addOrRemove)
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
}
