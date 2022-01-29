using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayDefault : PlayState
{
    public PlayDefault(PlayManager manager) : base(manager) {}

    public override void PressDownBlank()
    {
        base.PressDownBlank();

        manager.SetState(new PlayCharging(manager));
    }

    public override void PressReloadButton()
    {
        manager.SetState(new PlayRequestingReload(manager));
    }
}
