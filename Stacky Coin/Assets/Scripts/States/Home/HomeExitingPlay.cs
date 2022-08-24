using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeExitingPlay : HomeState
{
    public HomeExitingPlay(HomeManager manager) : base(manager) {}

    public override void OnApplicationQuit()
    {
        // Save the scored mini coins with gem bonus coins and without keys if the player quits the game before going to the coins can drop
        GameManager.I.RemoveMiniKeysAndGhostCoins(GameManager.I.scoredCoins);
        //GameManager.I.AddGemBonusMiniCoins();

        Data.AddMiniCoins(GameManager.I.scoredCoins);
    }
}
