using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skin : MonoBehaviour 
{
    public new Transform transform;
    public Transform visuals;
    [HideInInspector] public Transform shadedVisual;
    [HideInInspector] public int number;
    [HideInInspector] public bool unlocked;
}
