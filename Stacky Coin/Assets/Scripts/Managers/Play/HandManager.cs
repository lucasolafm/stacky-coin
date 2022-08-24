using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class HandManager : MonoBehaviour
{
    [SerializeField] private PlayManager playManager;
    [SerializeField] private CameraManager cameraManager;

    public Transform hand;
    public Transform handVisuals;
    public Collider handCollider;
    public GameObject[] sprites;
    [SerializeField] private Renderer coinToMeasure;

    public float moveDownTime;
    public float handBelowTopCoinDistance;
    public float firstHandMovePosition;

    public float bobStrengthLight, bobStrenghtHeavy, bobTime;

    public float flipMovementMin, flipMovementMax, FlipMovementTimeUp, FlipMovementTimePause, FlipMovementTimeDown;
    [SerializeField] private float shakeOverTime;

    private float elapsedTime;
    private Vector3 handVisualsPosition;

    private bool bobbing;
    private float bobStartTime, bobStrength;

    private bool flipping;
    private float flipStartTime, flipStrength;

    private bool shaking;
    private float shakeStartTime, shakeSize;

    void Start()
    {
        EventManager.CoinLandsOnHand.AddListener(OnCoinLandsOnHand);
        EventManager.HandCharges.AddListener(OnHandCharges);
        EventManager.HandStopsCharge.AddListener(OnHandStopsCharge);
        EventManager.CoinFlipping.AddListener(OnCoinFlipping);
        EventManager.CoinFlips.AddListener(OnCoinFlips);
        EventManager.ReachesNextStageTarget.AddListener(OnReachesNextStageTarget);
        EventManager.GoneGameOver.AddListener(OnGoneGameOver);
        
        print(coinToMeasure.bounds.size.y);
    }

    void Update()
    {
        handVisualsPosition = hand.position;

        // Bob animation
        elapsedTime = Time.time - bobStartTime;
        if (bobbing && elapsedTime < bobTime)
        {
            handVisualsPosition -= new Vector3(0, bobStrength * 
                                                    (elapsedTime < bobTime / 2 ? 
                                                    Utilities.EaseOutQuad(elapsedTime / (bobTime / 2)) : 
                                                    1 - Utilities.EaseInOutQuad((elapsedTime - bobTime / 2) / (bobTime / 2))), 0);
        }
        
        // Flipping animation
        elapsedTime = Time.time - flipStartTime;
        if (flipping && elapsedTime < FlipMovementTimeUp + FlipMovementTimePause + FlipMovementTimeDown)
        {            
            handVisualsPosition += new Vector3(0, flipStrength *
                                                    (elapsedTime < FlipMovementTimeUp + FlipMovementTimePause ?
                                                    Utilities.EaseOutQuad(Mathf.Min(elapsedTime / FlipMovementTimeUp, 1)) :
                                                    1 - Utilities.EaseInOutQuad((elapsedTime - FlipMovementTimeUp - FlipMovementTimePause) / 
                                                    FlipMovementTimeDown)), 0);                                   
        }

        // Shaking when charging
        if (shaking)
        {
            if (Time.time - shakeStartTime <= 1f)
            {
                shakeSize += shakeOverTime;
            }

            handVisualsPosition += Random.insideUnitSphere * shakeSize;
        }

        handVisuals.position = handVisualsPosition;
    }

    private void OnCoinLandsOnHand(Coin coin)
    {
        BobMovement(coin);
    }

    private void OnCoinFlipping(Coin coin, float chargeTime)
    {
        StopBobbing();
    }

    private void OnCoinFlips(Coin coin, float chargeTime)
    {
        StartCoroutine(FlipAnimation(chargeTime));
        FlipMovement(chargeTime);
    }

    private void OnHandCharges(Coin coin)
    {
        if (ScreenshotTool.PauseOnRelease) return;
        Shake();
    }

    private void OnHandStopsCharge()
    {
        StopShaking();
    }

    private void OnReachesNextStageTarget(Coin scoredCoin, float handHeight)
    {
        LeanTween.moveLocal(hand.gameObject, 
                            new Vector3(Mathf.Clamp(scoredCoin.transform.position.x - 0.8f, 2.108f, 2.216f), handHeight, hand.position.z), 
                            playManager.timeToAscendToNextStage).setEase(LeanTweenType.easeInOutQuad).setOnComplete(() => 
        {
            EventManager.HandAscendedCoinPile.Invoke();
        });
    }

    public void AdjustHandPosition(float pileTopPosition, int moveDownCoinAmount, Action completed)
    {
        playManager.SetState(new PlayHandAdjusting(playManager));
        EventManager.HandStopsCharge.Invoke();
        StopBobbing();

        LeanTween.moveLocal(hand.gameObject, new Vector3(pileTopPosition != 0 ? pileTopPosition - 0.8f : hand.position.x, 
                hand.position.y - coinToMeasure.bounds.size.y * Mathf.Max(moveDownCoinAmount, 0), hand.position.z), 
                moveDownTime).setEase(LeanTweenType.easeInOutSine).setOnComplete(() =>
        {
            playManager.SetState(new PlayDefault(playManager));
            completed();
        });
    }

    private void OnGoneGameOver(bool manualGameOver)
    {
        if (manualGameOver) return;

        // Stop any hand movements
        StopBobbing();

        // Move the hand away
        LeanTween.moveX(hand.gameObject, 
                        hand.transform.position.x - 1.5f, 
                        0.6f).setEase(LeanTweenType.easeInBack);
    }

    public void AscendHand(Vector3 topCoinPos, Action completed)
    {
        StartCoroutine(AscendHandRoutine(topCoinPos, () =>
        {
            EventManager.HandAscendedCoinPile.Invoke();
            completed();
        }));
    }

    private IEnumerator AscendHandRoutine(Vector3 topCoinPos, Action completed)
    {
        float t = 0;
        float height = playManager.nextStageTarget < 20
            ? playManager.handManager.firstHandMovePosition
            : topCoinPos.y - playManager.handManager.handBelowTopCoinDistance;
        Vector3 startPos = hand.position;
        Vector3 endPos = new Vector3(Mathf.Clamp(topCoinPos.x - 0.8f, 2.108f, 2.216f), 
            height, hand.position.z);
        
        cameraManager.AscendCamera(height);

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / playManager.timeToAscendToNextStage, 1);
            hand.position = Vector3.Lerp(startPos, endPos, Utilities.EaseInOutSine(t));
            yield return null;
        }

        completed();
    }

    private void StopBobbing()
    {
        bobbing = false;
        handVisuals.position = hand.position;
    }

    private void StopShaking()
    {
        shaking = false;
        handVisuals.position = hand.position;
    }

    private IEnumerator FlipAnimation(float chargeTime)
	{
		sprites[0].SetActive(false);
		sprites[1].SetActive(true);
		yield return new WaitForSeconds(0.0075f - (chargeTime * 0.5f));
		sprites[1].SetActive(false);
		sprites[2].SetActive(true);
		yield return new WaitForSeconds(0.0075f - (chargeTime * 0.5f));
		sprites[2].SetActive(false);
		sprites[3].SetActive(true);
		yield return new WaitForSeconds(0.4f);
		sprites[3].SetActive(false);
		sprites[2].SetActive(true);
		yield return new WaitForSeconds(0.01f);
		sprites[2].SetActive(false);
		sprites[1].SetActive(true);
		yield return new WaitForSeconds(0.01f);
		sprites[1].SetActive(false);
		sprites[0].SetActive(true);
	}

    private void BobMovement(Coin coin)
    {
        bobbing = true;
        bobStartTime = Time.time;
        bobStrength = coin.type == CoinType.Coin ? bobStrengthLight : bobStrenghtHeavy;
    }

    private void FlipMovement(float chargetime)
    {
        flipping = true;
        flipStartTime = Time.time;
        flipStrength = flipMovementMin + (flipMovementMax - flipMovementMin) * (chargetime / 4.224f);
    }

    private void Shake()
    {
        shaking = true;
        shakeStartTime = Time.time;
        shakeSize = 0;
    }
}
