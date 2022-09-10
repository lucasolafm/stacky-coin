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

    [SerializeField] private float coinFlipVolume;
    [SerializeField] private float gemFlipVolume;
    [SerializeField] private float keyFlipVolume;
    [SerializeField] private float coinCollisionVolume;
    [SerializeField] private float gemCollisionVolume;
    [SerializeField] private float keyCollisionVolume;
    [SerializeField] private float perfectHitVolume;

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
                soundSource.PlayOneShot(coinFlipClips[UnityEngine.Random.Range(0, coinFlipClips.Length)], coinFlipVolume/*, 1 + chargeTime * coinFlipPitchMult, 1 + chargeTime * coinFlipVolumeMult*/);
                break;
            case CoinType.Gem:
                soundSource.PlayOneShot(gemFlipClips[UnityEngine.Random.Range(0, gemFlipClips.Length)], gemFlipVolume/*, 1 + chargeTime * coinFlipPitchMult, 1 + chargeTime * coinFlipVolumeMult*/);
                break;
            case CoinType.Key:
                soundSource.PlayOneShot(keyFlipClip, keyFlipVolume);
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
                soundSource.PlayOneShot(coinCollisionClips[UnityEngine.Random.Range(0, coinCollisionClips.Length)], coinCollisionVolume/*, relativeVelocity * coinCollisionPitchMult, relativeVelocity * coinCollisionVolumeMult*/);
                break;
            case CoinType.Gem:
                soundSource.PlayOneShot(gemCollisionClips[UnityEngine.Random.Range(0, gemCollisionClips.Length)], gemCollisionVolume/*, relativeVelocity * coinCollisionPitchMult, relativeVelocity * coinCollisionVolumeMult*/);
                break;
            case CoinType.Key:
                soundSource.PlayOneShot(keyCollisionClip, keyCollisionVolume/*, relativeVelocity * coinCollisionPitchMult, relativeVelocity * coinCollisionVolumeMult*/);
                break;
        }
    }

    private void OnPerfectHit(Coin coin, int combo)
    {
        soundSource.PlayOneShot(perfectHitClips[Mathf.Min(combo, perfectHitClips.Length - 1) /*combo - combo / perfectHitClips.Length * perfectHitClips.Length*/], perfectHitVolume);
    }
}
