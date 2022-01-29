using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayRequestingReload : PlayState
{
    public PlayRequestingReload(PlayManager manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        manager.reloadPanel.SetActive(true);
    }

    public override void Exit()
    {
        base.Exit();

        manager.reloadPanel.SetActive(false);
    }

    public override void PressDownBlank()
    {
        manager.SetState(new PlayDefault(manager));
    }

    public override void PressReloadButton()
    {
        manager.SetState(new PlayDefault(manager));
    }

    public override void PressReloadButtonYes()
    {
        manager.SetState(new PlayGameOver(manager, true));
    }

    public override void PressReloadButtonNo()
    {
        manager.SetState(new PlayDefault(manager));
    }
}
