using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayLastCoinToNextStage : PlayState
{
    private Coin flippedCoin;
    private float startTime;

    public PlayLastCoinToNextStage(PlayManager manager, Coin flippedCoin) : base(manager) 
    {
        this.flippedCoin = flippedCoin;
    }

    public override void Enter()
    {
        base.Enter();

        startTime = Time.time;
    }

    public override void OnCoinScores(Coin coin)
    {
        base.OnCoinScores(coin);

        if (coin != flippedCoin) return;

        // Make sure the target will still be reached (in case of coins falling off in the meantime)
        if (manager.score >= manager.nextStageTarget)
        {
            // Get the height the hand should be at in the next stage
            float handHeightNextStage = manager.nextStageTarget < 20 ? manager.handManager.firstHandMovePosition : 
                                            coin.transform.position.y - manager.handManager.handBelowTopCoinDistance;// -0.912f + 0.4f * ((manager.nextStageTarget / 10) - 1);

            EventManager.ReachesNextStageTarget.Invoke(coin, handHeightNextStage);

            manager.SetState(new PlayInitializingNextStage(manager));
        }
        else
        {
            ContinueNormally();
        }
    }

    public override void OnCoinFalls(Coin coin)
    {
        base.OnCoinFalls(coin);

        if (coin != flippedCoin) return;

        ContinueNormally();
    }

    private void ContinueNormally()
    {
        // Spawn a new coin
        manager.StartCoroutine(manager.coinManager.SpawnCoin(Mathf.Max(manager.coinManager.spawnTime - (Time.time - startTime), 0)));

        manager.SetState(new PlayDefault(manager));
    }
}
