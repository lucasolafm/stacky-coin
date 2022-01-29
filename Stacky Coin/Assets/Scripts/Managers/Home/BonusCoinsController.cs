using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusCoinsController : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    [SerializeField] private Button bonusCoinsButton;
    [SerializeField] private Transform mainCamera, coinTubeCamera;
    [SerializeField] private ParticleSystem coinsEffect;
    [SerializeField] private List<int> bonusCoins = new List<int>();
    [SerializeField] private float timerTime;
    [SerializeField] private float pressAnimationTime;
    [SerializeField] private int particleAmount;
    [SerializeField] private ButtonShakeSettings _shakeSettings;

    void Awake()
    {
        EventManager.PressedBonusCoinsButton.AddListener(OnPressedBonusCoinsButton);
    }

    void Start()
    {
        RecordTimePlayed();

        if (IsTimeForBonusCoins())
        {
            EnableBonusCoinsButton();
        }
    }

    private void OnPressedBonusCoinsButton()
    {
        if (!homeManager.State.CanDropMiniCoins()) return;

        homeManager.State.DropMiniCoins(bonusCoins);

        bonusCoinsButton.interactable = false;

        StartCoroutine(ButtonPressedAnimation(bonusCoinsButton.transform, particleAmount));

        ResetTimer();
    }

    private void EnableBonusCoinsButton()
    {
        bonusCoinsButton.gameObject.SetActive(true);

        StartCoroutine(ShakeButton());
    }

    private bool IsTimeForBonusCoins()
    {
        return Data.bonusCoinsTimer <= 0;
    }

    private void RecordTimePlayed()
    {
        Data.bonusCoinsTimer -= GameManager.I.timePlayedLastPlay;
    }

    private void ResetTimer()
    {
        Data.bonusCoinsTimer = timerTime;
    }

    private IEnumerator ShakeButton()
    {
        float t;
        float startRotation;
        float rotationValue;
        float shakeRatio;
        float angle;
        bool firstShake;
        bool lastShake;
        Transform buttonTransform = bonusCoinsButton.transform;

        while (true)
        {
            yield return new WaitForSeconds(_shakeSettings.Delay / 2);
            
            for (int i = 0; i < _shakeSettings.ShakeAmount; i++)
            {
                t = 0;
                startRotation = buttonTransform.eulerAngles.z;
                firstShake = i == 0;
                lastShake = i == _shakeSettings.ShakeAmount - 1;
                shakeRatio = (_shakeSettings.ShakeAmount - i) / _shakeSettings.ShakeAmount;

                while (t < 1)
                {
                    t = Mathf.Min(t + Time.deltaTime / (_shakeSettings.Time * (firstShake ? 0.5f : 1)), 1);

                    angle = _shakeSettings.AngleValue * shakeRatio;

                    rotationValue = (lastShake ? startRotation : angle * (firstShake ? 0.5f : 1)) *
                                    (i % 2 == 0 ? 1 : -1) *
                                    Utilities.EaseInOutSine(t);

                    buttonTransform.rotation = Quaternion.Euler(0, 0, startRotation + rotationValue);

                    yield return null;
                }
            }
            
            yield return new WaitForSeconds(_shakeSettings.Delay / 2);
        }
    }

    public IEnumerator ButtonPressedAnimation(Transform button, int particleAmount)
    {
        PrepareCoinsEffect(button.position, particleAmount);

        coinsEffect.Play();

        float t = 0;
        Vector3 startScale = button.localScale;

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / pressAnimationTime, 1);

            button.localScale = startScale * (1 - (2.70158f * t * t * t - 1.70158f * t * t));

            yield return null;
        }

        button.gameObject.SetActive(false);
    }

    private void PrepareCoinsEffect(Vector3 buttonPosition, int particleAmount)
    {
        coinsEffect.transform.position = buttonPosition + new Vector3(0, 0, -10);/* - mainCamera.position + coinTubeCamera.position*/;

        ParticleSystem.EmissionModule emissionModule = coinsEffect.emission;
        emissionModule.SetBurst(0, new ParticleSystem.Burst(0, particleAmount));
    }
}
