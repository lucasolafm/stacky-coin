using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayGameOver : PlayState
{
    private bool manualGameOver;

    public PlayGameOver(PlayManager manager, bool manualGameOver = false) : base(manager) 
    {
        this.manualGameOver = manualGameOver;
    }

    public override void Enter()
    {
        base.Enter();

        GameManager.I.isGameOver = true;
        
        EventManager.CoinLandsOnFloor.AddListener(OnCoinLandsOnFloor);

        EventManager.GoingGameOver.Invoke();

        EventManager.GoneGameOver.Invoke(manualGameOver);
    }

    private void OnCoinLandsOnFloor(Coin coin)
    {
        if (coin.isStartingStack) return;
        
        EventManager.CoinLandsOnFloor.RemoveListener(OnCoinLandsOnFloor);

        if (!GameManager.I.isGameOver || manualGameOver) return;
        
        if (GameManager.I.scoredCoins.Count <= manager.pileCollapseSizes[0])
        {
            GameManager.I.audioSource.PlayOneShot(manager.pileCollapseClips[0]);
        }
        else if (GameManager.I.scoredCoins.Count <= manager.pileCollapseSizes[1])
        {
            GameManager.I.audioSource.PlayOneShot(manager.pileCollapseClips[1]);
        }
        else
        {
            GameManager.I.audioSource.PlayOneShot(manager.pileCollapseClips[2]);
        }
    }

    public override void OnApplicationQuit()
    {
        // Save the scored mini coins with gem bonus coins and without keys if the player quits the game before going to home
        GameManager.I.RemoveMiniKeysAndGhostCoins(GameManager.I.scoredCoins);
        //GameManager.I.AddGemBonusMiniCoins();

        Data.AddMiniCoins(GameManager.I.scoredCoins);
    }

    // Ignore events
    public override void OnCoinScores(Coin coin) {}
    public override void OnCoinFallsOffPile(Coin coin) {}
    public override void OnCoinPileFallsOver() {}
    public override void PressReloadButton() {}
}
