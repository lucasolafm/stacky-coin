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

	public float pointerBounceValue;
	public float pointerBounceTime;

	private bool bouncingPointer;

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

	public IEnumerator BounceCounterPointer()
	{
		if (bouncingPointer) yield break;
		bouncingPointer = true;

		Vector3 startPos = pointer.transform.position;
		for (int i = 0; i < 3; i++)
		{
			Vector3 endPos = startPos + new Vector3(0, pointerBounceValue / 3 * (3 - i));
			float t = 0;
			while (t < 1)
			{
				t = Mathf.Min(t += Time.deltaTime / pointerBounceTime, 1);

				pointer.transform.position = startPos + (endPos - startPos) *
				                             (t < 0.5f
					                             ? Utilities.EaseOutSine(t * 2)
					                             : 1 - Utilities.EaseInSine((t - 0.5f) * 2));

				yield return null;
			}
		}
		

		bouncingPointer = false;
	}
}
