using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayAgainCoinController : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    [SerializeField] private Transform playAgainCoin;

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

        StartCoroutine(EnterAnimation());
    }

    private void OnPlayingAgain()
    {
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
                        0.7f).setEase (LeanTweenType.easeOutBack);

        // Spin
        LeanTween.rotateAroundLocal(playAgainCoin.gameObject, 
                                    Vector3.left, 720, 
                                    0.7f).setEase (LeanTweenType.easeOutQuad);
    }

    private IEnumerator ExitAnimation()
    {
        // Spin
        LeanTween.rotateAroundLocal (playAgainCoin.gameObject, Vector3.left, 1000, 0.9f);

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
                (homeManager.screenWorldWidth * (inOrOut ? 1 - shiftAmount : shiftAmount)));

            yield return null;
        }
    }
}
