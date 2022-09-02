using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Chest : MonoBehaviour
{
	public int id;
	[SerializeField] private GameObject pointerLocked;
	public GameObject pointerOpen;
	[SerializeField] private GameObject pointerAd;
	public TextMeshPro pointerLockedText;
	
	[HideInInspector] public ChestManager chestManager;
	[HideInInspector] public HomeManager homeManager;
	public SpriteRenderer sprite;

    public ChestState state;
	[HideInInspector] public int position, price, level;
	[HideInInspector] public int counter;
	[HideInInspector] public Vector3 pointerOpenOriginalPos, pointerOpenOriginalScale;

	void Awake()
	{
		pointerOpenOriginalPos = pointerOpen.transform.localPosition;
		pointerOpenOriginalScale = pointerOpen.transform.localScale;
	}

	void Update()
	{
		state.Update();
	}

	public void SetCounter()
	{
		counter = price - homeManager.startOriginalMiniCoins.Length;
		pointerLockedText.text = counter.ToString();
	}

	public void SetState(ChestState state)
	{
		if (this.state != null) this.state.Exit();
		this.state = state;
		this.state.Enter();
	}

	public void SetPointerLocked()
	{
		pointerLocked.SetActive(true);
		pointerOpen.SetActive(false);
	}

	public void SetPointerOpen()
	{
		pointerLocked.SetActive(false);
		pointerOpen.SetActive(true);
	}

	public void SetPointerAd()
	{
		pointerLocked.SetActive(false);
		pointerAd.SetActive(true);
	}
}
