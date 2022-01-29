using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTubeDefault : CoinTubeState
{
    public CoinTubeDefault(CoinTubeManager manager) : base(manager) {}

    public override void MiniCoinsReachedTopOfScreen()
    {
        base.MiniCoinsReachedTopOfScreen();

        manager.SetState(new CoinTubeAscending(manager));
    }
}
