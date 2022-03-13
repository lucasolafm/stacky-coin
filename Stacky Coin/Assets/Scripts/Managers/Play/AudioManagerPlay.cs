using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class AudioManagerPlay : MonoBehaviour
{
    [SerializeField] private AudioSource soundSource;

    [SerializeField] private AudioClip keyFlipClip, keyCollisionClip;
    [SerializeField] private float coinCollisionMinVelocity;
    [SerializeField] private float coinFlipVolumeMult, coinFlipPitchMult, coinCollisionVolumeMult, coinCollisionPitchMult;

    [SerializeField] private AudioClip[] coinCollisionClips;
    [SerializeField] private AudioClip[] gemCollisionClips;
    [SerializeField] private AudioClip[] coinFlipClips;
    [SerializeField] private AudioClip[] gemFlipClips;
    [SerializeField] private AudioClip[] perfectHitClips;

    void Start()
    {
        EventManager.CoinFlips.AddListener(OnCoinFlips);
        EventManager.CoinCollides.AddListener(OnCoinCollides);
        EventManager.PerfectHit.AddListener(OnPerfectHit);
    }

    private void OnCoinFlips(Coin coin, float chargeTime)
    {
        switch (coin.type)
        {
            case CoinType.Coin:
                soundSource.PlayOneShot(coinFlipClips[UnityEngine.Random.Range(0, coinFlipClips.Length)], 1/*, 1 + chargeTime * coinFlipPitchMult, 1 + chargeTime * coinFlipVolumeMult*/);
                break;
            case CoinType.Gem:
                soundSource.PlayOneShot(gemFlipClips[UnityEngine.Random.Range(0, gemFlipClips.Length)], 1/*, 1 + chargeTime * coinFlipPitchMult, 1 + chargeTime * coinFlipVolumeMult*/);
                break;
            case CoinType.Key:
                soundSource.PlayOneShot(keyFlipClip, 0.8f);
                break;
        }
    }

    private void OnCoinCollides(Coin coin, float relativeVelocity)
    {   
        // Check if the velocity is high enough
        if (relativeVelocity < coinCollisionMinVelocity) return;

        switch(coin.type)
        {
            case CoinType.Coin:
                soundSource.PlayOneShot(coinCollisionClips[UnityEngine.Random.Range(0, coinCollisionClips.Length)], 1.2f/*, relativeVelocity * coinCollisionPitchMult, relativeVelocity * coinCollisionVolumeMult*/);
                break;
            case CoinType.Gem:
                soundSource.PlayOneShot(gemCollisionClips[UnityEngine.Random.Range(0, gemCollisionClips.Length)], 1.2f/*, relativeVelocity * coinCollisionPitchMult, relativeVelocity * coinCollisionVolumeMult*/);
                break;
            case CoinType.Key:
                soundSource.PlayOneShot(keyCollisionClip, 0.8f/*, relativeVelocity * coinCollisionPitchMult, relativeVelocity * coinCollisionVolumeMult*/);
                break;
        }
    }

    private void OnPerfectHit(Coin coin, int combo)
    {
        soundSource.PlayOneShot(perfectHitClips[combo - combo / perfectHitClips.Length * perfectHitClips.Length], 0.8f);
    }
}
