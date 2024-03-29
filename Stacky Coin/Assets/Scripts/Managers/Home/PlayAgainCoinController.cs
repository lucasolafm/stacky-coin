﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayAgainCoinController : MonoBehaviour
{
    public static bool EnteringScreen;
    
    [SerializeField] private Transform playAgainCoin;
    [SerializeField] private Transform visual;
    [SerializeField] private Collider collider;
    [SerializeField] private AudioClip enterClip;
    [SerializeField] private AudioClip playAgainClip;
    [SerializeField] private float enterClipDelay;
    
    private Vector3 startPosition;
    private float shiftProgress, shiftAmount;

    void Awake()
    {
        EventManager.LoadedHomeScene.AddListener(OnLoadedHomeScene);
        EventManager.PlayingAgain.AddListener(OnPlayingAgain);
        EventManager.EnteringCollection.AddListener(OnEnteringCollection);
        EventManager.EnteringHome.AddListener(OnEnteringHome);
    }

    private void OnLoadedHomeScene()
    {
        startPosition = playAgainCoin.position;
        
        Invoke(nameof(PlayEnterSound), enterClipDelay);

        EnteringScreen = true;
        StartCoroutine(EnterAnimation());
    }

    private void OnPlayingAgain()
    {
        collider.enabled = false;
        
        GameManager.I.audioSource.PlayOneShot(playAgainClip, 0.5f);

        StartCoroutine(ExitAnimation());
    }

    private void OnEnteringCollection()
    {
        StartCoroutine(ShiftSideways(false));
    }

    private void OnEnteringHome()
    {
        StartCoroutine(ShiftSideways(true));
    }

    private IEnumerator EnterAnimation()
    {
        playAgainCoin.position += new Vector3(0, -1.2f, 0);

        yield return null;

        // Move up
        LeanTween.move(playAgainCoin.gameObject, 
                        startPosition, 
                        0.7f).setEase (LeanTweenType.easeOutBack).setOnComplete(() =>
        {
            collider.enabled = true;
            EnteringScreen = false;
        });

        // Spin
        LeanTween.rotateAroundLocal(visual.gameObject, 
                                    Vector3.left, 720, 
                                    0.7f).setEase (LeanTweenType.easeOutQuad);
    }

    private IEnumerator ExitAnimation()
    {
        // Spin
        LeanTween.rotateAroundLocal (visual.gameObject, Vector3.left, 1000, 0.9f);

        // Move up
        LeanTween.moveY (playAgainCoin.gameObject, playAgainCoin.transform.position.y + 0.1f, 0.1f).setEase (LeanTweenType.easeOutQuad); 

        yield return new WaitForSeconds(0.1f);
        
        // Move down
        LeanTween.moveY (playAgainCoin.gameObject, playAgainCoin.transform.position.y - 1.2f, 0.5f).setEase (LeanTweenType.easeInQuad);
    }

    private IEnumerator ShiftSideways(bool inOrOut)
    {
        shiftProgress = 0;
        while (shiftProgress < 1)
        {
            shiftProgress = Mathf.Min(shiftProgress + Time.deltaTime / GameManager.I.collectionHomeTransitionTime, 1);
            shiftAmount = shiftProgress < 0.5 ? 2 * shiftProgress * shiftProgress : 1 - Mathf.Pow(-2 * shiftProgress + 2, 2) / 2;

            playAgainCoin.localPosition = 
                startPosition - playAgainCoin.TransformDirection(Vector3.right * 
                (1.18228f * (inOrOut ? 1 - shiftAmount : shiftAmount)));

            yield return null;
        }
    }
    

    private void PlayEnterSound()
    {
        GameManager.I.audioSource.PlayOneShot(enterClip);
    }
}
