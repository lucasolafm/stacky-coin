using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinTubeDescending : CoinTubeState
{
    private float t;
    private Vector3 originalPos;
    private float move;

    public CoinTubeDescending(CoinTubeManager manager) : base(manager) {}

    public override void Enter()
    {
        base.Enter();

        originalPos = manager.cameraTransform.position;

        move = Mathf.Min((manager.topOfScreen.y - manager.bottomRightOfScreen.y) * manager.cameraMoveScreenPercent, originalPos.y - manager.cameraPositionMin);
    }

    public override void Update()
    {
        t = Mathf.Min(t + Time.deltaTime / manager.cameraDescendTime, 1);

        manager.cameraTransform.position = originalPos - new Vector3(0, (t < 0.5 ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2) * move, 0);

        if (t < 1) return;
            
        EventManager.CoinTubeCameraRepositioned.Invoke();

        manager.SetState(new CoinTubeDefault(manager));
    }
}
