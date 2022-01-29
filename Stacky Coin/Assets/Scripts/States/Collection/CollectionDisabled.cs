using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionDisabled : CollectionState
{
    public CollectionDisabled(CollectionManager manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        manager.collectionHolder.SetActive(false);
        manager.collectionLights.SetActive(false);
    }
}
