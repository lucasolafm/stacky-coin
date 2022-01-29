using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinState : State
{
    protected MiniCoin miniCoin;

    public MiniCoinState(MiniCoin miniCoin)
    {
        this.miniCoin = miniCoin;
    }

    public virtual bool GetIsActive() { return true; }

    public virtual bool GetHasLanded() { return false; }

    public virtual bool GetIsFinishedDropping() { return false; }

    public virtual void OnLoadingScreenSlidOut() {}

    public virtual void OnBecameVisible() {}
}
