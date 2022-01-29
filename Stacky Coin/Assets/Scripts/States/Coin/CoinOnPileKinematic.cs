using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinOnPileKinematic : CoinState
{
    private bool canBeStraightened;
    private bool isStartingStack;

    public CoinOnPileKinematic(Coin coin, bool canBeStraightened, bool isStartingStack = false) : base(coin) 
    {
        this.canBeStraightened = canBeStraightened;
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

    public override void Enter()
    {
        base.Enter();

        coin.rb.isKinematic = true;
        coin.gameObject.isStatic = true;

        if (!canBeStraightened) return;
        
        coin.transform.eulerAngles = new Vector3(0, 0, -180);
    }

    public override void Exit()
    {
        base.Exit();

        coin.rb.isKinematic = false;
        coin.gameObject.isStatic = false;
    }
}
