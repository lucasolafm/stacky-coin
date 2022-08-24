using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinGhost : MiniCoin
{
    // public override CoinType GetCoinType()
    // {
    //     return CoinType.GhostCoin;
    // }

    public override void Land() 
    {
        SetState(new MiniCoinInTube(this));
    }
}
