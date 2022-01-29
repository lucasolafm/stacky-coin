using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGem : MiniCoin
{
    void Start()
    {
        InstantiateBonusCoinEffects();
    }

    public override CoinType GetCoinType()
    {
        return CoinType.Gem;
    }
    
    public override void Land()
    {
        base.Land();

        EventManager.MiniCoinAddedToTube.Invoke();

        SetState(new MiniCoinPayingBonus(this));
    }

    private void InstantiateBonusCoinEffects()
    {
        bonusCoinEffects = new ParticleSystem[GameManager.I.gemBonusAmount];
        for (int i = 0; i < GameManager.I.gemBonusAmount; i++)
        {
            bonusCoinEffects[i] = Instantiate(homeManager.effectGemBonusCoinPrefab);
            bonusCoinEffects[i].transform.parent = transform;
        }
    }
}
