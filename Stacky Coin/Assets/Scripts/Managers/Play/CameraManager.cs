using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private PlayManager playManager;

    [SerializeField] private GameObject cameraHolder;
    [SerializeField] private Transform shakerTransformCharge, shakerTransformFallOff, shakerTransformCollision;
    [SerializeField] private Transform mainCamera;
    [SerializeField] private Transform hand;
    
    [SerializeField] private float shakeFadeIn;
    [SerializeField] private float shakeFadeOut;
    
    [SerializeField] private float shakeTimeFlip;
    [SerializeField] private float shakeSpeedFlip;
    [SerializeField] private float shakeStrengthFlip;
    [SerializeField] private float minShakeStrengthFlip;
    [SerializeField] private float maxShakeStrengthFlip;
    
    [SerializeField] private float shakeTimeCollide;
    [SerializeField] private float shakeSpeedCollide;
    [SerializeField] private float shakeStrengthCollide;
    [SerializeField] private float shakeStrengthCollideHeavy;

    [SerializeField] private float shakeSpeedCharge;
    [SerializeField] private float shakeStrengthCharge;
    [SerializeField] private float shakeTimeCharge;

    [SerializeField] private float shakeSpeedFallOff;
    [SerializeField] private float shakeStrengthFallOff;
    [SerializeField] private float shakeTimeFallOff;
    
    private CameraShaker shakerCharge;
    private CameraShaker shakerFallOff;
    private float minHeight;
    private float offsetHand;
    private bool isCharging;
    private float chargingTime;

    void Start()
    {
        EventManager.HandCharges.AddListener(OnHandCharges);
        EventManager.HandStopsCharge.AddListener(OnHandStopsCharge);
        EventManager.CoinFlips.AddListener(OnCoinFlips);
        EventManager.CoinTouchesPile.AddListener(OnCoinTouchesPile);
        EventManager.CoinLandsOnFloor.AddListener(OnCoinLandsOnFloor);
        EventManager.GoneGameOver.AddListener(OnGoneGameOver);

        minHeight = cameraHolder.transform.position.y;
        offsetHand = cameraHolder.transform.position.y - hand.position.y;
        shakerCharge = new CameraShaker(shakeStrengthCharge, shakeSpeedCharge, shakeTimeCharge, ref shakerTransformCharge, mainCamera.rotation);
        shakerFallOff = new CameraShaker(shakeStrengthFallOff, shakeSpeedFallOff, shakeTimeFallOff, ref shakerTransformFallOff, mainCamera.rotation);
    }

    void Update()
    {
        if (!GameManager.I.isGameOver)
        {
            cameraHolder.transform.position = new Vector3(cameraHolder.transform.position.x, Mathf.Max(hand.position.y + offsetHand, minHeight),
                cameraHolder.transform.position.z);
        
            if (isCharging)
            {
                chargingTime += Time.deltaTime;
                if (chargingTime > 0.1f)
                {
                    if (chargingTime < 4.224f)
                    {
                        shakerCharge.AddShakeMultiplier();   
                    }
                    else
                    {
                        shakerCharge.StabilizeShakeMultiplier();
                    }
                }
            }

            shakerCharge.Tick();
        }

        shakerFallOff.Tick();
    }
    
    private void OnHandCharges(Coin arg0)
    {
        isCharging = true;
    }
    
    private void OnHandStopsCharge()
    {
        isCharging = false;
        chargingTime = 0;
    }

    private void OnCoinFlips(Coin coin, float chargeTime)
    {
        StartCoroutine(ShakeOnce(shakeTimeFlip, 
            minShakeStrengthFlip + chargeTime / 4.224f * (maxShakeStrengthFlip - minShakeStrengthFlip), shakeSpeedFlip));
    }

    private void OnCoinTouchesPile(Coin coin)
    {
        StartCoroutine(ShakeOnce(shakeTimeCollide, 
            coin.type == CoinType.Coin ? shakeStrengthCollide : shakeStrengthCollideHeavy, shakeSpeedCollide));
    }

    private void OnCoinLandsOnFloor(Coin coin)
    {
        shakerFallOff.AddShakeMultiplier();
    }

    private void OnGoneGameOver(bool manualGameOver)
    {
        if (manualGameOver) return;

        LeanTween.moveY(cameraHolder.gameObject, 0, 
                        0.4f).setEase(LeanTweenType.easeInOutQuad);
    }

    private IEnumerator ShakeOnce(float duration, float maxStrength, float maxSpeed)
    {
        float time = 0;
        float t = 0;
        float shakeMultiplier;
        float strength;
        float speed;
        
        while (t < 1)
        {
            time += Time.deltaTime;
            t = Mathf.Min(time / duration, 1);

            shakeMultiplier = t < shakeFadeIn ? Utilities.EaseInQuad(t / shakeFadeIn) :
                t > shakeFadeOut ? 1 - Utilities.EaseOutQuad((t - shakeFadeOut) / (1 - shakeFadeOut)) : 1;
            
            strength = shakeMultiplier * maxStrength;
            speed = shakeMultiplier * maxSpeed;

            shakerTransformCollision.localPosition = GetShakePosition(time, strength, speed);
            
            yield return null;
        }

        shakerTransformCollision.localPosition = Vector3.zero;
    }

    private Vector3 GetShakePosition(float time, float strength, float speed)
    {
        return mainCamera.rotation * new Vector3((Mathf.PerlinNoise(time * speed, time * speed * 2) - 0.5f) * strength,
            (Mathf.PerlinNoise(time * speed * 2, time * speed) - 0.5f) * strength);
    }
}
