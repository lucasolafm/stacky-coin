using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawned : CoinState
{
    public CoinSpawned(Coin coin) : base(coin) {}

    public override void Enter()
    {
        base.Enter();

        coin.transform.position = coin.coinManager.handManager.hand.position + new Vector3(0, 3.183f, 0);
        
        coin.gameObject.SetActive(true);
    }

    public override void OnCollideWithHand()
    {
        base.OnCollideWithHand();

        coin.SetState(new CoinOnHand(coin));
    }
}
