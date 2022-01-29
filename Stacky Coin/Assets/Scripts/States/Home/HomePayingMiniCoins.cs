using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HomePayingMiniCoins : HomeState
{
    private Chest chest;
    private int totalPaidCount, newPaidCount;
    private float leftOverTime;
    private int payCurrentFrameAmount;
    private int newCoinsCount, oldCoinsCount;
    private int[] originalMiniCoins;
    private int firstCoinOnScreenIndex;
    private int coinsToSpawnAmount;
    private float payTime;
    private float percentageOfPaidThisFrame;

    private float cameraStartPosition;
    private float cameraMinPosition;
    private float coinFloorPosition = -5.314369f;
    private MiniCoin miniCoin;
    private bool topMiniCoinInCenter;
    private float moveDownToCenterDistance;
    private float cameraMoveToCenterProgress;

    public HomePayingMiniCoins(HomeManager homeManager, Chest chest) : base(homeManager) 
    {
        this.chest = chest;
    }

    public override void Enter()
    {
        base.Enter();

        newCoinsCount = manager.newMiniCoins.Count;
        oldCoinsCount = manager.oldMiniCoins.Count;
        originalMiniCoins = Data.miniCoins;

        cameraStartPosition = manager.coinTubeManager.cameraTransform.position.y;
        cameraMinPosition = manager.coinTubeManager.bottomOfCoinTube.y + (manager.coinTubeManager.topOfScreen.y - manager.coinTubeManager.bottomRightOfScreen.y) / 2;

        Data.RemoveMiniCoins(chest.price);

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
                Debug.Log("placing at bottom");
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
            if (totalPaidCount < chest.price) continue;

            manager.SetState(new HomePreviewingSkin(manager));
            break;
        }
    }

    private void GetCoinAmountToPayThisFrame()
    {
        payTime = Mathf.Max(manager.miniCoinManager.payTime - manager.miniCoinManager.payTimePerCoin * totalPaidCount, manager.miniCoinManager.payTimeMin);
        payCurrentFrameAmount = Mathf.FloorToInt((leftOverTime + Time.deltaTime) / payTime);
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
                                                    new Vector3(manager.coinTubeManager.bottomRightOfScreen.x - manager.offSetSideCoinTube, 
                                                    manager.coinTubeManager.bottomOfCoinTube.y + manager.offSetBottomCoinTube + 
                                                    manager.miniCoinManager.indexFirstOldCoinInTube * manager.miniCoinManager.inTubeSpacing, 
                                                    100 + manager.miniCoinManager.indexFirstOldCoinInTube * -0.001f);

        manager.oldMiniCoins[manager.miniCoinManager.indexFirstOldCoinInList].SetState(new MiniCoinInTube(manager.oldMiniCoins[manager.miniCoinManager.indexFirstOldCoinInList]));                                              
    
        manager.oldMiniCoins[manager.miniCoinManager.indexFirstOldCoinInList].renderer.material = GetMaterial(manager.miniCoinManager.indexFirstOldCoinInTube);
    }   

    private Material GetMaterial(int indexInTube)
    {
        //Debug.Log("array count: "+ originalMiniCoins.Length);
        Debug.Log("index: "+indexInTube);
        if (originalMiniCoins[indexInTube] == 0)
        {
            return manager.miniCoinManager.coinMaterial;
        }
        else
        {
            return manager.miniCoinManager.gemMaterials[originalMiniCoins[indexInTube] - GameManager.I.coinSkinAmount];
        }
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

    private void PlaceCoinsOnNewScreenBelow()
    {
        // Spawn new old coins on the screen below
        firstCoinOnScreenIndex = manager.coinTubeManager.GetFirstCoinOnScreenIndex(manager.coinTubeManager.bottomRightOfScreen.y - 
                                                            (manager.coinTubeManager.topOfScreen.y - manager.coinTubeManager.bottomRightOfScreen.y) * 
                                                            manager.coinTubeManager.cameraMoveScreenPercent); 
                                
        coinsToSpawnAmount = manager.startOriginalMiniCoins.Length - manager.totalOldCoinsPaidCount - firstCoinOnScreenIndex;

        manager.visibleOldCoinsAmount = coinsToSpawnAmount;
        manager.oldCoinsPaidCurrentScreen = 0;

        // Add gems
        manager.miniCoinManager.AddMiniGemsToScreen();
        oldCoinsCount = manager.oldMiniCoins.Count;

        // Place the coins on the screen below
        for (int z = 0; z < coinsToSpawnAmount; z++)
        {
            manager.oldMiniCoins[oldCoinsCount - coinsToSpawnAmount + z].transform.position = 
                                                    new Vector3(manager.coinTubeManager.bottomRightOfScreen.x - manager.offSetSideCoinTube, 
                                                    manager.coinTubeManager.bottomOfCoinTube.y + manager.offSetBottomCoinTube + 
                                                    (firstCoinOnScreenIndex + z) * manager.miniCoinManager.inTubeSpacing, 
                                                    10 + (firstCoinOnScreenIndex + z) * -0.001f);

            manager.oldMiniCoins[oldCoinsCount - coinsToSpawnAmount + z].SetState(new MiniCoinInTube(manager.oldMiniCoins[oldCoinsCount - coinsToSpawnAmount + z]));
        }
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
