﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPause2 : TutorialState
{
    public TutorialPause2(TutorialManager TutorialManager) : base (TutorialManager) {}

    public override void DisplayTextBox() {}

    public override void Enter()
    {
        base.Enter();

        TutorialManager.panel.localScale = Vector3.zero;
    }

    public override void OnSufficientFlips()
    {
        TutorialManager.SetState(new TutorialKey(TutorialManager));
    }
}