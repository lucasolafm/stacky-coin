using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTouchingPile : CoinState
{
    private float startTime;

    public CoinTouchingPile(Coin coin) : base(coin) {}

    public override bool GetIsPartOfPile()
    {
        return true;
    }

    public override void Enter()
    {
        base.Enter();

        startTime = Time.time;

        // Add drag
        coin.rb.drag = coin.coinPileManager.coinDrag;
        coin.collider.material.dynamicFriction = 9;
        coin.collider.material.staticFriction = 9;

        // Disable trail
        coin.StartCoroutine(DisableTrailsAfterDelay());
        
        EventManager.CoinTouchesPile.Invoke(coin);
    }

    public override void Update()
    {
        base.Update();

        // Give the coin a chance to land completely
        if (Time.time - startTime < 0.27f) return;

        // Check if the angle is too steep
        if (coin.transform.eulerAngles.z < -180 - coin.coinPileManager.angleFalling || coin.transform.eulerAngles.z > 180 + coin.coinPileManager.angleFalling)
        {
            // Fall off
            EventManager.CoinFallsWhileTouchingPile.Invoke();

            coin.SetState(new CoinFalling(coin));
        }        
        // Check if it is almost still
        else if (coin.rb.velocity.x <= coin.coinManager.minMoveToScore)
        {
            // Score
            EventManager.CoinScores.Invoke(coin);

            coin.SetState(new CoinOnPile(coin));
        }
    }

    private IEnumerator DisableTrailsAfterDelay()
    {
        yield return new WaitForSeconds(2);

        coin.ToggleTrail();
    }
}
