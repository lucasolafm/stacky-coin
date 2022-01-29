using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialEnd : TutorialState
{
    public TutorialEnd(TutorialManager t) : base(t) {}

    public override void DisplayTextBox()
    {
        TutorialManager.StartCoroutine(DisplayTextboxAfterDelay());
        
        TutorialManager.StartCoroutine(EndTutorialAfterDelay());
    }

    private IEnumerator DisplayTextboxAfterDelay()
    {
        yield return new WaitForSeconds(1.5f);

        base.DisplayTextBox();
        base.Exit();
    }

    private IEnumerator EndTutorialAfterDelay()
    {
        yield return new WaitForSeconds(5.5f);

        TutorialManager.EndTutorial();
    }
}
