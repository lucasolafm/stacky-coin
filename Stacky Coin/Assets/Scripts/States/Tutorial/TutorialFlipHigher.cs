using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialFlipHigher : TutorialState
{
    private int flippedCorrectlyCount;

    public TutorialFlipHigher(TutorialManager t) : base (t) {}

    public override void Enter()
    {
        base.Enter();
        
        TutorialManager.tutorialText.text = TutorialManager.tutorialLevels[TutorialManager.currentLevelNr].text + " (0/3)";
    }

    public override void OnFlip(float HandChargesTime)
    {
        if (HandChargesTime > 0.25f)
        {
            flippedCorrectlyCount++;
        }

        if (flippedCorrectlyCount > 0)
        {
            TutorialManager.tutorialText.text = TutorialManager.tutorialLevels[TutorialManager.currentLevelNr].text + " (" + flippedCorrectlyCount + "/3)";
        }

        if (flippedCorrectlyCount < 3)
        {
            Coin coin = TutorialManager.instantiationManager.InstantiateObject(CoinType.Coin, 5);

            TutorialManager.InsertNextNewObjects(new Coin[]{coin});
        }

        base.OnFlip(HandChargesTime);
    }

    public override void OnSufficientFlips()
    {
        if (flippedCorrectlyCount == 3)
        {
            TutorialManager.StartCoroutine(NextTutorialLevel());
        }
    }

    private IEnumerator NextTutorialLevel()
    {
        yield return new WaitForSeconds(1);
        
        TutorialManager.SetState(new TutorialHeavyObj(TutorialManager));
    }
}
