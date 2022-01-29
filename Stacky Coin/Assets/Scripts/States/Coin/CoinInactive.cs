using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinInactive : CoinState
{
    public CoinInactive(Coin coin) : base(coin) {}

    public override void Enter()
    {
        base.Enter();

        coin.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        base.Exit();

        coin.gameObject.SetActive(true);
    }
}
