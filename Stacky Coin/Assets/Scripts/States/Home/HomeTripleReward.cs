using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeTripleReward : HomeState
{
    public HomeTripleReward(HomeManager manager) : base(manager) {}

    public override void OnHomeSceneLoaded()
    {
        EventManager.LoadedHomeScene.Invoke();
    }
}
