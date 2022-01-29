using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionEnteringHome : CollectionState
{
    public CollectionEnteringHome(CollectionManager manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        EventManager.EntersHome.Invoke();

        manager.StartCoroutine(Timer());
    }

    private IEnumerator Timer()
    {
        EventManager.EnteringHome.Invoke();

        yield return new WaitForSeconds(GameManager.I.collectionHomeTransitionTime);

        yield return new WaitForEndOfFrame();

        manager.SetState(new CollectionDisabled(manager));

        EventManager.EnteredHome.Invoke();
    }
}
