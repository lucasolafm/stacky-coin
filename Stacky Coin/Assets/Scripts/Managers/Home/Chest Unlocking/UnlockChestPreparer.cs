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

    public IEnumerator PrepareChest(Chest boughtChest, bool chestIsPaidByAd)
    {
        manager.unlockChest.gameObject.SetActive(true);
        manager.unlockChestRenderer.enabled = true;

        SetchestSprites(boughtChest.level);

        manager.StartCoroutine(BackgroundFade(true));

        yield return manager.StartCoroutine(MoveChestToPayingPosition(boughtChest.transform.position));

        EventManager.ChestArrivesAtPayingPosition.Invoke(boughtChest, chestIsPaidByAd);
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
