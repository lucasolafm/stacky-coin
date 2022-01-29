using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinFlips : CoinState
{
    private float chargeTime;
    private bool flipNow;

    public CoinFlips(Coin coin, float chargeTime) : base(coin) 
    {
        this.chargeTime = chargeTime; 
    }

    public override void Enter()
    {
        base.Enter();

        // Set maximum charge time based on coin weight
        chargeTime = Mathf.Min(chargeTime, coin.type == CoinType.Coin ? 4.224f : 4.192f);

        coin.ToggleTrail();

        flipNow = true;
    }

    public override void LateUpdate()
    {
        base.LateUpdate();

        if (!flipNow) return;

        EventManager.CoinFlipping.Invoke(coin, chargeTime);

        Flip();

        EventManager.CoinFlips.Invoke(coin, chargeTime);

        flipNow = false;
    }

    public override void OnCollideWithCoin()
    {
        base.OnCollideWithCoin();

        coin.SetState(new CoinTouchingPile(coin));
    }

    public override void OnCollideWithFallOffZone()
    {
        base.OnCollideWithFallOffZone();

        coin.SetState(new CoinFalling(coin));
    }   

    private void Flip()
    {
        // Torque
		coin.rb.maxAngularVelocity += chargeTime * (coin.type == CoinType.Coin ? 0.4f : 0.3028f);
        coin.rb.AddTorque(-coin.transform.forward * coin.coinManager.rotationSpeed);

        // Force
        coin.rb.AddForce(coin.coinManager.xForce, coin.coinManager.yForce * (1 + chargeTime / coin.coinManager.chargeSpeed), 0);
    }
}
