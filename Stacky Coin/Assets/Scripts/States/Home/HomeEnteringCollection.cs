using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeEnteringCollection : HomeState
{
    public HomeEnteringCollection(HomeManager manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        EventManager.EntersCollection.Invoke();

        manager.StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        // Wait for the lag of enabled the collection to pass
        //yield return null;
        //yield return null;

        EventManager.EnteringCollection.Invoke();

        yield return new WaitForSeconds(GameManager.I.collectionHomeTransitionTime);

        yield return new WaitForEndOfFrame();

        manager.SetState(new HomeDisabled(manager));

        EventManager.EnteredCollection.Invoke();
    }
}
