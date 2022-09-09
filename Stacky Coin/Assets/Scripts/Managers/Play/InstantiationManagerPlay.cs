using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiationManagerPlay : MonoBehaviour
{
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private CoinPileManager coinPileManager;

    [SerializeField] private Transform treasurePile;

    [SerializeField] private Transform treasurePileTopCoin;
    public int instantiateCoinsAmount;
    [SerializeField] private float gemChance;
    [SerializeField] private int keyCoinRatio;
    [SerializeField] private float keyRatioMultNoChests;

	private TreasurePileCoin[] treasurePileCoins;
	private CombineInstance[] combine;
    private Coin coin;
    private int[] ownedCoins, ownedGems;
    private int[] enabledCoinSkins, enabledGemSkins, chests;
    private int keyLevel;
    private int rng;
    private int coinNumber;
    private int nextKeyIndex;
    private int indexToInstatiate;

    public void GetCoinSkinData()
    {
        ownedCoins = Data.ownedCoins;
        ownedGems = Data.ownedGems;
        chests = Data.chests;
        nextKeyIndex = Random.Range(0, GetKeyCoinRatio());
    }

    public void InitializeStartingCoinStack()
    {
        CombineStaticCoinPile();
        SpawnStartingCoinStack();
    }

    public void InstantiateCoins(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            coin = InstantiateObject(!coinManager.testCoinsOnly ? GetRandomCoinType() : CoinType.Coin);

            coin.SetState(new CoinInactive(coin));
        }

        if (coinManager.spawnNoMoreKeys) return;
        InsertKeys();
    }

    public Coin InstantiateObject(CoinType type, int forceIndex = -1, int forceNumber = -1)
    {
        switch (type)
        {
            case CoinType.Coin:
                coin = InstantiateCoin(); break;
            case CoinType.Gem:
                coin = InstantiateGem(forceNumber); break;                
            case CoinType.Key:
                coin = InstantiateKey(forceNumber); break;
        }

        coin.coinManager = coinManager;
        coin.coinPileManager = coinPileManager;

        if (forceIndex == -1)
        {
            coinManager.Coins.Add(coin);
        }
        else
        {
            coinManager.Coins.Insert(forceIndex, coin);
        }

        return coin;
    }

    private void InsertKeys()
    { 
        // Check if the key should be inserted in this batch of instantiated coins
        // Also check if there room for another chest, keeping in mind the other keys already scored
        while (nextKeyIndex < coinManager.Coins.Count - coinManager.startingCoinStackAmount)
        {
            coin = InstantiateObject(CoinType.Key, coinManager.startingCoinStackAmount + nextKeyIndex);

            coin.SetState(new CoinInactive(coin));

            nextKeyIndex += 5 + Random.Range(0, GetKeyCoinRatio() - 5);
        }
    }

    private Coin InstantiateCoin()
    {
        indexToInstatiate = Random.Range(0, GameManager.I.coinPrefabs.Length);

        coinNumber = GameManager.I.coinPrefabs[indexToInstatiate].number;

        return Instantiate(GameManager.I.coinPrefabs[GetDoesArrayContain(ownedCoins, coinNumber) ? indexToInstatiate : 0]);
    }

    private Coin InstantiateGem(int forceNumber = -1)
    {
        indexToInstatiate = forceNumber == -1 ? Random.Range(0, GameManager.I.gemPrefabs.Length) : forceNumber;

        coinNumber = GameManager.I.gemPrefabs[indexToInstatiate].number;

        return Instantiate(GameManager.I.gemPrefabs[forceNumber != -1 || GetDoesArrayContain(ownedGems, coinNumber) ? indexToInstatiate : 0]);
    }

    private Coin InstantiateKey(int forceNumber = -1)
    {
        if (forceNumber != -1)
        {
            keyLevel = forceNumber;
        }
        else
        {
            rng = Random.Range(0,
                GameManager.I.commonSkinAmount + GameManager.I.rareSkinAmount + GameManager.I.epicSkinAmount);
            if (rng < GameManager.I.epicSkinAmount)
            {
                keyLevel = 2;
            }
            else if (rng < GameManager.I.epicSkinAmount + GameManager.I.rareSkinAmount)
            {
                keyLevel = 1;
            }
            else
            {
                keyLevel = 0;
            }
        }

        return Instantiate(GameManager.I.keyPrefabs[keyLevel]);
    }

    private void SpawnStartingCoinStack()
	{
        Vector3 center = new Vector3(treasurePileTopCoin.position.x, 0, treasurePileTopCoin.position.z);
		BoxCollider coinBelow = treasurePile.GetComponent<BoxCollider>();

		for (int i = 0; i < coinManager.startingCoinStackAmount; i++) 
		{
            Coin coin = InstantiateObject(CoinType.Coin);

            // Put the coin on top of the coin below it
			coin.transform.position = center + new Vector3(UnityEngine.Random.Range(-0.015f, 0.015f), 
                                                            coinBelow.transform.position.y + coin.collider.bounds.size.y, 
                                                            UnityEngine.Random.Range(-0.005f, 0.005f));

            coin.SetState(new CoinOnPile(coin, true));

            coin.rb.drag = coinPileManager.coinDrag;

            // Rotate it as if it was flipped
			coin.transform.eulerAngles = new Vector3(0, 0, 180);

            coin.gameObject.layer = 11;
            coin.isStartingStack = true;

            // Disable the perfect hit collider unless it's the top coin
			if (i != coinManager.startingCoinStackAmount - 1)
            {
                coin.perfectHit.gameObject.SetActive(false);
            }

            coinManager.spawnedCoinsCount++;

            coinBelow = coin.collider;
		}
	}    

    private void CombineStaticCoinPile()
    {
        Vector3 originalPos = treasurePile.position;
		treasurePile.position = Vector3.zero;
		treasurePileCoins = treasurePile.GetComponentsInChildren<TreasurePileCoin>();
		combine = new CombineInstance[treasurePileCoins.Length];

		for (int i = 0; i < treasurePileCoins.Length; i++) 
		{
			combine[i].mesh = treasurePileCoins[i].meshFilter.sharedMesh;
			combine[i].transform = treasurePileCoins[i].transform.localToWorldMatrix;
			treasurePileCoins[i].renderer.enabled = false;
			treasurePileCoins[i].gameObject.isStatic = true;
		}
		MeshFilter meshFilter = treasurePile.GetComponent<MeshFilter>();
		meshFilter.mesh = new Mesh();
		meshFilter.mesh.CombineMeshes(combine);
		treasurePile.position = originalPos;
		treasurePile.gameObject.SetActive(true);
    }

    private bool GetDoesArrayContain(int[] array, int value)
    {
        foreach (int element in array)
        {
            if (element == value)
            {
                return true;
            }
        }

        return false;
    }

    public CoinType GetRandomCoinType()
    {
        if (UnityEngine.Random.Range(0, 1f) <= gemChance)
        {
            return CoinType.Gem;
        }
        else
        {
            return CoinType.Coin;
        }
    }

    private int GetKeyCoinRatio()
    {
        foreach (int chest in chests)
        {
            if (chest > 0) return keyCoinRatio;
        }

        return Mathf.RoundToInt(keyCoinRatio * keyRatioMultNoChests);
    }
}
