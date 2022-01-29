﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChestManager : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    [SerializeField] private CoinTubeManager coinTubeManager;
    [SerializeField] private MiniCoinManager miniCoinManager;
    public Chest[] chests;
    [SerializeField] private Button[] buttons;
    [SerializeField] private new Camera camera;

    public Sprite[] spritesLocked, spritesOpen;
    public Material[] miniChestMaterials;
    public Sprite pointerSpriteLocked, pointerSpriteOpen, pointerSpriteAd;
    public Color backgroundColorLocked, backgroundColorOpen;
    public float lockedTransparency;
    public int priceRange;
    public int[] priceMinimums;
    public float pointerHoverTime, pointerHoverDistance, pointerExpandTime, pointerExpandSize;
    [SerializeField] private float spawnTime, spawnSize;
    [SerializeField] private float outlineTime, outlineSize;

    private int[] chestsInData;
    private Vector3 spriteStartScale, pointerStartScale;
    private Vector3[] outlinePositions;

    public void Initialize()
    {
        EventManager.MiniCoinAddedToTube.AddListener(OnMiniCoinAddedToTube);
        EventManager.MiniCoinRemovedFromTube.AddListener(OnMiniCoinRemovedFromTube);

        spriteStartScale = chests[0].sprite.transform.localScale;
        pointerStartScale = chests[0].pointer.transform.localScale;

        SetChestOutlines();

        // Add chests from data
        chestsInData = Data.chests;
        for (int i = 0; i < chests.Length; i++)
        {
            int position = i;
            buttons[i].onClick.AddListener(() => PressChest(position));

            PrepareChest(chests[i], GetChestLevel(chestsInData[i]), chestsInData[i], i);

            if (chestsInData[i] == 0)
            {
                chests[i].SetState(new ChestInactive(chests[i]));   
            }
            else
            {
                chests[i].PlaceMiniChest();

                if (chests[i].counter > 0)
                {
                    chests[i].SetState(new ChestLocked(chests[i]));                    
                }
                else
                {
                    chests[i].SetState(new ChestOpen(chests[i], false));
                }
            }
        }

        //AddNewChest(3, 250, 0);
        //EnableChest(0);
        //AddNewChest(2, 150, 1);
        //EnableChest(1);
        //AddNewChest(1, 50, 2);
        //EnableChest(2);
    }

    private void PressChest(int position)
    {
        chests[position].state.PressChest();
    }

    private void OnMiniCoinAddedToTube()
    {
        foreach (Chest chest in chests)
        {
            chest.state.OnMiniCoinAddedToTube();
        }
    }

    private void OnMiniCoinRemovedFromTube(MiniCoin miniCoin, int paidCount, float percentageOfPaidThisFrame)
    {
        foreach (Chest chest in chests)
        {
            chest.state.OnMiniCoinRemovedFromTube();
        }
    }

    private void SetChestOutlines()
    {
        foreach (Chest chest in chests)
        {
            outlinePositions = new Vector3[chest.polygonCollider.points.Length];
            for (int i = 0; i < outlinePositions.Length; i++)
            {
                outlinePositions[i] = chest.polygonCollider.points[i];
            }

            chest.outline.positionCount = outlinePositions.Length;
            chest.outline.SetPositions(outlinePositions); 
        }
    }

    public void AddNewChest(int level, int price, int position)
    {
        PrepareChest(chests[position], level, price, position);
        
        chests[position].SetState(new ChestLocked(chests[position]));
    }

    public void EnableChest(int chestPosition)
    {
        chests[chestPosition].gameObject.SetActive(true);

        StartCoroutine(ChestSpawnAnimation(chestPosition));

        chests[chestPosition].PlaceMiniChest();

        EventManager.EnabledNewChest.Invoke(chests[chestPosition]);
    }

    private void PrepareChest(Chest chest, int level, int price, int position)
    {
        chest.price = price;
        chest.level = level;
        chest.position = position;
        chest.chestManager = this;
        chest.homeManager = homeManager;
        chest.coinTubeManager = coinTubeManager;
        chest.miniCoinManager = miniCoinManager;
        chest.SetCounter();
    }

    private IEnumerator ChestSpawnAnimation(int index)
    {
        float t = 0;
        float progress;

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / spawnTime, 1);

            progress = 1 - (1 + 2.70158f * Mathf.Pow(t - 1, 3) + 1.70158f * Mathf.Pow(t - 1, 2));

            chests[index].sprite.transform.localScale = spriteStartScale * (1 + progress * spawnSize);
            chests[index].pointer.transform.localScale = pointerStartScale * (1 + progress * spawnSize);

            yield return null;
        }
    }

    private IEnumerator ExpandOutline(int index)
    {
        chests[index].outline.enabled = true;
        float t = 0;
        float progress;

        while (t < 1)
        {
            t = Mathf.Min(t + Time.deltaTime / outlineTime, 1);

            progress = 1 - Mathf.Pow(1 - t, 3);

            chests[index].outline.startWidth = outlineSize * progress;
            chests[index].outline.endWidth = outlineSize * progress;

            chests[index].outline.material.color = new Color(
                chests[index].outline.material.color.r, chests[index].outline.material.color.g, chests[index].outline.material.color.b, 1 - t);

            yield return null;
        }

        chests[index].outline.enabled = false;
    }

    public int GetChestLevel(int price)
    {
        if (price <= priceMinimums[0] + priceRange)
        {
            return 1;
        }
        else if (price <= priceMinimums[1] + priceRange)
        {
            return 2;
        }
        else
        {   
            return 3;
        }
    }

    public int GetRandomChestPrice(int level)
    {
        return priceMinimums[level - 1] + Random.Range(0, priceRange / 10 + 1) * 10;
    }

    public int GetFirstAvaialbleChestSlot()
    {
        chestsInData = Data.chests;

        for (int i = 0; i < chestsInData.Length; i++)
        {
            if (chestsInData[i] == 0)
            {
                return i;
            }
        }

        return 0;
    }
}