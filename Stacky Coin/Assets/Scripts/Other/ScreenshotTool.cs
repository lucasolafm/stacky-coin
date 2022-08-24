using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class ScreenshotTool : MonoBehaviour
{
    public static bool IsPaused;
    public static bool PauseOnRelease;

    private bool _nextFrame;
    private bool _skippingFrames;
    private int _framesSkipped;

    void Update()
    {
        if (Input.GetMouseButtonUp(0) && EventSystem.current.currentSelectedGameObject == null && PauseOnRelease)
        {
            TogglePause();
            PauseOnRelease = false;
        }
        
        if (_skippingFrames)
        {
            _framesSkipped++;
            if (_framesSkipped > 1)
            {
                TogglePause();
                _skippingFrames = false;
                _framesSkipped = 0;
            }
        }

        if (_nextFrame)
        {
            TogglePause();
            _nextFrame = false;
            _skippingFrames = true;
        }
    }
    
    public void OnHideButton()
    {
        gameObject.GetComponent<Canvas>().enabled = false;
        StartCoroutine(Hiding());
    }

    public void OnPauseButton()
    {
        TogglePause();
    }

    public void OnNextFrameButton()
    {
        _nextFrame = true;
    }

    public void OnPauseOnRelease()
    {
        PauseOnRelease = true;
    }

    private void TogglePause()
    {
        Time.timeScale = IsPaused ? 1 : 0;
        ParticleSystem[] particleSystems = FindObjectsOfType<ParticleSystem>();
        foreach (ParticleSystem system in particleSystems)
        {
            ParticleSystem.MainModule main = system.main;
            main.simulationSpeed = IsPaused ? 1 : 0;
        }
        
        IsPaused = !IsPaused;
    }

    private IEnumerator Hiding()
    {
        yield return new WaitForSecondsRealtime(5);
        gameObject.GetComponent<Canvas>().enabled = true;
    }
}