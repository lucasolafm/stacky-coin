using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectManagerPlay : MonoBehaviour
{
    [SerializeField] private ParticleSystem flipLaunchEffect, collisionEffect;
    [SerializeField] private ParticleSystem[] perfectHitEffects;
    [SerializeField] private float cloudMultiplier;
    [SerializeField] private float perfectHitRotationSpeed;

    private Vector3 flipLaunchEffectStartScale;

    void Start()
    {
        EventManager.CoinFlipping.AddListener(OnCoinFlipping);
        EventManager.CoinTouchesPile.AddListener(OnCoinTouchesPile);
        EventManager.PerfectHit.AddListener(OnPerfectHit);

        flipLaunchEffectStartScale = flipLaunchEffect.transform.localScale;
    }
    private void OnCoinFlipping(Coin coin, float chargeTime)
    {
        flipLaunchEffect.transform.position = coin.transform.position - new Vector3(0, 0.022f, 0);
        flipLaunchEffect.transform.localScale = flipLaunchEffectStartScale;
        flipLaunchEffect.transform.localScale += new Vector3(1, 1, 1) * (chargeTime * cloudMultiplier);
        flipLaunchEffect.Play();
    }    

    private void OnCoinTouchesPile(Coin coin)
    {
        collisionEffect.transform.position = coin.transform.position - new Vector3(0, 0.04f, 0);
        collisionEffect.Play();
    }    

    private void OnPerfectHit(Coin coin, int combo)
    {
        StartCoroutine(PerfectHitEffect(coin, combo));
    }

    private IEnumerator PerfectHitEffect(Coin coin, int combo)
    {
        combo = Mathf.Min(combo, perfectHitEffects.Length - 1);

        // Place the particles in between the two coins
        perfectHitEffects[combo].transform.position = coin.transform.position - new Vector3(0, 0.022f, 0);

        perfectHitEffects[combo].Play();

        // Rotate them
        while (perfectHitEffects[combo].isPlaying)
		{
            perfectHitEffects[combo].transform.Rotate(perfectHitEffects[combo].transform.up * (perfectHitRotationSpeed * combo));                                

            yield return null;
		}
    }
}
