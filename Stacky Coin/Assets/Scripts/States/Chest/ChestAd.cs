using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestAd : ChestOpen
{
    public ChestAd(Chest chest, bool fromLocked = false) : base(chest, fromLocked) {}

    public override void Enter()
    {
        base.Enter();

        pointer = chest.pointerAd;
    }

    public override void SetPointer()
    {
        chest.SetPointerAd();
    }

    public override void PressChest()
    {
        if (!chest.homeManager.State.CanBuyChest()) return;

        EventManager.BuysChestWithAd.Invoke(chest);
    }

    public override void StartOpeningProcess()
    {
        EventManager.BuysChest.Invoke(chest, true);

        Data.SetChest(0, chest.position);

        chest.SetState(new ChestInactive(chest));  
    }

    public override void OnMiniCoinAddedToTube(CoinType type) {}
    public override void OnMiniCoinRemovedFromTube(CoinType type) {}
}
