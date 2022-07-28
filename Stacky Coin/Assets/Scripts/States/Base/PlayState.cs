using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayState : State
{
    public PlayManager manager;

    public PlayState(PlayManager manager)
    {
        this.manager = manager;
    }

    public virtual void PressDownBlank() {}

    public virtual void PressUp() {}

    public virtual void PressReloadButton() {}

    public virtual void PressReloadButtonYes() {}

    public virtual void PressReloadButtonNo() {}

    public virtual void OnCoinScores(Coin coin)
    {
        // Increase score
        //manager.ChangeScore(1);

        // Save current pile height for highscore line
		manager.currentHighestPoint = coin.transform.position.y;
    }

    public virtual void OnCoinFalls(Coin coin) {}

    public virtual void OnCoinFallsOffPile(Coin coin) 
    { 
        if (!coin.State.GetIsScored()) return;
        
        //manager.ChangeScore(-1); 
    }

    public virtual void OnHandAscendedCoinPile() {}

    public virtual void OnCoinPileFallsOver() 
    { 
        manager.SetState(new PlayGameOver(manager)); 
    }

    public virtual void OnApplicationQuit() {}
}
