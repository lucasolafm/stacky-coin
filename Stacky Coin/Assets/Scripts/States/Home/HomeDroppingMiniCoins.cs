using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeDroppingMiniCoins : HomeState
{
    private List<int> identifiers = new List<int>(), previousGemIndexes = new List<int>(), identifiersToSave = new List<int>();
    private bool saveToData;
    private int forceOriginalMiniCoinsCount;
    private List<List<int>> coinIdentifiers = new List<List<int>>();
    private int identifiersCount, originalMiniCoinsCount, newMiniCoinsCount;
    private int previousCoinsInTube;
    private int gemCount, keyCount;
    private MiniCoin miniCoin;
    private int spawnIndex, inTubeIndex;
    private int nextGemIndex = -1;
    private Vector3 spawnPos;
    private float inTubeHeight;

    public HomeDroppingMiniCoins(HomeManager manager, List<int> identifiers, bool saveToData, int forceOriginalMiniCoinsCount = -1) : base(manager) 
    {
        this.identifiers = identifiers;
        this.saveToData = saveToData;
        this.forceOriginalMiniCoinsCount = forceOriginalMiniCoinsCount;
    }

    public override bool CanDropMiniCoins()
    {
        return true;
    }

    public override void Enter()
    {
        base.Enter();

        if (forceOriginalMiniCoinsCount == -1)
        {
            originalMiniCoinsCount = Data.miniCoins.Length;
        }
        else
        {
            originalMiniCoinsCount = forceOriginalMiniCoinsCount;
        }

        AddCoinsToDrop(identifiers);

        manager.StartCoroutine(SpawningNewMiniCoins());
    }

    public override void DropMiniCoins(List<int> identifiers)
    {
        manager.droppingMiniCoinsInQueue++;

        manager.StartCoroutine(manager.WaitForLastCoinBeforeDroppingMiniCoins(manager.droppingMiniCoinsInQueue, identifiers, Data.miniCoins.Length));

        SaveCoinsToData(identifiers);
    }

    public override void PressPlayAgainButton()
    {
        base.PressPlayAgainButton();

        manager.SetState(new HomeEnteringPlay(manager));
    }

    public override void PressCollectionButton()
    {
        base.PressCollectionButton();

        manager.SetState(new HomeEnteringCollection(manager));
    }

    public override void SwipeScreen(bool rightOrLeft)
    {
        if (rightOrLeft == false) return;

        manager.SetState(new HomeEnteringCollection(manager));
    }

    private IEnumerator SpawningNewMiniCoins()
    {
        for (int i = 0; i < coinIdentifiers.Count; i++)
        {
            identifiersCount = coinIdentifiers[i].Count;

            newMiniCoinsCount = manager.newMiniCoins.Count;

            GetMiniCoinsCurrentlyInTube(i);

            EventManager.SpawningNewMiniCoins.Invoke();
            
            for (int z = 0; z < identifiersCount; z++)
            {
                manager.totalSpawnedMiniCoins++;

                InstantiateMiniCoin(i, z);

                GetNextGemIndex(i, z);

                SaveCurrentGemIndex(z);   

                // Get the index for spawning and falling
                spawnIndex = z - gemCount * GameManager.I.gemBonusAmount;   

                GetInTubeIndex(z);

                GetSpawnPos(z);

                GetInTubeHeight(z);

                if (z >= nextGemIndex)
                {
                    if (!manager.loadingScreenSlidOut)
                    {
                        // Set the start en end heights of the dropping coin
                        miniCoin.SetState(new MiniCoinAwaitingDropping(miniCoin, spawnPos, inTubeHeight));   
                    }                 
                    else
                    {
                        miniCoin.SetState(new MiniCoinDropping(miniCoin, spawnPos, inTubeHeight));   
                    }
                } 
                else
                {
                    // If this is a gem's bonus coin, place it directly in the tube as inactive
                    miniCoin.SetState(new MiniCoinGemBonus(miniCoin, new Vector3(spawnPos.x, inTubeHeight, spawnPos.z)));
                }  

                CountKeys(i, z);
            }    

            // Save the new minicoins count
            manager.newMiniCoinsCount = manager.newMiniCoins.Count;

            manager.miniCoinsToWaitForBeforeDropping.Add(miniCoin);

            // Wait for the last coin to finish dropping
            while (!miniCoin.State.GetIsFinishedDropping())
            {
                yield return null;
            }                    

            // Wait for all processes to finish
            yield return new WaitForEndOfFrame();

            RemoveGhostsAndKeysFromLocalList();
        }

        // Reset the queue
        if (manager.miniCoinsToWaitForBeforeDropping.Count > manager.droppingMiniCoinsInQueue)
        {
            manager.miniCoinsToWaitForBeforeDropping.Clear();
            manager.droppingMiniCoinsInQueue = 0;
        }

        manager.SetState(new HomeDefault(manager));
    }

    private void AddCoinsToDrop(List<int> identifiers)
    {
        coinIdentifiers.Add(identifiers);

        if (saveToData)
        {
            SaveCoinsToData(identifiers);
        }
    }

    private void SaveCoinsToData(List<int> newIdentifiers)
    {
        Debug.Log("save to data");

        // Save mini coin identifiers to the data without the keys and ghost coins
        identifiersToSave = new List<int>(newIdentifiers);
        GameManager.I.RemoveMiniKeysAndGhostCoins(identifiersToSave);
        Data.AddMiniCoins(identifiersToSave);
    }

    private void InstantiateMiniCoin(int i, int z)
    {
        miniCoin = manager.instantiationManager.InstantiateMiniObject(coinIdentifiers[i][z]);     
        miniCoin.indexInList = newMiniCoinsCount + z;           
        manager.newMiniCoins.Add(miniCoin);     
    }

    private void GetNextGemIndex(int i, int z)
    {
        if (identifiersCount > z + GameManager.I.gemBonusAmount && 
            coinIdentifiers[i][z + GameManager.I.gemBonusAmount] >= GameManager.I.coinSkinAmount)
        {
            nextGemIndex = z + GameManager.I.gemBonusAmount;
        }
    }

    private void SaveCurrentGemIndex(int z)
    {
        if (z == nextGemIndex)
        {
            previousGemIndexes.Add(z - (gemCount + 1) * GameManager.I.gemBonusAmount);
            gemCount++;
        }   
    }

    private void GetInTubeIndex(int z)
    {
        inTubeIndex = z - keyCount; 
        foreach (int gemIndex in previousGemIndexes)
        {
            // Move up for each gem based on the distance to that gem
            inTubeIndex -= Mathf.Clamp(GameManager.I.gemBonusAmount - 
                                        (spawnIndex - gemIndex - ((z >= nextGemIndex ? GameManager.I.gemBonusCoinDelay : 0) - 1)), 
                                        0, GameManager.I.gemBonusAmount);
        }
    }

    private void GetSpawnPos(int z)
    {
        spawnPos = new Vector3(manager.coinTubeManager.bottomRightOfScreen.x - manager.offSetSideCoinTube, 
                                manager.coinTubeManager.topOfScreen.y + spawnIndex * manager.miniCoinManager.dropSpacing, 
                                100 + /*(previousCoinsInTube + z + 1)*/ manager.totalSpawnedMiniCoins * -0.001f);       
    }

    private void GetInTubeHeight(int z)
    {
        inTubeHeight = manager.coinTubeManager.bottomOfCoinTube.y + manager.offSetBottomCoinTube + 
                        (previousCoinsInTube + inTubeIndex) * manager.miniCoinManager.inTubeSpacing;
    }

    private void CountKeys(int i, int z)
    {
        if (coinIdentifiers[i][z] != 0 && coinIdentifiers[i][z] < GameManager.I.coinSkinAmount)
        {
            keyCount++;          
        } 
    }

    private void GetMiniCoinsCurrentlyInTube(int i)
    {
        previousCoinsInTube = originalMiniCoinsCount;
        for (int z = i - 1; z >= 0; z--)
        {
            previousCoinsInTube += coinIdentifiers[z].Count;
        }
    }

    private void RemoveGhostsAndKeysFromLocalList()
    {
        for (int z = identifiersCount - 1; z >= 0; z--)
        {
            if (manager.newMiniCoins[newMiniCoinsCount + z].GetCoinType() != CoinType.Key && 
                manager.newMiniCoins[newMiniCoinsCount + z].GetCoinType() != CoinType.GhostCoin) continue;

            manager.newMiniCoins.RemoveAt(newMiniCoinsCount + z);
        }
    }
}