﻿using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using UnityEngine;
using System.Linq;
using UnityEngine.Events;

public class HomePayingMiniCoins : HomeState
{
    public static UnityEvent DonePayingMiniCoins = new UnityEvent();
    
    private Chest chest;
    private int payAmount;
    private int totalPaidCount, newPaidCount;
    private float leftOverTime;
    private int payCurrentFrameAmount;
    private int newCoinsCount;
    private int[] originalMiniCoins;
    private int firstCoinOnScreenIndex;
    private int coinsToSpawnAmount;
    private float payTime;
    private float percentageOfPaidThisFrame;

    private float cameraStartPosition;
    private float coinFloorPosition = -5.314369f;
    private MiniCoin miniCoin;
    private bool topMiniCoinInCenter;
    private float moveDownToCenterDistance;
    private float cameraMoveToCenterProgress;
    private float totalTime;
    private float timePassed;

    public HomePayingMiniCoins(HomeManager homeManager, Chest chest) : base(homeManager) 
    {
        this.chest = chest;
    }

    public override void Enter()
    {
        base.Enter();

        newCoinsCount = manager.newMiniCoins.Count;
        
        int[] miniCoinsInTube = Data.miniCoins;
        payAmount = 0;
        int coinValue = 0;
        for (int i = miniCoinsInTube.Length - 1; i >= 0; i--)
        {
            if (coinValue >= chest.price) break;
            
            coinValue += miniCoinsInTube[i] > 0 ? GameManager.I.gemBonusAmount : 1;
            payAmount++;
        }
        
        // for (int i = oldMiniCoins.Length - 1; i >= 0; i--)
        // {
        //     if (coinValue >= chest.price) break;
        //     
        //     coinValue += oldMiniCoins[i] > 0 ? GameManager.I.gemBonusAmount : 1;
        //     payAmount++;
        // }

        totalTime = 0;
        for (int i = 0; i < payAmount; i++)
        {
            totalTime += Mathf.Max(manager.miniCoinManager.payTime - manager.miniCoinManager.payTimePerCoin * i,
                    manager.miniCoinManager.payTimeMin);
        }
        totalTime += 0.27f - 0.0005f * payAmount;
        HomeManager.chestUnlockTime = totalTime;
        
        originalMiniCoins = Data.miniCoins;

        cameraStartPosition = manager.coinTubeManager.cameraTransform.position.y;
        
        Data.RemoveMiniCoins(payAmount);

        EventManager.PaidForChest.Invoke(chest);

        moveDownToCenterDistance = manager.coinTubeManager.camera.transform.position.y - (newCoinsCount != 0 ?
            manager.newMiniCoins[newCoinsCount - 1].transform.position.y :
            manager.oldMiniCoins[manager.miniCoinManager.indexLastOldCoinInList].transform.position.y);

        cameraMoveToCenterProgress = 0;
        if (moveDownToCenterDistance <= 0)
        {
            cameraMoveToCenterProgress = 1;
        }
    }

    public override void Update()
    {
        while (cameraMoveToCenterProgress < 1)
        {
            cameraMoveToCenterProgress = Mathf.Min(cameraMoveToCenterProgress + Time.deltaTime / manager.coinTubeManager.cameraMoveToCenterTime, 1);

            manager.coinTubeManager.cameraTransform.position = new Vector3(manager.coinTubeManager.cameraTransform.position.x,
                cameraStartPosition - moveDownToCenterDistance * (-(Mathf.Cos(Mathf.PI * cameraMoveToCenterProgress) - 1) / 2), 
                manager.coinTubeManager.cameraTransform.position.z);

            EventManager.CoinTubeCameraRepositioned.Invoke();

            while (manager.coinTubeManager.bottomRightOfScreen.y < GetHeightByIndexInTube(manager.miniCoinManager.indexFirstOldCoinInTube))
            {
                PlaceCoinAtBottomOfScreen();   
            }    

            return;
        }

        GetCoinAmountToPayThisFrame();

        for (int i = 0; i < payCurrentFrameAmount; i++)
        {
            // Get the percentage of where it is on the frame so that they each have a different start time
            // when entering the chest
            percentageOfPaidThisFrame = (payCurrentFrameAmount - i - 1) * (1 / (float)payCurrentFrameAmount);

            if (newPaidCount < newCoinsCount)
            {
                miniCoin = manager.newMiniCoins[newCoinsCount - 1 - newPaidCount];

                PayNewCoin(miniCoin, newCoinsCount - 1 - newPaidCount);
            }
            else
            {
                miniCoin = manager.oldMiniCoins[manager.miniCoinManager.indexLastOldCoinInList];

                PayOldCoin(miniCoin);

                manager.miniCoinManager.indexLastOldCoinInList--;
                if (manager.miniCoinManager.indexLastOldCoinInList < 0)
                {
                    manager.miniCoinManager.indexLastOldCoinInList = manager.oldMiniCoins.Count - 1;
                }
            }

            if (!topMiniCoinInCenter && miniCoin.transform.position.y <= manager.coinTubeManager.camera.transform.position.y)
            {
                topMiniCoinInCenter = true;
            }

            if (topMiniCoinInCenter)
            {
                MoveCameraDown();
            }

            while (manager.coinTubeManager.bottomRightOfScreen.y < GetHeightByIndexInTube(manager.miniCoinManager.indexFirstOldCoinInTube) &&
                    manager.coinTubeManager.bottomRightOfScreen.y > coinFloorPosition)
            {
                PlaceCoinAtBottomOfScreen();   
            }         

            totalPaidCount++;
            //Debug.Log("To pay: " + (payAmount - totalPaidCount).ToString());
            // if (totalPaidCount < payAmount) continue;
            //
            // manager.SetState(new HomePreviewingSkin(manager));
            // break;
        }

        timePassed += Time.deltaTime;
        if (timePassed < totalTime) return;
        
        DonePayingMiniCoins.Invoke();
        manager.SetState(new HomePreviewingSkin(manager));
    }

    private void GetCoinAmountToPayThisFrame()
    {
        payTime = Mathf.Max(manager.miniCoinManager.payTime - manager.miniCoinManager.payTimePerCoin * totalPaidCount, manager.miniCoinManager.payTimeMin);
        payCurrentFrameAmount = Mathf.Min(Mathf.FloorToInt((leftOverTime + Time.deltaTime) / payTime), payAmount - totalPaidCount);
        leftOverTime += Time.deltaTime - payCurrentFrameAmount * payTime;
    }

    private void PlaceCoinAtBottomOfScreen()
    {
        manager.miniCoinManager.indexFirstOldCoinInTube--;
        manager.miniCoinManager.indexFirstOldCoinInList--;
        if (manager.miniCoinManager.indexFirstOldCoinInList < 0)
        {
            manager.miniCoinManager.indexFirstOldCoinInList = manager.oldMiniCoins.Count - 1;
        }

        manager.oldMiniCoins[manager.miniCoinManager.indexFirstOldCoinInList].transform.position = 
                                                    new Vector3(manager.coinTubeManager.coinTubeVisual.transform.position.x, 
                                                    manager.coinTubeManager.bottomOfCoinTube.y + manager.offSetBottomCoinTube + 
                                                    manager.miniCoinManager.indexFirstOldCoinInTube * manager.miniCoinManager.inTubeSpacing, 
                                                    100 + manager.miniCoinManager.indexFirstOldCoinInTube * -0.001f);

        manager.oldMiniCoins[manager.miniCoinManager.indexFirstOldCoinInList].SetState(new MiniCoinInTube(manager.oldMiniCoins[manager.miniCoinManager.indexFirstOldCoinInList]));                                              
    
        manager.oldMiniCoins[manager.miniCoinManager.indexFirstOldCoinInList].renderer.material = GetMaterial(manager.miniCoinManager.indexFirstOldCoinInTube);
        
        manager.oldMiniCoins[manager.miniCoinManager.indexFirstOldCoinInList].SetCoinType(GetCoinType(manager.miniCoinManager.indexFirstOldCoinInTube));
    }   

    private Material GetMaterial(int indexInTube)
    {
        if (originalMiniCoins[indexInTube] == 0)
        {
            return manager.miniCoinManager.coinMaterial;
        }
        
        return manager.miniCoinManager.gemMaterials[originalMiniCoins[indexInTube] - GameManager.I.coinSkinAmount];
    }

    private CoinType GetCoinType(int indexInTube)
    {
        return originalMiniCoins[indexInTube] > 0 ? CoinType.Gem : CoinType.Coin;
    }

    private void MoveCameraDown()
    {
        manager.coinTubeManager.cameraTransform.position -=
            new Vector3(0, manager.miniCoinManager.inTubeSpacing * 
            (1 + totalPaidCount * manager.coinTubeManager.cameraDescendIncrease)
            + Random.Range(-manager.coinTubeManager.cameraDescendRandomness, manager.coinTubeManager.cameraDescendRandomness));

        manager.coinTubeManager.cameraTransform.localPosition = new Vector3(manager.coinTubeManager.cameraTransform.localPosition.x,
            Mathf.Max(manager.coinTubeManager.cameraTransform.localPosition.y, 0), manager.coinTubeManager.cameraTransform.localPosition.z);

        EventManager.CoinTubeCameraRepositioned.Invoke();
    }

    private void PayNewCoin(MiniCoin miniCoin, int index)
    {
        EventManager.MiniCoinRemovedFromTube.Invoke(miniCoin, totalPaidCount, percentageOfPaidThisFrame);

        // Pay all new mini coins
        miniCoin.SetState(new MiniCoinInactive(miniCoin));
        manager.newMiniCoins.RemoveAt(index);

        newPaidCount++;
    }

    private void PayOldCoin(MiniCoin miniCoin)
    {
        EventManager.MiniCoinRemovedFromTube.Invoke(miniCoin, totalPaidCount, percentageOfPaidThisFrame);

        // Pay all currently visible old mini coins
        miniCoin.SetState(new MiniCoinInactive(miniCoin));

        manager.oldCoinsPaidCurrentScreen++;
        manager.totalOldCoinsPaidCount++;
    }

    private float GetHeightByIndexInTube(int index)
    {
        return manager.coinTubeManager.bottomOfCoinTube.y + manager.offSetBottomCoinTube + index * manager.miniCoinManager.inTubeSpacing;
    }
}
