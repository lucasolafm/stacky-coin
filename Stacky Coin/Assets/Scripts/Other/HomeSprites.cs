using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class HomeSprites : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    [SerializeField] private Camera camera;
    
    private Transform[] sprites;
    private Vector3[] startPositions;
    private float cameraWidth;

    void Awake()
    {
        sprites = transform.GetComponentsInChildren<Transform>().Where(t => t.parent == transform).ToArray();
        startPositions = sprites.Select(t => t.position).ToArray();
        cameraWidth = camera.ScreenToWorldPoint(new Vector3(Screen.width, 0)).x -
                      camera.ScreenToWorldPoint(Vector3.zero).x;

        EventManager.EnteringCollection.AddListener(OnEnteringCollection);
        EventManager.EnteringHome.AddListener(OnEnteringHome);
    }
    
    private void OnEnteringCollection()
    {
        StartCoroutine(ShiftSideways(false));
    }

    private void OnEnteringHome()
    {
        StartCoroutine(ShiftSideways(true));
    }
    
    private IEnumerator ShiftSideways(bool inOrOut)
    {
        float t = 0;
        float shiftAmount;
        Vector3 totalMove = sprites[0].TransformDirection(Vector3.right * 1.182324f);
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / GameManager.I.collectionHomeTransitionTime, 1);
            shiftAmount = t < 0.5 ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

            for (var i = 0; i < sprites.Length; i++)
            {
                sprites[i].position = startPositions[i] + totalMove * (inOrOut ? 1 - shiftAmount : shiftAmount);
            }

            yield return null;
        }
    }
}
