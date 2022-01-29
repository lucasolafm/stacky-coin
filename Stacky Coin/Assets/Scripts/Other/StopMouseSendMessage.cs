using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StopMouseSendMessage : MonoBehaviour
{
    public Camera _camera;

    void Start()
    {
        _camera.eventMask = ~_camera.cullingMask;
    }
}
