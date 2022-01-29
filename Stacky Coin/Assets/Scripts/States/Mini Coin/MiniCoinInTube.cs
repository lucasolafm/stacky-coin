using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinInTube : MiniCoinState
{
    public MiniCoinInTube(MiniCoin miniCoin) : base(miniCoin) {}

    public override bool GetHasLanded()
    {
        return true;
    }

    public override bool GetIsFinishedDropping()
    {
        return true;
    }
}
