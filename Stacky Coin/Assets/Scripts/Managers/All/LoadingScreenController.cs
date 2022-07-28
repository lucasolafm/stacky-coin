using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private Image background;
    [SerializeField] private Image title;
    [SerializeField] private RectTransform canvas;

    private float progress;
    private float logoProgress;

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EventManager.LoadedHomeScene.AddListener(OnLoadedHomeScene);
        EventManager.TransitioningScenes.AddListener(OnTransitioningScenes);

        SetTitleToScreenHeight();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex == 1) return;

        StartCoroutine(SlideLoadingScreen(false, true)); 
    }

    private void OnLoadedHomeScene()
    {
        StartCoroutine(SlideLoadingScreen(false, false)); 
    }

    private void OnTransitioningScenes(float time)
    {
        StartCoroutine(SlideLoadingScreen(true, SceneManager.GetActiveScene().buildIndex == 0 ? true : false));
    }

    private IEnumerator SlideLoadingScreen(bool inOrOut, bool playOrHome)
    {
        background.enabled = title.enabled = true;
        background.fillOrigin = title.fillOrigin = playOrHome ? 0 : 1;

        // Wait for the lag of loading the scene to pass
        if (inOrOut == false)
        {
            yield return null;
            yield return null;
            yield return null;
        }

        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / GameManager.I.sceneTransitionTime, 1);

            progress = t < 0.5f ? 2 * t * t : 1 - Mathf.Pow(-2 * t + 2, 2) / 2;

            background.fillAmount = title.fillAmount = inOrOut ? progress : 1 - progress;

            yield return null;
        }

        if (inOrOut == false) 
        {
            background.enabled = title.enabled = false;

            EventManager.LoadingScreenSlidOut.Invoke();
        }
    }

    private void SetTitleToScreenHeight()
    {
        float screenHeight = Screen.height / canvas.localScale.y;
        float widthHeightRatio = title.rectTransform.sizeDelta.x / title.rectTransform.sizeDelta.y;
        float newWidth = screenHeight * widthHeightRatio;
        title.rectTransform.sizeDelta = new Vector2(newWidth, screenHeight);
    }
}
