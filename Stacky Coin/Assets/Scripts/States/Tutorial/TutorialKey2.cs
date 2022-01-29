using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialKey2 : TutorialState
{
    public TutorialKey2(TutorialManager t) : base(t) {} 

    public override void Enter()
    {
        base.Enter();

        TutorialManager.panel.localScale = Vector3.zero;
    }

    public override void OnSufficientFlips()
    {
        TutorialManager.SetState(new TutorialEnd(TutorialManager));
    }
}