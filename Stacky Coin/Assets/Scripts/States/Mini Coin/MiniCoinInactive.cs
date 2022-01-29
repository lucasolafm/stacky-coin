using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinInactive : MiniCoinState
{
    public MiniCoinInactive(MiniCoin miniCoin) : base(miniCoin) {}

    public override bool GetIsActive()
    {
        return false;
    }

    public override void Enter()
    {
        base.Enter();

        miniCoin.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        base.Exit();

        miniCoin.gameObject.SetActive(true);
    }
}
