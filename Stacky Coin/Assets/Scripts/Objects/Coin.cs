using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using MEC;

public class Coin : MonoBehaviour
{
    public new GameObject gameObject;
    public new Transform transform;
    public Rigidbody rb;
    public new Renderer renderer;
	public new BoxCollider collider;
    public TrailRenderer trail;
    public PerfectHit perfectHit;
    public int number;
    public int maxAngularVelocity;
    private float touchSoundPitch;

    public CoinManager coinManager;
    public CoinPileManager coinPileManager;

    public CoinType type;
    public CoinState State;
    public bool isStartingStack;

    void Start()
    {
        rb.maxAngularVelocity = maxAngularVelocity;
    }

    void Update()
    {
        State.Update();
    }

    void LateUpdate()
    {
        State.LateUpdate();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag(Utilities.handTag))
        {
            State.OnCollideWithHand();
        }
        else if (collision.gameObject.CompareTag(Utilities.coinTag))
        {
            State.OnCollideWithCoin(collision);
        }
        else if (collision.gameObject.CompareTag(Utilities.floorTag))
        {
            State.OnCollideWithCoin(collision);

            EventManager.CoinLandsOnFloor.Invoke(this);
        }
    }

	void OnTriggerEnter(Collider collider)
	{
        if (collider.CompareTag(Utilities.fallOffAreaTag))
        {
            State.OnCollideWithFallOffZone();
        }
	}

    public virtual int GetId() { return 0; }

    public virtual void ToggleTrail() 
    {
        trail.enabled = !trail.enabled;
    }

    public void SetState(CoinState state)
    {
        if (State != null) State.Exit();
        State = state;
        State.Enter();
    }
}
