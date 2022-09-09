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
    private MiniCoin firstCoinToFall;
    private MiniCoin lastRealMiniCoinToFall;

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

                spawnIndex = z;

                inTubeIndex = z - keyCount;

                GetSpawnPos(z);

                GetInTubeHeight(z);
     
                if (!manager.loadingScreenSlidOut)
                {
                    // Set the start en end heights of the dropping coin
                    miniCoin.SetState(new MiniCoinAwaitingDropping(miniCoin, spawnPos, inTubeHeight));   
                }                 
                else
                {
                    miniCoin.SetState(new MiniCoinDropping(miniCoin, spawnPos, inTubeHeight));   
                }
                    
                CountKeys(i, z);
            }    

            // Save the new minicoins count
            manager.newMiniCoinsCount = manager.newMiniCoins.Count;

            manager.miniCoinsToWaitForBeforeDropping.Add(miniCoin);

            while (!firstCoinToFall.State.GetHasLanded())
            {
                yield return null;
            }

            manager.tubeFillLoopAudioSource.clip = manager.tubeFillClip;
            manager.tubeFillLoopAudioSource.volume = 0.9f;
            manager.tubeFillLoopAudioSource.Play();
            GameManager.I.audioSource.PlayOneShot(manager.tubeFillEndClip, 0.5f);

            while (!lastRealMiniCoinToFall.State.GetHasLanded())
            {
                yield return null;
            }

            GameManager.I.audioSource.PlayOneShot(manager.tubeFillStartClip, 0.4f);
            manager.tubeFillLoopAudioSource.Stop();
            
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

        if (z == 0)
        {
            firstCoinToFall = miniCoin;
        }
        
        if (coinIdentifiers[i][z] != -1)
        {
            lastRealMiniCoinToFall = miniCoin;
        }
    }

    private void GetSpawnPos(int z)
    {
        spawnPos = new Vector3(manager.coinTubeManager.coinTubeVisual.transform.position.x, 
                                manager.coinTubeManager.topOfScreen.y + spawnIndex * manager.miniCoinManager.dropSpacing, 
                                100 + manager.totalSpawnedMiniCoins * -0.001f);       
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
