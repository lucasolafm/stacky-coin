using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayInitializingNextStage : PlayState
{
    public PlayInitializingNextStage(PlayManager manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        manager.nextStageTarget += 10;
    }

    public override void Exit()
    {
        base.Exit();

        EventManager.StageInitialized.Invoke();
    }

    public override void OnHandAscendedCoinPile()
    {
        base.OnHandAscendedCoinPile();

        manager.SetState(new PlayDefault(manager));
    }
}
