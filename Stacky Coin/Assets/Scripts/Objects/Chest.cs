using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Chest : MonoBehaviour
{
	[HideInInspector] public ChestManager chestManager;
	[HideInInspector] public HomeManager homeManager;
	[HideInInspector] public CoinTubeManager coinTubeManager;
	[HideInInspector] public MiniCoinManager miniCoinManager;
	public RectTransform rectTransform;
	public RectTransform pointer;
	public Image sprite, pointerSprite, backgroundSprite;
    public TextMeshProUGUI counterText;
	public PolygonCollider2D polygonCollider;
	public LineRenderer outline;

	public ChestState state;
	[HideInInspector] public int position, price, level;
	[HideInInspector] public int counter;
	[HideInInspector] public Vector3 pointerOriginalPos, pointerOriginalScale, miniChestOriginalScale;

	void Awake()
	{
		pointerOriginalPos = pointer.localPosition;
		pointerOriginalScale = pointer.localScale;
	}

	void Update()
	{
		state.Update();
	}

	public void SetCounter()
	{
		counter = price - homeManager.startOriginalMiniCoins.Length;
		counterText.text = counter.ToString();
	}

	public void SetState(ChestState state)
	{
		if (this.state != null) this.state.Exit();
		this.state = state;
		this.state.Enter();
	}
}
