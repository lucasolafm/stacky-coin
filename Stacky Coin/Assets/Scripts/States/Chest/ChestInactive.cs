using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestInactive : ChestState
{
    public ChestInactive(Chest chest) : base(chest) {}   

    public override bool GetIsActive()
    {
        return false;
    }

    public override void Enter()
    {
        base.Enter();

        chest.gameObject.SetActive(false);
    }
}
