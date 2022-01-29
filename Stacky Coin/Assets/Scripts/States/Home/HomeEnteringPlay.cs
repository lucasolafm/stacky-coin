using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeEnteringPlay : HomeState
{
    public HomeEnteringPlay(HomeManager manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        EventManager.PlayingAgain.Invoke();
    }
}
