using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeDisabled : HomeState
{
    public HomeDisabled(HomeManager manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        manager.homeHolder.SetActive(false);
    }
}
