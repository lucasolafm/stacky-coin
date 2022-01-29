using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTubeAscending : CoinTubeState
{
    private float t;
    private Vector3 originalPos;

    public CoinTubeAscending(CoinTubeManager manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        originalPos = manager.cameraTransform.position;
    }

    public override void Update()
    {
        t = Mathf.Min(t + Time.deltaTime / manager.cameraAscendTime, 1);

        manager.cameraTransform.position = originalPos + new Vector3(0, Utilities.EaseInOutQuad(t) * 
                                                                        ((manager.topOfScreen.y - manager.bottomRightOfScreen.y) * 
                                                                        manager.cameraMoveScreenPercent), 0);

        if (t < 1) return;
            
        EventManager.CoinTubeCameraRepositioned.Invoke();

        manager.SetState(new CoinTubeDefault(manager));
    }
}
