using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Camera))]
public class CameraSizePerScreenSize : MonoBehaviour
{
    public float sceneWidth;
    public MoveCamUpPerScreenHeight moveScript;
    Camera _camera;

    void Awake()
    {
        _camera = GetComponent<Camera>();

        float unitsPerPixel = sceneWidth / Screen.width;

        float desiredHalfHeight = 0.5f * unitsPerPixel * Screen.height;

        _camera.orthographicSize = desiredHalfHeight;

        if (moveScript) moveScript.MoveUp();
    }
}

