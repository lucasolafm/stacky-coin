using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinGemBonus : MiniCoinState
{
    private Vector3 position;

    public MiniCoinGemBonus(MiniCoin miniCoin, Vector3 position) : base(miniCoin) 
    {
        this.position = position;
    }

    public override void Enter()
    {
        base.Enter();

        miniCoin.transform.position = position;

        miniCoin.gameObject.SetActive(false);
    }

    public override void Exit()
    {
        base.Exit();

        miniCoin.gameObject.SetActive(true);
    }
}
