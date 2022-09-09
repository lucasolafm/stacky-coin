using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinFalling : CoinState
{
    private bool landedOnFloor;
    
    public CoinFalling(Coin coin) : base(coin) {}
    
    public override void Enter()
    {
        base.Enter();

        coin.collider.material.dynamicFriction = 0.7f;
        coin.collider.material.staticFriction = 0.7f;
        coin.rb.drag = 0;

        // Stop colliding with the invisible walls
        foreach (Collider collider in coin.coinPileManager.invisibleWalls)
        {
            Physics.IgnoreCollision(coin.collider, collider);
        }

        EventManager.CoinFalls.Invoke(coin);
    }

    public override bool GetIsFallen() => true;

    public override void OnCollideWithCoin(Collision collision)
    {
        base.OnCollideWithCoin(collision);

        EventManager.CoinCollides.Invoke(coin, collision.relativeVelocity.sqrMagnitude);
    }

    public override void OnCollideWithFloor()
    {
        if (landedOnFloor) return;
        landedOnFloor = true;

        EventManager.CoinLandsOnFloor.Invoke(coin);
    }
}
