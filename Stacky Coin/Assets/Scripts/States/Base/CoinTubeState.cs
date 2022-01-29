using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTubeState : State
{
    protected CoinTubeManager manager;

    public CoinTubeState(CoinTubeManager manager)
    {
        this.manager = manager;
    }

    public virtual void MiniCoinsReachedTopOfScreen() {}
}
