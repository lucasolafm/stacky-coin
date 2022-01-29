using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Info/Chest Charge Up Info")]
public class ChestChargeUpInfo : ScriptableObject
{
    public float enterChestTime;
    public float enterChestTimePerCoin;
    public float payingCoinEndScalePercent;
    public float chestMoveTime;
    public float chestScaleMin, chestScaleMax;
    public float chestExpandTimeMin, chestExpandTimeMax;
    public float chestExpandScaleMin, chestExpandScaleMax;
    public float chestShakeSizeMin, chestShakeSizeMax;
    public float chestPivotLengthMin, chestPivotLengthMax;
    public float chestPivotTimeMin, chestPivotTimeMax;
    public float chestExplodeTime;
    public float chestExplodeScale;
    public float chestFadeOutTime;
    public float backgroundFadeTimeIn;
    public float backgroundFadeTimeOut;
    public float backgroundTransparency;
    public float previewCoinEnlargeTime;
    public float previewCoinEnlargeSize;
    public float previewCoinPivotTime;
    public float previewCoinPivotAmount;
    public float previewCoinMoveTime;
    public float previewCoinShrinkTime;
    public float previewCoinMoveDelay;
    public float collectionButtonBounceTimeOut;
    public float collectionButtonBounceTimeIn;
    public float collectionButtonBounceSize;
    public float duplicateScreenEnlargeTime;
    public float duplicateScreenDelay;
}
