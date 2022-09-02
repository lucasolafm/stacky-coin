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
        
        chest.SetPointerLocked();

        UpdateCounter();
    }

    public override void OnMiniCoinAddedToTube(CoinType type)
    {
        chest.counter -= type == CoinType.Gem ? GameManager.I.gemBonusAmount : 1;
		UpdateCounter();
    }

    public override void OnMiniCoinRemovedFromTube(CoinType type)
    {
        chest.counter += type == CoinType.Gem ? GameManager.I.gemBonusAmount : 1;
        UpdateCounter();
    }

    private void UpdateCounter()
	{
        chest.pointerLockedText.text = chest.counter.ToString();

		if (chest.counter <= 0)
		{
			chest.SetState(new ChestOpen(chest, true));
		}
	}
}
