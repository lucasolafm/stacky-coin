using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeDefault : HomeState
{
    public HomeDefault(HomeManager manager) : base(manager) {}

    public override bool CanBuyChest()
    {
        return true;
    }

    public override bool CanDropMiniCoins()
    {
        return true;
    }

    public override void OnHomeSceneLoaded()
    {
        EventManager.LoadedHomeScene.Invoke();
        EventManager.EnterDefaultHome.Invoke();
    }

    public override void PressPlayAgainButton()
    {
        base.PressPlayAgainButton();

        manager.SetState(new HomeEnteringPlay(manager));
    }

    public override void DropMiniCoins(List<int> identifiers)
    {
        manager.SetState(new HomeDroppingMiniCoins(manager, identifiers, true));
    }

    public override void OnBuysChest(int price, bool chestIsPaidByAd)
    {
        if (chestIsPaidByAd) return;

        manager.SetState(new HomePreparingPayingChest(manager));
    }

    public override void PressCollectionButton()
    {
        base.PressCollectionButton();

        manager.SetState(new HomeEnteringCollection(manager));
    }

    public override void SwipeScreen(bool rightOrLeft)
    {
        if (rightOrLeft == false) return;

        manager.SetState(new HomeEnteringCollection(manager));
    }
}
