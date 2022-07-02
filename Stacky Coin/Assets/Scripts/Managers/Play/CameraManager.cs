using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private PlayManager playManager;

    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private Transform shaker;
    [SerializeField] private Transform mainCamera;
    
    [SerializeField] private float shakeFadeIn;
    [SerializeField] private float shakeFadeOut;
    
    [SerializeField] private float shakeTimeFlip;
    [SerializeField] private float shakeSpeedFlip;
    [SerializeField] private float shakeStrengthFlip;
    
    [SerializeField] private float shakeTimeCollide;
    [SerializeField] private float shakeSpeedCollide;
    [SerializeField] private float shakeStrengthCollide;
    [SerializeField] private float shakeStrengthCollideHeavy;

    [SerializeField] private float shakeSpeedFallOff;
    [SerializeField] private float shakeStrengthFallOff;
    [SerializeField] private float shakeTimeFallOff;

    private float shakeContinuousStrength;
    private float shakeContinuousStrengthFloorRatio;

    void Start()
    {
        EventManager.CoinFlips.AddListener(OnCoinFlips);
        EventManager.CoinLandsOnPile.AddListener(OnCoinLandsOnPile);
        EventManager.CoinLandsOnFloor.AddListener(OnCoinLandsOnFloor);
        EventManager.GoneGameOver.AddListener(OnGoneGameOver);
        
        StartCoroutine(ShakeContinuously(shakeSpeedFallOff, shakeTimeFallOff));
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            //shakeContinuousStrength += shakeStrengthFallOff;
        }

        //if (shakeContinuousStrength > 0) print(shakeContinuousStrength);
    }

    private void OnCoinFlips(Coin coin, float chargeTime)
    {
        //StartCoroutine(ShakeOnce(shakeTimeFlip, shakeStrengthFlip, shakeSpeedFlip));
    }

    private void OnCoinLandsOnPile(Coin coin)
    {
        //StartCoroutine(ShakeOnce(shakeTimeCollide, 
            //coin.type == CoinType.Coin ? shakeStrengthCollide : shakeStrengthCollideHeavy, shakeSpeedCollide));
    }

    private void OnCoinLandsOnFloor(Coin coin)
    {
        shakeContinuousStrength += shakeStrengthFallOff;
    }

    private void OnGoneGameOver(bool manualGameOver)
    {
        if (manualGameOver) return;

        LeanTween.moveY(cameraHolder.gameObject, 0, 
                        0.4f).setEase(LeanTweenType.easeInOutQuad);
    }

    public void AscendCamera(float handHeight)
    {
        LeanTween.moveY(cameraHolder.gameObject, 
            handHeight + 1.212f,
            playManager.timeToAscendToNextStage).setEase(LeanTweenType.easeInOutSine);
    }

    private IEnumerator ShakeOnce(float duration, float maxStrength, float speed)
    {
        float time = 0;
        float t = 0;
        float strength;
        
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.Min(time / duration, 1);

            strength = (t < shakeFadeIn ? Utilities.EaseInQuad(t / shakeFadeIn) :
                t > shakeFadeOut ? 1 - Utilities.EaseOutQuad((t - shakeFadeOut) / (1 - shakeFadeOut)) : 1) * maxStrength;

            shaker.localPosition = GetShakePosition(time, strength, speed);
            
            yield return null;
        }

        shaker.localPosition = Vector3.zero;
    }

    private IEnumerator ShakeContinuously(float speed, float duration)
    {
        float time = 0;
        float t = 0;
        float strength = 0;

        while (true)
        {
            time += Time.deltaTime;

            yield return new WaitForEndOfFrame();
            
            t = Mathf.Min((shakeContinuousStrength > strength ? 0 : t) + Time.deltaTime / duration, 1);

            shakeContinuousStrength *= 1 - Utilities.EaseOutQuad(t);
            strength = shakeContinuousStrength;
            
            shaker.localPosition = GetShakePosition(time, strength, speed);

            yield return null;
        }

        shaker.localPosition = Vector3.zero;
    }

    private Vector3 GetShakePosition(float time, float strength, float speed)
    {
        return mainCamera.rotation * new Vector3((Mathf.PerlinNoise(time * speed, time * speed * 2) - 0.5f) * strength,
            (Mathf.PerlinNoise(time * speed * 2, time * speed) - 0.5f) * strength);
    }
}
