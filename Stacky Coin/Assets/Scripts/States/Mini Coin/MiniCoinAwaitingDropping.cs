using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinAwaitingDropping : MiniCoinState
{
    private Vector3 begin;
    private float end;

    public MiniCoinAwaitingDropping(MiniCoin miniCoin, Vector3 begin, float end) : base (miniCoin) 
    {
        this.begin = begin;
        this.end = end;
    }

    public override void OnLoadingScreenSlidOut()
    {
        miniCoin.SetState(new MiniCoinDropping(miniCoin, begin, end));
    }
}
