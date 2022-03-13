using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public static class EventManager 
{
	public class BoolEvent : UnityEvent<bool>{}
	public class IntEvent : UnityEvent<int>{}
	public class FloatEvent : UnityEvent<float>{}
	public class CoinEvent : UnityEvent<Coin>{}
	public class CoinIntEvent : UnityEvent<Coin, int>{}
	public class CoinFloatEvent : UnityEvent<Coin, float>{}
	public class MiniCoinIntFloatEvent : UnityEvent<MiniCoin, int, float>{}
	public class ChestEvent : UnityEvent<Chest>{}
	public class ChestBoolEvent : UnityEvent<Chest, bool>{}

	// All
	public static FloatEvent TransitioningScenes = new FloatEvent();
	public static UnityEvent LoadingScreenSlidOut = new UnityEvent();
	public static UnityEvent FirstTimePlaying = new UnityEvent();
	public static UnityEvent FirstTimeInHome = new UnityEvent();

	// Play
	public static CoinEvent CoinSpawns = new CoinEvent();
	public static CoinEvent CoinDespawns = new CoinEvent();
	public static CoinEvent CoinLandsOnHand = new CoinEvent();
	public static UnityEvent<Coin> CoinLandsOnFloor = new UnityEvent<Coin>();
	public static CoinEvent HandCharges = new CoinEvent();
	public static UnityEvent HandStopsCharge = new UnityEvent();
	public static CoinFloatEvent CoinFlipping = new CoinFloatEvent();
	public static CoinFloatEvent CoinFlips = new CoinFloatEvent();
	public static CoinFloatEvent CoinCollides = new CoinFloatEvent();
	public static CoinEvent CoinTouchesPile = new CoinEvent();
	public static CoinEvent CoinScores = new CoinEvent();
	public static CoinEvent CoinFalls = new CoinEvent();
	public static UnityEvent CoinFallsWhileTouchingPile = new UnityEvent();
	public static UnityEvent<Coin[]> CoinsFallOffPile = new UnityEvent<Coin[]>();
	public static UnityEvent CoinPileFallsOver = new UnityEvent();
	public static UnityEvent GoingGameOver = new UnityEvent();
	public static BoolEvent GoneGameOver = new BoolEvent();
	public static IntEvent ScoreChanges = new IntEvent();
	public static CoinIntEvent PerfectHit = new CoinIntEvent();
	public static CoinFloatEvent ReachesNextStageTarget = new CoinFloatEvent();
	public static UnityEvent HandAscendedCoinPile = new UnityEvent();
	public static UnityEvent StageInitialized = new UnityEvent();

	// Home
	public static UnityEvent LoadedHomeScene = new UnityEvent();
	public static UnityEvent EnterDefaultHome = new UnityEvent();
	public static UnityEvent NewHighScore = new UnityEvent();
	public static UnityEvent PlayingAgain = new UnityEvent();
	public static UnityEvent SpawningNewMiniCoins = new UnityEvent();
	public static UnityEvent MiniCoinAddedToTube = new UnityEvent();
	public static MiniCoinIntFloatEvent MiniCoinRemovedFromTube = new MiniCoinIntFloatEvent();
	public static UnityEvent LastCoinOnScreenPaid = new UnityEvent();
	public static UnityEvent CoinTubeCameraRepositioned = new UnityEvent();
	public static UnityEvent EntersCollection = new UnityEvent();
	public static UnityEvent EnteringCollection = new UnityEvent();
	public static UnityEvent EnteredCollection = new UnityEvent();
	public static UnityEvent EntersHome = new UnityEvent();
	public static UnityEvent EnteringHome = new UnityEvent();
	public static UnityEvent EnteredHome = new UnityEvent();
	public static ChestBoolEvent BuysChest = new ChestBoolEvent();
	public static ChestEvent BuysChestWithAd = new ChestEvent();
	public static UnityEvent MiniCoinEntersChest = new UnityEvent();
	public static ChestBoolEvent ChestArrivesAtPayingPosition = new ChestBoolEvent();
	public static IntEvent UnlockedNewCoinSkin = new IntEvent();
	public static UnityEvent SkinPreviewEntersCollectionButton = new UnityEvent();
	public static ChestEvent EnabledNewChest = new ChestEvent();
	public static ChestEvent PaidForChest = new ChestEvent();
	public static UnityEvent PressedBonusCoinsButton = new UnityEvent();
	public static BoolEvent SwipesScreen = new BoolEvent();
}
