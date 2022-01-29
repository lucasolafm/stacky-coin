using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManagerCollection : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    [SerializeField] private RectTransform canvas, overlayCanvas;
    [SerializeField] private RectTransform backButton;

    private List<RectTransform> uiElements = new List<RectTransform>();
    private List<Vector3> uiElementsStartPositions = new List<Vector3>();
    [SerializeField] private GameObject collectionHolder;

    private float shiftProgress, shiftAmount;
    private int uiElementsCount;

    void Start()
    {
        EventManager.EnteringCollection.AddListener(OnEnteringCollection);
        EventManager.EnteringHome.AddListener(OnEnteringHome);

        collectionHolder.SetActive(true);

        // Get all UI elements
        for (int i = 0; i < canvas.childCount; i++)
        {
            if (canvas.GetChild(i).parent != canvas) continue;

            uiElements.Add(canvas.GetChild(i).GetComponent<RectTransform>());

            uiElementsStartPositions.Add(canvas.GetChild(i).localPosition - new Vector3(canvas.sizeDelta.x, 0, 0));

            uiElementsCount++;
        }

        for (int i = 0; i < overlayCanvas.childCount; i++)
        {
            if (overlayCanvas.GetChild(i).parent != overlayCanvas) continue;

            uiElements.Add(overlayCanvas.GetChild(i).GetComponent<RectTransform>());

            uiElementsStartPositions.Add(overlayCanvas.GetChild(i).localPosition - new Vector3(canvas.sizeDelta.x, 0, 0));

            uiElementsCount++;
        }

        // Put elements to the side of the screen
        foreach (RectTransform uiElement in uiElements)
        {
            uiElement.localPosition -= new Vector3(canvas.sizeDelta.x, 0, 0);
        }

        collectionHolder.SetActive(false);
    }    

    private void OnEnteringCollection()
    {
        StartCoroutine(ShiftUISideways(true));

        StartCoroutine(ExpandBackButton(true));
    }

    private void OnEnteringHome()
    {
        StartCoroutine(ShiftUISideways(false));

        StartCoroutine(ExpandBackButton(false));
    }

    private IEnumerator ShiftUISideways(bool inOrOut)
    {
        shiftProgress = 0;
        while (shiftProgress < 1)
        {
            shiftProgress = Mathf.Min(shiftProgress + Time.deltaTime / GameManager.I.collectionHomeTransitionTime, 1);
            shiftAmount = shiftProgress < 0.5 ? 2 * shiftProgress * shiftProgress : 1 - Mathf.Pow(-2 * shiftProgress + 2, 2) / 2;

            // Move UI elements
            for (int i = 0; i < uiElementsCount; i++)
            {
                uiElements[i].localPosition = 
                    uiElementsStartPositions[i] + 
                    new Vector3(canvas.sizeDelta.x * (inOrOut == true ? shiftAmount : 1 - shiftAmount), 0, 0);
            }

            yield return null;
        }
    }   

    private IEnumerator ExpandBackButton(bool inOrOut)
    {
        if (inOrOut == true)
        {
            yield return new WaitForSeconds(GameManager.I.collectionHomeTransitionTime / 2);
        }

        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / (GameManager.I.collectionHomeTransitionTime / 2), 1);

            backButton.localScale = new Vector3(-backButton.localScale.y * 
                                                (inOrOut ? 
                                                Mathf.Sin(t * Mathf.PI / 2) : 
                                                1 - (1 - Mathf.Cos(t * Mathf.PI / 2))), 
                                                backButton.localScale.y, 1);

            yield return null;
        }
    }
}
