using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockChestPreparer
{
    private UnlockSkinManager manager;

    public UnlockChestPreparer(UnlockSkinManager manager)
    {
        this.manager = manager;
    }

    public IEnumerator PrepareChest(Chest boughtChest, bool chestIsPaidByAd, Action completed)
    {
        manager.unlockChest.gameObject.SetActive(true);
        manager.unlockChestRenderer.enabled = true;

        SetchestSprites(boughtChest.level);

        manager.StartCoroutine(BackgroundFade(true));

        Vector3 startPosition = manager.GetChestPositionOnScreen(boughtChest);
        yield return manager.StartCoroutine(MoveChestToPayingPosition(startPosition));

        EventManager.ChestArrivesAtPayingPosition.Invoke(boughtChest, chestIsPaidByAd);

        completed();
    }

    public IEnumerator BackgroundFade(bool inOrOut)
    {
        manager.background.gameObject.SetActive(true);
        float time = inOrOut ? manager.info.backgroundFadeTimeIn : manager.info.backgroundFadeTimeOut;

        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / time, 1);

            manager.background.material.color = new Color(0, 0, 0, (inOrOut == true ? t : 1 - t) * manager.info.backgroundTransparency);

            yield return null;
        }

        if (inOrOut == false)
        {
            manager.background.gameObject.SetActive(false);
        }
    }

    public IEnumerator GlowFade()
    {
        float t = 0;
        Color color = manager.glowColors[manager.boughtChest.level - 1];
        Vector3 textStartScale = manager.textNew.localScale;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / manager.info.backgroundFadeTimeOut, 1);

            manager.unlockGlow.color = new Color(color.r, color.g, color.b, 1 - t);
            manager.textNew.localScale = textStartScale * (1 - t);

            yield return null;
        }

        manager.unlockGlow.color = color;
        manager.unlockGlow.gameObject.SetActive(false);
        manager.textNew.localScale = textStartScale;
        manager.textNew.gameObject.SetActive(false);
    }

    private void SetchestSprites(int level)
    {
        manager.unlockChestRenderer.sprite = manager.chestSprites[level - 1];
        manager.unlockChestChargeEffect.sprite = manager.chestChargeEffectSprites[level - 1];
    }

    private IEnumerator MoveChestToPayingPosition(Vector3 startPosition)
    {
        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / manager.info.chestMoveTime, 1);

            manager.unlockChest.position = Vector3.Lerp(startPosition, manager.chestPayingPosition, Mathf.Sin(t * Mathf.PI / 2));

            manager.unlockChest.localScale = manager.chestStartScale * (1 + (manager.info.chestScaleMin * t));

            yield return null;
        }
    }
}
