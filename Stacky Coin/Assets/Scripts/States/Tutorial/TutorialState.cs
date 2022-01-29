using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TutorialState
{
    protected TutorialManager TutorialManager;
    protected int flipCount = 0;

    public TutorialState(TutorialManager tutorialManager)
    {
        TutorialManager = tutorialManager;
    }

    public virtual void Enter() 
    {
        TutorialManager.panel.gameObject.SetActive(false);
        DisplayTextBox();

        if (TutorialManager.currentLevelNr < TutorialManager.tutorialLevels.Length) 
        {
            TutorialManager.InsertNextNewObjects(TutorialManager.tutorialLevels[TutorialManager.currentLevelNr].coins);
        }
    }

    public virtual void Exit() 
    {
        TutorialManager.AdvanceLevel();
    }

    public virtual void OnFlip(float HandChargesTime) 
    {
        if (TutorialManager.currentLevelNr >= TutorialManager.tutorialLevels.Length) return;

        flipCount++;
        if (flipCount >= TutorialManager.tutorialLevels[TutorialManager.currentLevelNr].coins.Length)
        {
            OnSufficientFlips();
        }
    }

    public virtual void OnSufficientFlips() {}

    public virtual void DisplayTextBox() 
    {
        TutorialManager.panel.gameObject.SetActive(true);

        TutorialManager.tutorialText.text = TutorialManager.tutorialLevels[TutorialManager.currentLevelNr].text;

        TutorialManager.AnimateTextBox();
    }
}
