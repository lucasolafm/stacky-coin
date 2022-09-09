using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BonusCoinsController : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    [SerializeField] private SpriteRenderer bonusCoinsButton;
    [SerializeField] private Transform mainCamera, coinTubeCamera;
    [SerializeField] private ParticleSystem coinsEffect;
    [SerializeField] private AudioClip coinsGetClip;
    public List<int> bonusCoins = new List<int>();
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

        bonusCoinsButton.GetComponent<Collider>().enabled = false;

        StartCoroutine(ButtonPressedAnimation(bonusCoinsButton, particleAmount));
        
        GameManager.I.audioSource.PlayOneShot(coinsGetClip, 1);

        ResetTimer();
    }

    private void EnableBonusCoinsButton()
    {
        bonusCoinsButton.enabled = true;
        bonusCoinsButton.GetComponent<Collider>().enabled = true;

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
        Quaternion startRotation;
        float rotationValue;
        float shakeRatio;
        float angle;
        bool firstShake;
        bool lastShake;
        float lastShakeAngle = 0;
        Transform buttonTransform = bonusCoinsButton.transform;

        while (true)
        {
            yield return new WaitForSeconds(_shakeSettings.Delay / 2);
            
            for (int i = 0; i < _shakeSettings.ShakeAmount; i++)
            {
                t = 0;
                startRotation = buttonTransform.rotation;
                firstShake = i == 0;
                lastShake = i == _shakeSettings.ShakeAmount - 1;
                if (lastShake) lastShakeAngle = buttonTransform.eulerAngles.z;
                shakeRatio = (_shakeSettings.ShakeAmount - i) / _shakeSettings.ShakeAmount;

                while (t < 1)
                {
                    t = Mathf.Min(t + Time.deltaTime / (_shakeSettings.Time * (firstShake ? 0.5f : 1)), 1);

                    angle = _shakeSettings.AngleValue * shakeRatio;

                    rotationValue = (lastShake ? lastShakeAngle : angle * (firstShake ? 0.5f : 1)) *
                                    (i % 2 == 0 ? 1 : -1) *
                                    Utilities.EaseInOutSine(t);

                    buttonTransform.rotation = startRotation * Quaternion.Euler(0, 0, rotationValue);

                    yield return null;
                }
            }
            
            yield return new WaitForSeconds(_shakeSettings.Delay / 2);
        }
    }

    public IEnumerator ButtonPressedAnimation(SpriteRenderer button, int particleAmount)
    {
        PrepareCoinsEffect(button.transform.position, particleAmount);

        coinsEffect.Play();

        float t = 0;
        Vector3 startScale = button.transform.localScale;

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / pressAnimationTime, 1);

            button.transform.localScale = startScale * (1 - (2.70158f * t * t * t - 1.70158f * t * t));

            yield return null;
        }

        button.enabled = false;
    }

    private void PrepareCoinsEffect(Vector3 buttonPosition, int particleAmount)
    {
        coinsEffect.transform.position = buttonPosition;

        ParticleSystem.EmissionModule emissionModule = coinsEffect.emission;
        emissionModule.SetBurst(0, new ParticleSystem.Burst(0, particleAmount));
    }
}
