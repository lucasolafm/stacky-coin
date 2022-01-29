using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeState : State
{
    protected HomeManager manager;

    public HomeState(HomeManager manager)
    {
        this.manager = manager;
    }

    public virtual bool CanBuyChest() { return false; }

    public virtual bool CanDropMiniCoins() { return false; }

    public virtual void OnHomeSceneLoaded() {}

    public virtual void OnApplicationQuit() {}

    public virtual void PressPlayAgainButton() {}

    public virtual void PressCollectionButton() {}

    public virtual void SwipeScreen(bool rightOrLeft) {}

    public virtual void PressWatchAdButton() {}

    public virtual void OnBuysChest(int price, bool chestIsPaidByAd) {}

    public virtual void DropMiniCoins(List<int> identifiers) {}
}
