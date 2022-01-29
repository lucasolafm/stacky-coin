using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CollectionCameraController : MonoBehaviour
{
    [SerializeField] private Transform[] cameras;
    [SerializeField] private RectTransform scrollContent;

    private float scrollPrevPosition; 
    private Coroutine followScrollerRoutine;
    private float cameraTopPosition;
    private float scrollContentTopPosition;

    void Start()
    {
        EventManager.EnteredCollection.AddListener(OnEnteredCollection);
        EventManager.EnteredHome.AddListener(OnEnteredHome);

        cameraTopPosition = cameras[0].localPosition.y;
        scrollContentTopPosition = scrollContent.position.y;
    }

    private void OnEnteredCollection()
    {
        followScrollerRoutine = StartCoroutine(FollowScroller());
    }

    private void OnEnteredHome()
    {
        if (followScrollerRoutine != null) StopCoroutine(followScrollerRoutine);

        ResetCamera();
    }

    private IEnumerator FollowScroller()
    {
        scrollPrevPosition = scrollContent.position.y;

        while (true)
        {
            foreach (Transform camera in cameras)
            {
                camera.localPosition += new Vector3(0, scrollPrevPosition - scrollContent.position.y, 0);
            }

            scrollPrevPosition = scrollContent.position.y;

            yield return null;
        }
    }

    private void ResetCamera()
    {
        foreach (Transform camera in cameras)
        {
            camera.localPosition = new Vector3(camera.localPosition.x, cameraTopPosition, camera.localPosition.z);
        }

        scrollContent.position = new Vector3(scrollContent.position.x, scrollContentTopPosition, scrollContent.position.z);
    }
}
