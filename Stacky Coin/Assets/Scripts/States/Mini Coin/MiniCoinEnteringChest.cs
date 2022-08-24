using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoinEnteringChest : MiniCoinState
{
    private Vector3 inTubePosition, chestPayingPosition;
    private float startTime, enterTime;
    private Vector3 startScale, endScale;
    private float t;

    public MiniCoinEnteringChest(MiniCoin miniCoin, Vector3 inTubePosition, Vector3 chestPayingPosition, float startTime, float enterTime, Vector3 startScale, Vector3 endScale) : base(miniCoin) 
    {
        this.inTubePosition = inTubePosition;
        this.chestPayingPosition = chestPayingPosition;
        this.startTime = startTime;
        this.enterTime = enterTime;
        this.startScale = startScale;
        this.endScale = endScale;
    }

    public override void Enter()
    {
        base.Enter();

        inTubePosition = new Vector3(inTubePosition.x, inTubePosition.y, 0);

        t = startTime / enterTime + Random.Range(-0.005f, 0.005f);
    }

    public override void Update()
    {
        t = Mathf.Min(t + Time.deltaTime / enterTime, 1);

        miniCoin.transform.position = Vector3.Lerp(inTubePosition, chestPayingPosition, -(Mathf.Cos(Mathf.PI * t) - 1) / 2);

        miniCoin.transform.localScale = Vector3.Lerp(startScale, endScale, t * t);

        if (t == 1)
        {
            miniCoin.unlockSkinManager.OnMiniCoinEntersChest(miniCoin.GetCoinType());

            miniCoin.SetState(new MiniCoinInactive(miniCoin));
        }
    }
}
