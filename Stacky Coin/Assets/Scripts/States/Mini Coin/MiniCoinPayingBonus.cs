using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinPayingBonus : MiniCoinState
{
    private int nextMiniCoinToLandIndex;
    private int pushIndex;
    private int coinsLandedCount;
    private int newMiniCoinsCount;
    private WaitForEndOfFrame waitForEndOfFrame = new WaitForEndOfFrame();
    private MiniCoin bonusMiniCoin;

    public MiniCoinPayingBonus(MiniCoin miniCoin) : base(miniCoin) {}

    public override bool GetHasLanded()
    {
        return true;
    }

    public override void Enter()
    {
        base.Enter();

        nextMiniCoinToLandIndex = miniCoin.indexInList;
        pushIndex = miniCoin.indexInList;    
        newMiniCoinsCount = miniCoin.homeManager.newMiniCoins.Count;
        
        nextMiniCoinToLandIndex = miniCoin.miniCoinManager.GetNextNewMiniCoinIndex(nextMiniCoinToLandIndex);

        miniCoin.StartCoroutine(SpawnBonusCoins());
    }

    private IEnumerator SpawnBonusCoins()
    {
        int coinsSpawnedCount = 0;

        while (coinsLandedCount - GameManager.I.gemBonusCoinDelay + 1 != GameManager.I.gemBonusAmount)
        {
            yield return waitForEndOfFrame;

            // Wait for the next coin to land
            if (!miniCoin.homeManager.newMiniCoins[nextMiniCoinToLandIndex].State.GetHasLanded()) continue;

            nextMiniCoinToLandIndex = miniCoin.miniCoinManager.GetNextNewMiniCoinIndex(nextMiniCoinToLandIndex);

            // Get the next coin that the gem should push up
            pushIndex = miniCoin.miniCoinManager.GetNextNewMiniCoinIndex(pushIndex);

            // Wait until an amount of coins have landed before moving
            coinsLandedCount++;
            if (coinsLandedCount < GameManager.I.gemBonusCoinDelay) continue;

            bonusMiniCoin = miniCoin.homeManager.newMiniCoins[miniCoin.indexInList - GameManager.I.gemBonusAmount + coinsLandedCount - GameManager.I.gemBonusCoinDelay];

            // Enable the next bonus coin
            bonusMiniCoin.SetState(new MiniCoinInTube(bonusMiniCoin));

            CoinSpawnEffect(bonusMiniCoin.transform, coinsSpawnedCount);

            coinsSpawnedCount++;

            // Move the gem up as the bonus coins are enabled            
            miniCoin.transform.Translate(Vector3.up * miniCoin.miniCoinManager.inTubeSpacing, Space.World);

            // Push the coins above the gem up
            for (int z = miniCoin.indexInList + 1; z < pushIndex; z++)
            {
                // Don't push up if it's a key or if it hasn't been enabled yet
                if (miniCoin.homeManager.newMiniCoins[z].GetCoinType() == CoinType.Key || 
                    !miniCoin.homeManager.newMiniCoins[z].State.GetHasLanded()) continue;

                miniCoin.homeManager.newMiniCoins[z].transform.Translate(Vector3.up * miniCoin.miniCoinManager.inTubeSpacing, Space.World);
            }

            EventManager.MiniCoinAddedToTube.Invoke();
        }

        miniCoin.SetState(new MiniCoinInTube(miniCoin));
    }

    private void CoinSpawnEffect(Transform bonusMiniCoin, int index)
    {
        miniCoin.bonusCoinEffects[index].transform.position = bonusMiniCoin.transform.position + new Vector3(0, -0.031f, 0);

        miniCoin.bonusCoinEffects[index].transform.parent = bonusMiniCoin;

        miniCoin.bonusCoinEffects[index].transform.localScale = new Vector3(1, 1, 1);

        miniCoin.bonusCoinEffects[index].gameObject.SetActive(true);

        miniCoin.bonusCoinEffects[index].Play();
    }
}
