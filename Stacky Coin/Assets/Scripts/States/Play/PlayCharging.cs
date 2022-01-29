using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayCharging : PlayState
{
    private float startTime;

    public PlayCharging(PlayManager manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        startTime = Time.time;

        EventManager.HandCharges.Invoke(manager.coinManager.Coins[manager.coinManager.newCoinIndex]);
    }

    public override void Exit()
    {
        base.Exit();
        
        EventManager.HandStopsCharge.Invoke();
    }

    public override void PressUp()
    {
        base.PressUp();

        // Flip coin
        manager.coinManager.Coins[manager.coinManager.newCoinIndex].State.Flip(Time.time - startTime);

        manager.SetState(new PlayDefault(manager));
    }
}
