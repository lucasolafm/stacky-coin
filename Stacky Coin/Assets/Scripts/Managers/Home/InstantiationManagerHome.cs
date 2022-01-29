using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InstantiationManagerHome : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    [SerializeField] private MiniCoinManager miniCoinManager;

    [SerializeField] private Transform miniCoinHolder;

    public int miniGemInstantiateAmount;
    private MiniCoin miniCoin;

    public MiniCoin InstantiateMiniObject(int identifier)
    {
        if (identifier == 0)
        {
            miniCoin = Instantiate(GameManager.I.miniCoinPrefab, miniCoinHolder);
        }
        else if (identifier >= GameManager.I.coinSkinAmount)
        {            
            miniCoin = Instantiate(GameManager.I.miniGemsPrefabs[identifier - GameManager.I.coinSkinAmount], miniCoinHolder);
        }
        else if (identifier > 0)
        {
            miniCoin = Instantiate(GameManager.I.miniKeysPrefabs[identifier - 1], miniCoinHolder);
        }
        else
        {
            miniCoin = Instantiate(GameManager.I.miniCoinGhostPrefab, miniCoinHolder);
        }

        miniCoin.homeManager = homeManager;
        miniCoin.miniCoinManager = miniCoinManager;   
        
        return miniCoin;
    }
}
