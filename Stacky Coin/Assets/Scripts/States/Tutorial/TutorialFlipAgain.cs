using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFlipAgain : TutorialState
{
    public TutorialFlipAgain(TutorialManager t) : base (t) {}

    public override void OnSufficientFlips()
    {
        TutorialManager.SetState(new TutorialFlipHigher(TutorialManager));
    }
}
