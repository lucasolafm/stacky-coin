using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerfectHit : MonoBehaviour 
{
    public bool didPerfectHit;

    void OnTriggerEnter(Collider col)
    {
		if (col.gameObject.CompareTag(Utilities.perfectHitTag))
        {
            didPerfectHit = true;
        }
    }
}
