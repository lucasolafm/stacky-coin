using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChestLocked : ChestState
{
    public ChestLocked(Chest chest) : base(chest) {}

    public override bool GetIsActive()
    {
        return true;
    }

    public override void Enter()
    {
        base.Enter();

        chest.sprite.sprite = chest.chestManager.spritesLocked[chest.level - 1];
        chest.sprite.color = new Color(chest.sprite.color.r, chest.sprite.color.g, chest.sprite.color.b, chest.chestManager.lockedTransparency);
        chest.pointerSprite.sprite = chest.chestManager.pointerSpriteLocked;
        chest.pointer.localPosition = chest.pointerOriginalPos;
        chest.backgroundSprite.color = chest.chestManager.backgroundColorLocked;

        chest.counterText.gameObject.SetActive(true);
        chest.counterText.font = chest.counterShaderLocked;
        chest.counterText.fontSize = 37;

        UpdateCounter();
    }

    public override void OnMiniCoinAddedToTube()
    {
        chest.counter--;
		UpdateCounter();
    }

    public override void OnMiniCoinRemovedFromTube()
    {
        chest.counter++;
        UpdateCounter();
    }

    private void UpdateCounter()
	{
        chest.counterText.text = chest.counter.ToString();

		if (chest.counter <= 0)
		{
			chest.SetState(new ChestOpen(chest, true));
		}
	}
}
