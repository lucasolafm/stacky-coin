using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectionState : State
{
    protected CollectionManager manager;

    public CollectionState(CollectionManager manager)
    {
        this.manager = manager;
    }

    public virtual void SwipeScreen(bool rightOrLeft) {}
}
