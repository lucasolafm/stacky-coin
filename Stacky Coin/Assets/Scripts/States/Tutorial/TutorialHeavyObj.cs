using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialHeavyObj : TutorialState
{
    public TutorialHeavyObj(TutorialManager t) : base (t) {}

    public override void OnSufficientFlips()
    {
        TutorialManager.SetState(new TutorialGem(TutorialManager));
    }

    public override void DisplayTextBox()
    {
        return;
    }
}
