using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManagerPlay : MonoBehaviour
{
    [SerializeField] private AudioSource soundSource;

    [SerializeField] private AudioClip coinFlipSound, getCoinSound, coinCollisionSound, gemCollisionSound, keyCollisionSound, coinPerfectHitSound, payCoinSound;
    [SerializeField] private float coinCollisionMinVelocity;
    [SerializeField] private float coinFlipVolumeMult, coinFlipPitchMult, coinCollisionVolumeMult, coinCollisionPitchMult;

    private List<AudioClip> soundsPlayedThisFrame = new List<AudioClip>();
    private AudioClip sound;
    private bool alreadyPlayed;

    void Start()
    {
        EventManager.CoinFlips.AddListener(OnCoinFlips);
        EventManager.CoinCollides.AddListener(OnCoinCollides);
        EventManager.PerfectHit.AddListener(OnPerfectHit);
    }

    void LateUpdate()
    {
        soundsPlayedThisFrame.Clear();
    }

    private void OnCoinFlips(Coin coin, float chargeTime)
    {
        PlaySound(coinFlipSound, 1/*, 1 + chargeTime * coinFlipPitchMult, 1 + chargeTime * coinFlipVolumeMult*/);
    }

    private void OnCoinCollides(Coin coin, float relativeVelocity)
    {   
        // Check if the velocity is high enough
        if (relativeVelocity < coinCollisionMinVelocity) return;

        switch(coin.type)
        {
            case CoinType.Coin:
                sound = coinCollisionSound; break;
            case CoinType.Gem:
                sound = gemCollisionSound; break;
            case CoinType.Key:
                sound = keyCollisionSound; break;
        }

        PlaySound(sound, 1/*, relativeVelocity * coinCollisionPitchMult, relativeVelocity * coinCollisionVolumeMult*/);
    }

    private void OnPerfectHit(Coin coin, int combo)
    {
        PlaySound(coinPerfectHitSound, 0.8f);
    }

    private void PlaySound(AudioClip sound, float volume/*, float pitch, float volume*/)
    {
        // Don't play this sound if it has already been played this frame
        alreadyPlayed = false;
        foreach (AudioClip soundClip in soundsPlayedThisFrame)
        {
            if (sound == soundClip)
            {
                alreadyPlayed = true;
                break;
            }
        }

        if (alreadyPlayed) return;

        //soundSource.pitch = pitch;

        soundSource.PlayOneShot(sound, volume); 

        soundsPlayedThisFrame.Add(sound);
    }
}
