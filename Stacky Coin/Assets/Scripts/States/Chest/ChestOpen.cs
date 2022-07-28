using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ChestOpen : ChestState
{
    private bool fromLocked;
    private float hoverTime, hoverProgress, expandTime, expandProgress;
    private bool firstMove = true, upOrDown, inOrOut, doneExpanding;

    public ChestOpen(Chest chest, bool fromLocked = false) : base(chest) 
    {
        this.fromLocked = fromLocked;
    }

    public virtual void SetPointer()
    {
        chest.pointerSprite.sprite = chest.chestManager.pointerSpriteOpen;
        chest.counterText.text = "OPEN";
        chest.counterText.fontStyle = TMPro.FontStyles.Normal;
        chest.counterText.fontSize = 21;
        chest.counterText.color = Color.black;
        chest.counterText.gameObject.SetActive(true);
    }

    public override bool GetIsActive()
    {
        return true;
    }

    public override void Enter()
    {
        base.Enter();

        chest.sprite.sprite = chest.chestManager.spritesOpen[chest.level - 1];
        chest.sprite.color = new Color(chest.sprite.color.r, chest.sprite.color.g, chest.sprite.color.b, 1);
        chest.backgroundSprite.color = chest.chestManager.backgroundColorOpen;
        SetPointer();
    }

    public override void Update()
    {
        // Hovering
        hoverTime += Time.deltaTime / (chest.chestManager.pointerHoverTime * (firstMove ? 0.5f : 1));

        hoverProgress = -(Mathf.Cos(Mathf.PI * Mathf.Min(hoverTime, 1)) - 1) / 2;

        chest.pointer.localPosition = (firstMove ? chest.pointerOriginalPos : chest.pointerOriginalPos + 
                                        new Vector3(0, chest.chestManager.pointerHoverDistance * 0.5f * (upOrDown ? -1 : 1), 0)) + 
                                        new Vector3(0, chest.chestManager.pointerHoverDistance * (firstMove ? 0.5f : 1) * hoverProgress, 0) * (upOrDown ? 1 : -1);

        if (hoverTime >= 1)
        {
            hoverTime -= 1;
            upOrDown = !upOrDown;
            firstMove = false;
        }

        // Expanding
        if (!doneExpanding && fromLocked)
        {
            expandTime += Time.deltaTime / (chest.chestManager.pointerExpandTime * (inOrOut ? 1 : 0.5f));

            expandProgress = -(Mathf.Cos(Mathf.PI * Mathf.Min(expandTime, 1)) - 1) / 2;

            chest.pointer.localScale = chest.pointerOriginalScale * (1 + (inOrOut ? 1 - expandProgress : expandProgress) * chest.chestManager.pointerExpandSize);

            if (expandTime >= 1)
            {
                if (inOrOut)
                {
                    doneExpanding = true;
                }

                expandTime -= 1;
                inOrOut = true;
            }
        }
    }

    public override void OnMiniCoinAddedToTube()
    {
        chest.counter--;
    }

    public override void OnMiniCoinRemovedFromTube()
    {
        chest.counter++;
        if (chest.counter > 0)
        {
            chest.SetState(new ChestLocked(chest));
        }
    }

    public override void PressChest()
    {
        if (!chest.homeManager.State.CanBuyChest()) return;

        StartOpeningProcess();
    }

    public override void StartOpeningProcess()
    {
        EventManager.BuysChest.Invoke(chest, false);

        Data.SetChest(0, chest.position);

        chest.SetState(new ChestInactive(chest));  
    }
}
