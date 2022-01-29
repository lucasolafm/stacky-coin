using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinOnHand : CoinState
{
    public CoinOnHand(Coin coin) : base(coin) {}

    public override void Enter()
    {
        base.Enter();

        coin.rb.isKinematic = true;

        // Ignore collision between coin and hand
        Physics.IgnoreCollision(coin.collider, coin.coinManager.handManager.handCollider);

        // Place coin on hand
        coin.transform.parent = coin.coinManager.handManager.handVisuals;
        coin.transform.position = new Vector3(coin.transform.position.x, coin.coinManager.handManager.hand.position.y + 0.972397f, coin.transform.position.z);

        EventManager.CoinLandsOnHand.Invoke(coin);
    }

    public override void Exit()
    {
        base.Exit();

        coin.rb.isKinematic = false;

        coin.transform.parent = coin.coinManager.coinHolder;
    }

    public override void Flip(float chargeTime)
    {
        base.Flip(chargeTime);

        coin.SetState(new CoinFlips(coin, chargeTime));
    }
}
