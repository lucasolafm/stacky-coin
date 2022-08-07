using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPause : TutorialState
{
    public TutorialPause(TutorialManager TutorialManager) : base (TutorialManager) {}

    public override void OnSufficientFlips()
    {
        TutorialManager.SetState(new TutorialPause2(TutorialManager));
    }
}
