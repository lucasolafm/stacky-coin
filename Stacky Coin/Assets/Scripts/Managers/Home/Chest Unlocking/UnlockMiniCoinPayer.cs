using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnlockMiniCoinPayer
{
    private UnlockSkinManager manager;
    
    private MiniCoin payingCoin;

    public UnlockMiniCoinPayer(UnlockSkinManager manager)
    {
        this.manager = manager;
    }

    public void CoinEnteringChest(MiniCoin miniCoin, int paidCount, float percentageOfPaidThisFrame)
    {
        payingCoin = GetAvailableMiniCoin();

        payingCoin.SetCoinType(miniCoin.GetCoinType());
        payingCoin.renderer.material = miniCoin.renderer.material;

        payingCoin.SetState(new MiniCoinEnteringChest(payingCoin, miniCoin.miniCoinManager.keyCamera.transform.position + 
                                                        (miniCoin.transform.position - miniCoin.miniCoinManager.coinTubeManager.camera.transform.position),
                                                        new Vector3(manager.chestPayingPosition.x, manager.chestPayingPosition.y, 6), percentageOfPaidThisFrame * Time.deltaTime, 
                                                        manager.info.enterChestTime - manager.info.enterChestTimePerCoin * paidCount, 
                                                        manager.payingCoinStartScale, manager.payingCoinEndScale));
    }

    private MiniCoin GetAvailableMiniCoin()
    {
        foreach (MiniCoin payingCoin in manager.payingCoins)
        {
            if (payingCoin.State.GetIsActive()) continue;

            return payingCoin;
        }

        Debug.Log("Not enough paying coins");
        return null;
    }
}
