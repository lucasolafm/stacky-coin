using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFlip : TutorialState
{
    public TutorialFlip(TutorialManager t) : base (t) {}

    public override void OnSufficientFlips()
    {
        TutorialManager.SetState(new TutorialFlipAgain(TutorialManager));
    }
}
