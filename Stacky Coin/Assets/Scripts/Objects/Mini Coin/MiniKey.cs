using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MiniKey : MiniCoin
{
    public int level;
    [SerializeField] private PolygonCollider2D polygonCollider;
    [SerializeField] private LineRenderer[] outlines;
    [SerializeField] private Material glowMaterial;
    
    private Vector3 startPositon, startScale;
    private Vector3[] outlinePositions;
    private int chestPosition, chestPrice;
    private WaitForSeconds outlineDelayWait;

    void Start()
    {
        startScale = transform.localScale;
        outlineDelayWait = new WaitForSeconds(miniCoinManager.keyOutlineDelay);

        outlinePositions = new Vector3[polygonCollider.points.Length];
        for (int i = 0; i < outlinePositions.Length; i++)
        {
            outlinePositions[i] = polygonCollider.points[i];
        }

        foreach (LineRenderer outline in outlines)
        {
            outline.positionCount = outlinePositions.Length;
            outline.SetPositions(outlinePositions); 
        }

        // Get chest values and save to data
        chestPosition = miniCoinManager.chestManager.GetFirstAvaialbleChestSlot();
        chestPrice = miniCoinManager.chestManager.GetRandomChestPrice(level);
        Data.SetChest(chestPrice, chestPosition);

        miniCoinManager.chestManager.AddNewChest(level, chestPrice, chestPosition);
    }

    public override CoinType GetCoinType()
    {
        return CoinType.Key;
    }

    public override int GetId()
    {
        return level;
    }

    public override void Land()
    {
        base.Land();
        
        GameManager.I.audioSource.PlayOneShot(homeManager.tubeKeyDropClip);

        SetState(new MiniCoinInTube(this));

        transform.position = miniCoinManager.keyCamera.transform.position + (transform.position - miniCoinManager.coinTubeManager.camera.transform.position);

        gameObject.layer = 24;
        foreach (LineRenderer outline in outlines)
        {
            outline.gameObject.layer = 24;
        }

        startPositon = transform.position;
        renderer.material = glowMaterial;

        StartCoroutine(ExpandAnimation());
        StartCoroutine(HoverAnimation());
        StartCoroutine(OutlineAnimations());
    }

    private IEnumerator ExpandAnimation()
    {
        float t = 0;
        float progress;

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / miniCoinManager.keyExpandTime, 1);

            progress = 1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2);

            transform.localScale = startScale * miniCoinManager.keyExpandStartSize * (1 + miniCoinManager.keyExpandSize * progress);

            yield return null;
        }
    }

    private IEnumerator HoverAnimation()
    {
        float t = 0;
        float progress;

        for (int i = 0; i < 3; i++)
        {
            while (t < 1)
            {
                t += Time.deltaTime / (miniCoinManager.keyHoverTime * (i == 1 ? 1 : 0.5f));

                progress = i == 0 ? Mathf.Sin((Mathf.Min(t, 1) * Mathf.PI) / 2) : 
                            i == 1 ? -(Mathf.Cos(Mathf.PI * Mathf.Min(t, 1)) - 1) / 2 : 
                            1 - Mathf.Cos((Mathf.Min(t, 1) * Mathf.PI) / 2);

                transform.position = startPositon + new Vector3(0, miniCoinManager.keyHoverLength, 0) * (i == 0 ? 0 : 0.5f) * (i == 1 ? -1 : 1) +
                                        new Vector3(0, miniCoinManager.keyHoverLength, 0) * (i == 1 ? 1 : 0.5f) * (i == 1 ? 1 : -1) * progress;

                yield return null;
            }

            t -= 1;
        }

        StartCoroutine(MoveToChestSlot());
    }

    private IEnumerator OutlineAnimations()
    {
        for (int i = 0; i < outlines.Length; i++)
        {
            outlines[i].enabled = true;
            StartCoroutine(ExpandOutline(i));

            yield return outlineDelayWait;
        }
    }

    private IEnumerator ExpandOutline(int index)
    {
        float t = 0;
        float progress;

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / (miniCoinManager.keyOutlineOutTime * (index + 1 == outlines.Length ? 1.2f : 1)), 1);

            progress = 1 - Mathf.Pow(1 - t, 3);

            outlines[index].startWidth = miniCoinManager.keyOutlineSize * progress;
            outlines[index].endWidth = miniCoinManager.keyOutlineSize * progress;

            outlines[index].material.color = new Color(outlines[index].material.color.r, outlines[index].material.color.g, outlines[index].material.color.b, 1 - t);

            yield return null;
        }
    }

    private IEnumerator MoveToChestSlot()
    {
        float t = 0;
        float progress;
        Vector3 target = miniCoinManager.chestManager.chests[chestPosition].rectTransform.TransformPoint(
                            miniCoinManager.chestManager.chests[chestPosition].rectTransform.position);

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / miniCoinManager.keyMoveToSlotTime, 1);

            progress = Mathf.Sin((t * Mathf.PI) / 2);

            transform.position = Vector3.Lerp(startPositon, target, progress);

            transform.localScale = startScale * (1 - t * t * t * t * t * 0.7f);

            yield return null;
        }
        
        GameManager.I.audioSource.PlayOneShot(homeManager.tubeGemBonusClip, 0.4f);

        // Enable the chest
        miniCoinManager.chestManager.EnableChest(chestPosition);
        gameObject.SetActive(false);
    }
}
