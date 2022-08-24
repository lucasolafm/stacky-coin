using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinCoin : MiniCoin
{
    // public override CoinType GetCoinType()
    // {
    //     return CoinType.Coin;
    // }

    public override void Land()
    {
        EventManager.MiniCoinAddedToTube.Invoke(GetCoinType());

        SetState(new MiniCoinInTube(this));
    }
}
