using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HighscoreLine : MonoBehaviour
{
    [SerializeField] private new Renderer renderer;

    void Start()
    {
        if (Data.highPileHeight <= -1) return;

        transform.position = new Vector3(transform.position.x, Data.highPileHeight, transform.position.z);

        renderer.enabled = true;
    }
}
