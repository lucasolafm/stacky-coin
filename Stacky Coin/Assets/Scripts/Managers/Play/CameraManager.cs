using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    [SerializeField] private PlayManager playManager;

    [SerializeField] private GameObject cameras;

    void Start()
    {
        EventManager.ReachesNextStageTarget.AddListener(OnReachesNextStageTarget);
        EventManager.GoneGameOver.AddListener(OnGoneGameOver);
    }

    private void OnReachesNextStageTarget(Coin scoredCoin, float handHeight)
    {
        // Ascend coin pile
        LeanTween.moveY(cameras, 
                        handHeight + 1.212f,
                        playManager.timeToAscendToNextStage).setEase(LeanTweenType.easeInOutQuad);
    }

    private void OnGoneGameOver(bool manualGameOver)
    {
        if (manualGameOver) return;

        // Move down to show the coins falling
        LeanTween.moveY(cameras, 0, 
                        0.4f).setEase(LeanTweenType.easeInOutQuad);
    }
}
