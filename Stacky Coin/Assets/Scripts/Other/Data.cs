using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Data
{
    private static int[] intArrayToSave, intArrayFromData;
    private static int newMiniCoinsCount;

    public static int firstTimePlaying { get { return PlayerPrefs.GetInt("firstTimePlaying"); } set { PlayerPrefs.SetInt("firstTimePlaying", value); } }
    public static int firstTimeInHome { get { return PlayerPrefs.GetInt("firstTimeInHome"); } set { PlayerPrefs.SetInt("firstTimeInHome", value); } }
    public static int highScore { get { return PlayerPrefs.GetInt("highScore"); } set { PlayerPrefs.SetInt("highScore", value); } }
    public static int[] ownedCoins { get { return PlayerPrefsX.GetIntArray("ownedCoins"); } set { PlayerPrefsX.SetIntArray("ownedCoins", value); } }
    public static int[] ownedGems { get { return PlayerPrefsX.GetIntArray("ownedGems"); } set { PlayerPrefsX.SetIntArray("ownedGems", value); } }
    public static int[] unseenNewSkins { get { return PlayerPrefsX.GetIntArray("unseenNewSkins"); } set { PlayerPrefsX.SetIntArray("unseenNewSkins", value); } }
    public static int[] miniCoins { get { return PlayerPrefsX.GetIntArray("miniCoins"); } set { PlayerPrefsX.SetIntArray("miniCoins", value); } }
    public static int[] chests { get { return PlayerPrefsX.GetIntArray("chests"); } set { PlayerPrefsX.SetIntArray("chests", value); } }
    public static float playAdTimer { get { return PlayerPrefs.GetFloat("playAdTimer"); } set { PlayerPrefs.SetFloat("playAdTimer", value); } }
    public static float[] adTimers { get { return PlayerPrefsX.GetFloatArray("adTimers"); } set { PlayerPrefsX.SetFloatArray("adTimers", value); } }
    public static float bonusCoinsTimer { get { return PlayerPrefs.GetFloat("bonusCoinsTimer"); } set { PlayerPrefs.SetFloat("bonusCoinsTimer", value); } }
    public static int tutorialLevel { get { return PlayerPrefs.GetInt("tutorialLevel"); } set { PlayerPrefs.SetInt("tutorialLevel", value); } }
    public static float highPileHeight { get { return PlayerPrefs.GetFloat("highPileHeight"); } set { PlayerPrefs.SetFloat("highPileHeight", value); } }
    public static int tripleAdRewardUsed { get { return PlayerPrefs.GetInt("tripleAdRewardUsed"); } set { PlayerPrefs.SetInt("tripleAdRewardUsed", value); } }
    
    
    public static void InitializePlayerPrefs()
    {
        highScore = 0;
        miniCoins = new int[0];
        chests = new int[3] {0, 0, 0};
        ownedCoins = new int[1] {0};
        ownedGems = new int[] {390/*, 400, 420*/};
        unseenNewSkins = new int[0];
        playAdTimer = 0;
        bonusCoinsTimer = 0;
        tutorialLevel = 0;
        highPileHeight = -1;
        tripleAdRewardUsed = 0;
    }

    public static void AddMiniCoins(List<int> newMiniCoins)
    {
        intArrayFromData = miniCoins;
        newMiniCoinsCount = newMiniCoins.Count;
        intArrayToSave = new int[intArrayFromData.Length + newMiniCoinsCount];

        // Add the existing minicoins from data
        for (int i = 0; i < intArrayFromData.Length; i++)
        {
            intArrayToSave[i] = intArrayFromData[i];
        }

        // Add the new minicoins on top
        for (int i = 0; i < newMiniCoinsCount; i++)
        {
            intArrayToSave[intArrayFromData.Length + i] = newMiniCoins[i];
        }

        miniCoins = intArrayToSave;
    }

    public static void RemoveMiniCoins(int amount)
    {
        intArrayFromData = miniCoins;
        intArrayToSave = new int[intArrayFromData.Length - amount];

        for (int i = 0, count = intArrayToSave.Length; i < count; i++)
        {
            intArrayToSave[i] = intArrayFromData[i];
        }

        miniCoins = intArrayToSave;
    }

    public static void SetChest(int price, int position)
    {
        intArrayToSave = chests;
        intArrayToSave[position] = price;
        chests = intArrayToSave;
    }
    
    public static void EnableAllCoinSkins()
    {
        intArrayToSave = new int[GameManager.I.coinSkinAmount];
        for (int i = 0; i < intArrayToSave.Length; i++)
        {
            intArrayToSave[i] = GameManager.I.coinPrefabs[i].number;
        }
        ownedCoins = intArrayToSave;

        intArrayToSave = new int[GameManager.I.gemPrefabs.Length];
        for (int i = 0; i < intArrayToSave.Length; i++)
        {
            intArrayToSave[i] = GameManager.I.gemPrefabs[i].number;
        }
        ownedGems = intArrayToSave;
    }

    public static void AddOwnedCoinSkin(int coinId, bool coinOrGem, out bool isDuplicateSkin)
    {   
        intArrayFromData = coinOrGem ? ownedCoins : ownedGems;
        isDuplicateSkin = false;

        // Check if it is already owned
        foreach (int coin in intArrayFromData)
        {
            if (coin == coinId)
            {
                isDuplicateSkin = true;
                break;
            }
        }

        if (isDuplicateSkin) return;

        intArrayToSave = new int[intArrayFromData.Length + 1];

        for (int i = 0; i < intArrayToSave.Length - 1; i++)
        {
            intArrayToSave[i] = intArrayFromData[i];
        }

        intArrayToSave[intArrayToSave.Length - 1] = coinId;

        if (coinOrGem)
        {
            ownedCoins = intArrayToSave;
        }
        else
        {
            ownedGems = intArrayToSave;
        }

        AddUnseenNewskin(coinId, coinOrGem);

        isDuplicateSkin = false;
    }

    private static void AddUnseenNewskin(int id, bool coinOrGem)
    {
        intArrayFromData = unseenNewSkins;
        intArrayToSave = new int[intArrayFromData.Length + 1];

        for (int i = 0; i < intArrayToSave.Length - 1; i++)
        {
            intArrayToSave[i] = intArrayFromData[i];
        }

        intArrayToSave[intArrayToSave.Length - 1] = coinOrGem ? id : id + GameManager.I.coinSkinAmount;

        unseenNewSkins = intArrayToSave;
    }

    public static void RemoveAllUnseenNewSkins()
    {
        unseenNewSkins = new int[0];
    }
}
