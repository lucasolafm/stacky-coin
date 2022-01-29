using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class LoadingScreenController : MonoBehaviour
{
    [SerializeField] private Image loadingScreen, logo;
    [SerializeField] private Slider slider;
    [SerializeField] private RectTransform canvas;
    [SerializeField] private Sprite sprite, spriteFlipped;

    private float progress;
    private float logoProgress;

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        EventManager.LoadedHomeScene.AddListener(OnLoadedHomeScene);
        EventManager.TransitioningScenes.AddListener(OnTransitioningScenes);
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
        canvas.gameObject.SetActive(true);

        // Set slide direction of loading screen and logo
        loadingScreen.sprite = playOrHome == true ? sprite : spriteFlipped;
        loadingScreen.rectTransform.localScale = new Vector3(1, playOrHome == true ? 1 : -1, 1);
        slider.direction = playOrHome == true ? Slider.Direction.BottomToTop : Slider.Direction.TopToBottom;
        logo.fillOrigin = playOrHome == true ? 0 : 1;

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

            slider.value = inOrOut == true ? progress : 1 - progress;

            logoProgress = (Screen.height * progress - (Screen.height - (Screen.height / 2 + 
                            logo.rectTransform.rect.height * canvas.localScale.y / 2))) / 
                            (logo.rectTransform.rect.height * canvas.localScale.y);

            logo.fillAmount = inOrOut == true ? logoProgress : 1 - logoProgress;

            yield return null;
        }

        if (inOrOut == false) 
        {
            canvas.gameObject.SetActive(false);

            EventManager.LoadingScreenSlidOut.Invoke();
        }
    }
}
