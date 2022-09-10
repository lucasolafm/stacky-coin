using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockSkinPreviewer
{
    private UnlockSkinManager manager;
    private Vector3 duplicateScreenScale;

    public UnlockSkinPreviewer(UnlockSkinManager manager)
    {
        this.manager = manager;
        duplicateScreenScale = manager.duplicateScreen.localScale;
    }

    public void InstantiatePreviewSkinCoin(Skin skin, bool notDuplicate)
    {
        manager.previewSkinCoin = GameObject.Instantiate(skin);

        manager.previewSkinCoin.name = "Preview Skin Coin";
        manager.previewSkinCoin.transform.rotation = skin.transform.rotation;
        manager.previewSkinCoin.visuals.localPosition = Vector3.zero;
        manager.previewSkinCoin.shadedVisual.gameObject.SetActive(true);
        manager.previewSkinCoin.gameObject.isStatic = false;
        manager.previewSkinCoin.gameObject.SetActive(false);

        manager.unlockGlow.color = manager.glowColors[manager.boughtChest.level - 1];
        manager.unlockGlow.gameObject.SetActive(false);
    }

    public IEnumerator EnlargePreviewSkinCoin(bool isDuplicate)
    {
        Vector3 startScale = manager.previewSkinCoin.transform.localScale;
        Vector3 textStartScale = manager.textNew.localScale;
        Vector3 glowStartScale = manager.unlockGlow.transform.localScale;
        manager.previewSkinCoin.gameObject.SetActive(true);
        if (!isDuplicate)
        {
            manager.unlockGlow.gameObject.SetActive(true);
            manager.textNew.gameObject.SetActive(true);
        }
        manager.previewSkinCoin.transform.position = manager.collectionCamera.ScreenToWorldPoint(new Vector3(Screen.width / 2f, Screen.height / 2f, 10));
        
        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / manager.info.previewCoinEnlargeTime, 1);
            
            manager.previewSkinCoin.transform.localScale = startScale * (Utilities.EaseOutBack(t) * manager.info.previewCoinEnlargeSize);
            if (!isDuplicate)
            {
                manager.unlockGlow.transform.localScale = glowStartScale * Utilities.EaseOutSine(Mathf.Min(t * 1.5f, 1));
                manager.textNew.transform.localScale = textStartScale * Utilities.EaseOutBack(t);
            }

            yield return null;
        }
    }

    public IEnumerator PivotPreviewSkinCoin()
    {
        float progress = 0;
        float prevProgress;
        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / manager.info.previewCoinPivotTime, 1);

            prevProgress = progress;
            progress = 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);

            manager.previewSkinCoin.visuals.Rotate(new Vector3(0, 0, (progress - prevProgress) * manager.info.previewCoinPivotAmount), Space.Self);

            yield return null;
        }
    }

    public IEnumerator DisplayDuplicateScreen(int bonusCoins)
    {
        yield return new WaitForSeconds(manager.info.duplicateScreenDelay);

        manager.duplicateScreen.gameObject.SetActive(true);

        manager.duplicateBonusCoinsText.text = "+" + bonusCoins;

        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / manager.info.duplicateScreenEnlargeTime, 1);

            manager.duplicateScreen.localScale = duplicateScreenScale * Mathf.Sin((t * Mathf.PI) / 2);

            yield return null;
        }

        // Wait for coin to shrink
        yield return new WaitForSeconds(manager.info.previewCoinEnlargeTime + manager.info.previewCoinMoveDelay - 
                                        manager.info.duplicateScreenDelay - manager.info.duplicateScreenEnlargeTime);

        t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / manager.info.previewCoinShrinkTime, 1);

            manager.duplicateScreen.localScale = duplicateScreenScale * (1 - t);

            yield return null;
        }

        manager.duplicateScreen.gameObject.SetActive(false);
    }

    public IEnumerator MovePreviewSkinCoin()
    {
        Vector3 startPos = manager.previewSkinCoin.transform.position;
        Vector3 startScale = manager.previewSkinCoin.transform.localScale;
        Vector3 exitPos =
            manager.collectionUICamera.ScreenToWorldPoint(
                manager.mainCamera.WorldToScreenPoint(manager.collectionButton.position));
        float progress;

        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / manager.info.previewCoinMoveTime, 1);

            progress = Mathf.Sin((t * Mathf.PI) / 2);

            manager.previewSkinCoin.transform.position = Vector3.Lerp(startPos, exitPos, progress);

            manager.previewSkinCoin.transform.localScale = startScale * (1 - t);

            yield return null;
        }

        manager.previewSkinCoin.gameObject.SetActive(false);
    }

    public IEnumerator ShrinkPreviewSkinCoin()
    {
        Vector3 startScale = manager.previewSkinCoin.transform.localScale;

        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / manager.info.previewCoinShrinkTime, 1);

            manager.previewSkinCoin.transform.localScale = startScale * (1 - t);

            yield return null;
        }
    }

    public IEnumerator CollectionButtonBounce()
    {
        Vector3 startScale = manager.collectionButton.localScale;
        float t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / manager.info.collectionButtonBounceTimeOut, 1);
            
            manager.collectionButton.localScale = startScale * (1 + Mathf.Sin((t * Mathf.PI) / 2) * manager.info.collectionButtonBounceSize);
            
            yield return null;
        }

        t = 0;
        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / manager.info.collectionButtonBounceTimeIn, 1);

            manager.collectionButton.localScale = startScale * (1 + (1 - (-(Mathf.Cos(Mathf.PI * t) - 1) / 2)) * manager.info.collectionButtonBounceSize);

            yield return null;
        }
    }

    public void ToggleLights(bool onOrOff, bool isDuplicateSkin)
    {
        if (!isDuplicateSkin)
        {
            manager.collectionLightsFull.SetActive(onOrOff);
        }
        else
        {
            manager.collectionLightsGreyscale.SetActive(onOrOff);
        }
    }

    public void SetGreyscaleMaterial()
    {        
        Renderer renderer = manager.previewSkinCoin.shadedVisual.GetComponent<Renderer>();
        Material[] materials = renderer.materials;

        for (int i = 0; i < renderer.materials.Length; i++)
        {
            materials[i] = manager.duplicateCoinMaterial;
        }

        renderer.materials = materials;
    }
}
