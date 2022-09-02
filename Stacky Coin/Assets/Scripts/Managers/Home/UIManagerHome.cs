using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class UIManagerHome : MonoBehaviour
{
    [SerializeField] private RectTransform staticCanvas, dynamicCanvas;
    [HideInInspector] public List<RectTransform> uiElements = new List<RectTransform>();
    [HideInInspector] public List<Vector3> uiElementsStartPositions = new List<Vector3>();
    [SerializeField] private TextMeshProUGUI score, highScore;
    [SerializeField] private RectTransform collectionButton;
    [SerializeField] private RectTransform highScoreCrown; 

    [SerializeField] private HighScoreCrownInfo highScoreCrownInfo;

    private float shiftProgress, shiftAmount;
    private int uiElementsCount;

    void Awake()
    {
        EventManager.NewHighScore.AddListener(OnNewHighScore);  
    }

    void Start()
    {
        EventManager.EnteringCollection.AddListener(OnEnteringCollection);
        EventManager.EnteringHome.AddListener(OnEnteringHome);

        score.text = GameManager.I.finalScore.ToString();
        highScore.text = Data.highScore.ToString();

        for (int i = 0; i < staticCanvas.childCount; i++)
        {
            if (staticCanvas.GetChild(i).parent != staticCanvas) continue;

            uiElements.Add(staticCanvas.GetChild(i).GetComponent<RectTransform>());

            uiElementsStartPositions.Add(staticCanvas.GetChild(i).localPosition);

            uiElementsCount++;
        }
    }

    private void OnNewHighScore()
    {
        StartCoroutine(BounceHighScoreCrown());
    }

    private void OnEnteringCollection()
    {
        StartCoroutine(ShiftUISideways(false));

        StartCoroutine(ExpandCollectionButton(false));
    }

    private void OnEnteringHome()
    {
        StartCoroutine(ShiftUISideways(true));

        StartCoroutine(ExpandCollectionButton(true));
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
                    new Vector3(staticCanvas.sizeDelta.x * (inOrOut == true ? 1 - shiftAmount : shiftAmount), 0, 0);
            }

            yield return null;
        }
    }   

    private IEnumerator ExpandCollectionButton(bool inOrOut)
    {
        if (inOrOut == true)
        {
            yield return new WaitForSeconds(GameManager.I.collectionHomeTransitionTime / 2);
        }

        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / (GameManager.I.collectionHomeTransitionTime / 2), 1);

            collectionButton.localScale = new Vector3(collectionButton.localScale.y * 
                                                        (inOrOut ? 
                                                        Mathf.Sin(t * Mathf.PI / 2) : 
                                                        1 - (1 - Mathf.Cos(t * Mathf.PI / 2))), 
                                                        collectionButton.localScale.y, 1);

            yield return null;
        }
    }

    private IEnumerator BounceHighScoreCrown()
    {
        float t;
        Vector3 startPos = highScoreCrown.localPosition;

        while (true)
        {
            t = 0;
            while (t < 1)
            {
                t = Mathf.Min(t + Time.deltaTime / highScoreCrownInfo.bounceTime, 1);

                highScoreCrown.localPosition = startPos + new Vector3(0, Mathf.Sin((t * Mathf.PI) / 2) * highScoreCrownInfo.bounceHeight, 0);

                yield return null;
            }

            t = 0;
            while (t < 1)
            {
                t = Mathf.Min(t + Time.deltaTime / highScoreCrownInfo.bounceTime, 1);

                highScoreCrown.localPosition = startPos + new Vector3(0, (1 - (1 - Mathf.Cos((t * Mathf.PI) / 2))) * highScoreCrownInfo.bounceHeight, 0);

                yield return null;
            }
        }
    }
}
