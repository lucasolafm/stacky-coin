using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamUpPerScreenHeight : MonoBehaviour
{
    public Vector3 targetPosBottomScreen;
    public Vector3 camVerticleOffset;
    public bool inCollection;
    
    public void MoveUp()
    {
        //return;
        Camera _camera = GetComponent<Camera>();

        camVerticleOffset = targetPosBottomScreen - _camera.ScreenToWorldPoint(Vector3.zero);

        if (!inCollection)
        {
            _camera.transform.position += camVerticleOffset;
        }
        else
        {
            _camera.transform.position -= camVerticleOffset;
        }
    }
}
