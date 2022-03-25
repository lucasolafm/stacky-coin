using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinManager : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    public CoinTubeManager coinTubeManager;
    [SerializeField] private InstantiationManagerHome instantiationManager;
    public ChestManager chestManager;
    public Camera mainCamera, keyCamera;
    
    public Material[] gemMaterials;
    public Material coinMaterial;

    public float dropSpacing, inTubeSpacing;
    public float startDropSpeed, dropSpeedMultiplier;
    public float payTime, payTimePerCoin, payTimeMin;

    public float keyExpandStartSize, keyExpandSize, keyExpandTime;
    public float keyHoverLength, keyHoverTime;
    public float keyOutlineOutTime, keyOutlineSize, keyOutlineDelay;
    public float keyMoveToSlotTime; 
    
    private MiniCoin miniCoin;
    private int firstCoinOnScreenIndex;
    private int coinsFitOnScreenAmount;
    private int oldMiniCoinsCount, oldMiniGemsCount;
    private int addedGemCount;

    [HideInInspector] public int indexFirstOldCoinInTube;
    [HideInInspector] public int indexFirstOldCoinInList;
    [HideInInspector] public int indexLastOldCoinInList;

    public void SpawnOldMiniCoins()
    {
        homeManager.totalSpawnedMiniCoins = homeManager.startOriginalMiniCoins.Length;

        coinsFitOnScreenAmount = Mathf.CeilToInt((coinTubeManager.topOfScreen.y - coinTubeManager.bottomRightOfScreen.y) / inTubeSpacing);

        firstCoinOnScreenIndex = Mathf.FloorToInt(Mathf.Max(coinTubeManager.bottomRightOfScreen.y - 
                                                    (coinTubeManager.bottomOfCoinTube.y + homeManager.offSetBottomCoinTube), 0) / inTubeSpacing);

        homeManager.visibleOldCoinsAmount = homeManager.startOriginalMiniCoins.Length - firstCoinOnScreenIndex;                        

        // Spawn mini coins
        for (int i = 0; i < coinsFitOnScreenAmount * 3; i++)
        {
            miniCoin = instantiationManager.InstantiateMiniObject(0);
            homeManager.oldMiniCoins.Add(miniCoin);

            miniCoin.SetState(new MiniCoinInactive(miniCoin));
        }

        // Spawn mini gems
        for (int i = 0; i < instantiationManager.miniGemInstantiateAmount; i++)
        {
            miniCoin = instantiationManager.InstantiateMiniObject(GameManager.I.coinSkinAmount);
            homeManager.oldMiniGems.Add(miniCoin);

            miniCoin.SetState(new MiniCoinInactive(miniCoin));
        }

        AddMiniGemsToScreen();
        oldMiniCoinsCount = homeManager.oldMiniCoins.Count;

        indexFirstOldCoinInTube = firstCoinOnScreenIndex;
        indexFirstOldCoinInList = homeManager.oldMiniCoins.Count - homeManager.visibleOldCoinsAmount;
        indexLastOldCoinInList = homeManager.oldMiniCoins.Count - 1;

        // Place the minicoins on their positions and enable them
        for (int i = 0; i < homeManager.visibleOldCoinsAmount; i++)
        {
            miniCoin = homeManager.oldMiniCoins[oldMiniCoinsCount - homeManager.visibleOldCoinsAmount + i];

            // Place the minicoins that will be visible on the screen
            miniCoin.transform.position = new Vector3(coinTubeManager.coinTubeVisual.transform.position.x, 
                                                        coinTubeManager.bottomOfCoinTube.y + homeManager.offSetBottomCoinTube + 
                                                        (firstCoinOnScreenIndex + i) * inTubeSpacing, 
                                                        100 + (firstCoinOnScreenIndex + i) * -0.001f);

            miniCoin.SetState(new MiniCoinInTube(miniCoin));
        }
    }

    public void AddMiniGemsToScreen()
    {
        oldMiniCoinsCount = homeManager.oldMiniCoins.Count;
        oldMiniGemsCount = homeManager.oldMiniGems.Count;

        // First remove all mini gems from the list
        for (int i = oldMiniCoinsCount - 1; i >= 0; i--)
        {
            if (homeManager.oldMiniCoins[i].GetCoinType() == 0) continue;

            homeManager.oldMiniCoins[i].SetState(new MiniCoinInactive(homeManager.oldMiniCoins[i]));
            homeManager.oldMiniCoins.RemoveAt(i);
        }
        addedGemCount = 0;

        // Add gems to the list as they are in the data
        for (int i = 0; i < homeManager.visibleOldCoinsAmount; i++)
        {
            if (homeManager.startOriginalMiniCoins[homeManager.startOriginalMiniCoins.Length - 1 - homeManager.totalOldCoinsPaidCount - i] == 0) continue;

            // Set material
            homeManager.oldMiniGems[addedGemCount].renderer.material = gemMaterials[homeManager.startOriginalMiniCoins[homeManager.startOriginalMiniCoins.Length - 1 - 
                                                                                    homeManager.totalOldCoinsPaidCount - i] - GameManager.I.coinSkinAmount];

            if (i > 0)
            {
                // Insert a gem
                homeManager.oldMiniCoins.Insert(homeManager.oldMiniCoins.Count - 1 - i + 1, homeManager.oldMiniGems[addedGemCount]);
            }
            else
            {
                // Add to the list if it is the top one
                homeManager.oldMiniCoins.Add(homeManager.oldMiniGems[addedGemCount]);
            }
            
            addedGemCount++;
        }
    }

    public int GetNextNewMiniCoinIndex(int index)
    {
        index++;
        if (homeManager.newMiniCoinsCount > index + GameManager.I.gemBonusAmount && 
            homeManager.newMiniCoins[index + GameManager.I.gemBonusAmount].GetCoinType() == CoinType.Gem)
        {
            index += GameManager.I.gemBonusAmount;
        }

        return index;
    }
}
