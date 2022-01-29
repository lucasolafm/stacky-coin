using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChargingEffectController : MonoBehaviour
{
    [SerializeField] private CoinManager coinManager;
    [SerializeField] private ParticleSystem chargingEffect;
    [SerializeField] private SpriteRenderer pullEffect;
    [SerializeField] private Transform hand;
    [SerializeField] private float delay;
    [SerializeField] private float radiusMin, radiusMax;
    [SerializeField] private float velocityMin, velocityMax;
    [SerializeField] private float emissionMin, emissionMax;
    [SerializeField] private float trailLifeTimeMin, trailLifeTimeMax;
    [SerializeField] private float trailWidthMin, trailWidthMax;
    [SerializeField] private float pullTimeMin, pullTimeMax;
    [SerializeField] private float pullSizeMin, pullSizeMax;
    [SerializeField] private float pullTransparency;
    [SerializeField] private float maxIntensity;
    [SerializeField] private AnimationCurve progressionCurve;

    private WaitForSeconds delayWait;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;
    private ParticleSystem.ShapeModule shapeModule;
    private ParticleSystem.EmissionModule emissionModule;
    private ParticleSystem.TrailModule trailModule;
    private Coroutine emitParticlesRoutine;
    private float chargingTime;
    private float progression;

    void Awake()
    {
        velocityModule = chargingEffect.velocityOverLifetime;
        shapeModule = chargingEffect.shape;
        emissionModule = chargingEffect.emission;
        trailModule = chargingEffect.trails;
    }

    void Start()
    {
        EventManager.HandCharges.AddListener(OnHandCharges);
        EventManager.HandStopsCharge.AddListener(OnHandStopsCharge);

        chargingEffect.transform.parent = hand;

        delayWait = new WaitForSeconds(delay);
    }

    private void OnHandCharges(Coin coin)
    {
        emitParticlesRoutine = StartCoroutine(EmitParticles());
    }    

    private void OnHandStopsCharge()
    {
        StopEmittingParticles();
    }

    private void OnGoneGameOver(bool manualGameOver)
    {
        StopEmittingParticles();
    }

    private IEnumerator EmitParticles()
    {
        yield return delayWait;

        chargingTime = 0;
        float t = 0;
        chargingEffect.gameObject.SetActive(true);
        chargingEffect.Play();

        while (true)
        {
            chargingTime += Time.deltaTime;

            progression = progressionCurve.Evaluate(Mathf.Min(chargingTime / coinManager.maxChargeTimeLight, maxIntensity));

            LinesEffect(progression);

            t = Mathf.Min(t + Time.deltaTime / (pullTimeMin + progression * (pullTimeMax - pullTimeMin)), 1);
            PullEffect(t, progression);
            t = t < 1 ? t : 0;

            yield return null;
        }
    }

    private void LinesEffect(float progression)
    {
        velocityModule.radial = velocityMin + progression * (velocityMax - velocityMin);

        shapeModule.radius = radiusMin + progression * (radiusMax - radiusMin);

        emissionModule.rateOverTime = emissionMin + progression * (emissionMax - emissionMin);

        trailModule.lifetime = trailLifeTimeMin + progression * (trailLifeTimeMax - trailLifeTimeMin);

        trailModule.widthOverTrail = trailWidthMin + progression * (trailWidthMax - trailWidthMin);
    }

    private void PullEffect(float t, float progression)
    {
        pullEffect.transform.localScale = (pullSizeMin + progression * (pullSizeMax - pullSizeMin)) * (1 - t) * new Vector2(1, 1);

        pullEffect.color = new Color(pullEffect.color.r, pullEffect.color.g, pullEffect.color.b, t * pullTransparency);
    }

    private void StopEmittingParticles()
    {
        if (emitParticlesRoutine != null) StopCoroutine(emitParticlesRoutine);
        chargingEffect.Stop();
        chargingEffect.gameObject.SetActive(false);
    }
}
