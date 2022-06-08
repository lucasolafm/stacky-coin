using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class CoinSpawned : CoinState
{
    public CoinSpawned(Coin coin) : base(coin) {}

    public override void Enter()
    {
        base.Enter();

        coin.rb.isKinematic = true;
        coin.gameObject.SetActive(true);

        coin.StartCoroutine(EnterAnimation(() =>
        {
            coin.SetState(new CoinOnHand(coin));
        }));
    }

    private IEnumerator EnterAnimation(Action completed)
    {
        float t = 0;
        float progress;
        Vector3 endPos = coin.coinManager.handManager.hand.position + new Vector3(0, 0.972397f);
        Vector3 startPos = new Vector3(1.386f, endPos.y + 0.5f, endPos.z);
        
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / 0.2f, 1);

            progress = Utilities.EaseOutSine(t);

            coin.transform.position = new Vector3(startPos.x + (endPos.x - startPos.x) * progress,
                startPos.y + (endPos.y - startPos.y) * Utilities.EaseInCubic(progress), endPos.z);

            yield return null;
        }

        completed();
    }
}
