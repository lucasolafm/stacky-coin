using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialGem : TutorialState
{
    public bool learnedHeavyObjects;

    public TutorialGem(TutorialManager t) : base(t) {}

    public override void OnFlip(float HandChargesTime)
    {
        if (HandChargesTime > 0.5f)
        {
            learnedHeavyObjects = true;
        }

        base.OnFlip(HandChargesTime);
    }

    public override void OnSufficientFlips()
    {
        if (learnedHeavyObjects)
        {
            TutorialManager.SetState(new TutorialPause(TutorialManager));
        }
        else
        {
            Coin gem = TutorialManager.instantiationManager.InstantiateObject(CoinType.Gem, TutorialManager.coinManager.spawnedCoinsCount);

            TutorialManager.InsertNextNewObjects(new Coin[]{gem});
        }
    }
}
