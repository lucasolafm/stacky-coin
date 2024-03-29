﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinState : State
{
    public Coin coin;

    public CoinState(Coin coin)
    {
        this.coin = coin;
    }

    public virtual bool GetIsPartOfPile() { return false; }

    public virtual bool GetIsScored() { return false; }

    public virtual bool GetIsFallen() { return false; }

    public virtual void GetStabilityOnPile(out bool unstable, out bool falling) { unstable = false; falling = false; }

    public virtual bool GetIsStillOnPile() { return true; }

    public virtual void LateUpdate() {}
    
    public virtual void OnCollideWithCoin(Collision collision) {}

    public virtual void OnCollideWithFloor() {}

    public virtual void OnCollideWithFallOffZone() {}

    public virtual void Flip(float chargeTime) {}
}
