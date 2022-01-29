using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestState : State
{
    protected Chest chest;

    public ChestState(Chest chest)
    {
        this.chest = chest;
    }

    public virtual bool GetIsActive() { return false; }

    public virtual void PressChest() {}

    public virtual void StartOpeningProcess() {}

    public virtual void OnMiniCoinAddedToTube() {}

    public virtual void OnMiniCoinRemovedFromTube() {}
}
