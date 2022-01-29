using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionDefault : CollectionState
{
    public CollectionDefault(CollectionManager manager) : base(manager) {}

    public override void SwipeScreen(bool rightOrLeft)
    {
        if (rightOrLeft == true) return;

        manager.SetState(new CollectionEnteringHome(manager));
    }
}
