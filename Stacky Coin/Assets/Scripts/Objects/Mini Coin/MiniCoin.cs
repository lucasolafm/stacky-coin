using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniCoin : MonoBehaviour
{
    [SerializeField] private CoinType type;
    public new GameObject gameObject;
    public new Transform transform;
    public new Renderer renderer;
    public MeshFilter meshFilter;
    [HideInInspector] public ParticleSystem[] bonusCoinEffects;

    [HideInInspector] public HomeManager homeManager;
    [HideInInspector] public MiniCoinManager miniCoinManager;
    [HideInInspector] public UnlockSkinManager unlockSkinManager;
    [HideInInspector] public MiniCoinState State;
    [HideInInspector] public int indexInList;

    void Update()
    {
        State.Update();
    }

    void OnBecameVisible()
    {
        State.OnBecameVisible();
    }

    public CoinType GetCoinType() => type;

    public void SetCoinType(CoinType newType) => type = newType;

    public virtual int GetId() { return 0; }

    public virtual void Land() {}

    public void SetState(MiniCoinState state)
    {
        State?.Exit();
        State = state;
        State.Enter();
    }
}
