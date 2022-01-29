using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinOnPile : CoinState
{
    private bool isStartingStack;
    private float distanceMoved;
    private Vector3 lastPosition;

    public CoinOnPile(Coin coin, bool isStartingStack = false) : base(coin) 
    {
        this.isStartingStack = isStartingStack;
    }

    public override bool GetIsPartOfPile()
    {
        return true;
    }

    public override bool GetIsScored()
    {
        return !isStartingStack;
    }

    public override void GetStabilityOnPile(out bool unstable, out bool falling)
    {
        // Check if the coin is fully falling by checking the distance it moved and how steep the angle is
        falling = coin.rb.velocity.x >= coin.coinPileManager.lenghtFalling || 
                    (coin.transform.eulerAngles.z < 180 - coin.coinPileManager.angleFalling || coin.transform.eulerAngles.z > 180 + coin.coinPileManager.angleFalling);

        // Check if it's almost falling
        unstable = coin.rb.velocity.x >= coin.coinPileManager.lenghtAboutToFall;
    }

    public override void Enter()
    {
        base.Enter();

        lastPosition = coin.transform.position;
    }    
}
