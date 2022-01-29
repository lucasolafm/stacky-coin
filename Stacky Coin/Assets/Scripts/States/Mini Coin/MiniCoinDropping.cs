using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinDropping : MiniCoinState
{
    private Vector3 begin; 
    private float end;
    private float speed;
    private float move;
    private bool hasBeenSeen;

    public MiniCoinDropping(MiniCoin miniCoin, Vector3 begin, float end) : base(miniCoin) 
    {
        this.begin = begin;
        this.end = end;
    }

    public override void Enter()
    {
        base.Enter();

        speed = miniCoin.miniCoinManager.startDropSpeed;

        miniCoin.transform.position = begin;
    }

    public override void Update()
    {        
        move = speed * Time.deltaTime;

        // If the next move will put the coin over or near the end point, put it on the end point
        if (miniCoin.transform.position.y - move < end + move * 0.5f)
        {
            miniCoin.transform.position = new Vector3(miniCoin.transform.position.x, end, miniCoin.transform.position.z);

            miniCoin.Land();

            return;
        }

        miniCoin.transform.Translate(Vector3.down * move, Space.World);

        if (hasBeenSeen)
        {
            speed *= miniCoin.miniCoinManager.dropSpeedMultiplier;
        }
    }

    public override void OnBecameVisible()
    {
        hasBeenSeen = true;
    }
}
