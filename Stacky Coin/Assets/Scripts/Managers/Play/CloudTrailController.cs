using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloudTrailController : MonoBehaviour
{
    [SerializeField] private CoinManager coinManager;

    [SerializeField] private Transform[] particles;
    [SerializeField] private Sprite[] sprites;
    [SerializeField] private float distanceMax, distanceMin;
    [SerializeField] private float positionRandomness;
    [SerializeField] private float sizeByChargeTimeMin, sizeByChargeTimeMax, sizeMin;
    [SerializeField] private float moveSpeedByOrder, moveSpeedRandomness;
    [SerializeField] private float moveSpeedByChargeTimeMin, moveSpeedByChargeTimeMax;
    [SerializeField] private float dragFlat, dragByOrder;
    [SerializeField] private float shrinkSpeedFlat, shrinkSpeedPercent, shrinkSpeedRandomness;
    [SerializeField] private float spriteChangeTimeMin, spriteChangeTimeMax, spriteChangeOverTime;

    private int particlesLength, spritesLength;
    private SpriteRenderer[] particleRenderers;
    private List<Vector3> waypoints = new List<Vector3>();
    private float particleDistance; 
    private int waypointsCount;
    private float waypointDistance;
    private float totalDistance;
    private int spawnCount;
    private Vector3 direction;
    private Coroutine[] lifetimeRoutines;
    private WaitForFixedUpdate waitForFixedUpdate = new WaitForFixedUpdate();
    private int[] legalSprites = new int[3];
    private float size;

    void Start()
    {
        EventManager.CoinFlipping.AddListener(OnCoinFlipping);

        particlesLength = particles.Length;
        spritesLength = sprites.Length;

        particleRenderers = new SpriteRenderer[particlesLength];
        for (int i = 0; i < particleRenderers.Length; i++)
        {
            particleRenderers[i] = particles[i].GetComponent<SpriteRenderer>();
        }

        lifetimeRoutines = new Coroutine[particlesLength];
    }

    private void OnCoinFlipping(Coin coin, float chargeTime)
    {
        StartCoroutine(EmitParticles(coin.transform, chargeTime));
    }

    private IEnumerator EmitParticles(Transform coin, float chargeTime)
    {
        waypoints.Clear();
        totalDistance = 0;

        waypoints.Add(coin.position);
        waypointsCount = 1;

        // Spawn first particle that won't move
        SpawnParticle(0, waypoints[0], GetParticleScale(0, chargeTime), Vector3.zero, 0, GetParticleShrinkSpeed(), Random.Range(0, spritesLength));
        spawnCount = 1; 

        while (spawnCount < particlesLength)
        {
            yield return waitForFixedUpdate;
            
            if (ScreenshotTool.IsPaused) continue;

            waypoints.Add(coin.position);

            GetWaypointDistance(waypoints[waypointsCount]);
            totalDistance += waypointDistance;

            GetParticleDistance();
            GetParticleDirection();
            
            // Spawn as many particles as possible on this frame
            while (totalDistance >= particleDistance && spawnCount < particlesLength)
            {
                SpawnParticle(spawnCount, GetParticlePosition(), GetParticleScale(spawnCount, chargeTime), direction, 
                                GetParticleMoveSpeed(chargeTime), GetParticleShrinkSpeed(), Random.Range(0, spritesLength));
                spawnCount++;

                totalDistance -= particleDistance;
            }

            waypointsCount++;   
        }
    }

    private void SpawnParticle(int index, Vector3 position, Vector3 scale, Vector3 direction, float moveSpeed, float shrinkSpeed, int sprite)
    {
        particleRenderers[index].sprite = sprites[sprite];

        particles[index].position = position;

        particles[index].localScale = scale;

        if (lifetimeRoutines[index] != null) StopCoroutine(lifetimeRoutines[index]);
        lifetimeRoutines[index] = StartCoroutine(ParticleLifetime(index, direction, moveSpeed, shrinkSpeed, sprite));
    }

    private IEnumerator ParticleLifetime(int index, Vector3 direction, float moveSpeed, float shrinkSpeed, int sprite)
    {
        float timeAlive = 0;
        float spriteChangeStartTime = Time.time;
        float spriteChangeTime = GetSpriteChangeTime(timeAlive);
        int currentSprite = sprite;

        while (true)
        {
            yield return null;
            
            if (ScreenshotTool.IsPaused) continue;

            timeAlive += Time.deltaTime;

            particles[index].position += GetParticleMove(index, direction, timeAlive, moveSpeed);

            particles[index].localScale = GetParticleShrinkScale(index, shrinkSpeed, timeAlive);

            // Change the sprite over time
            if (Time.time - spriteChangeStartTime >= spriteChangeTime)
            {
                GetLegalSprites(currentSprite);
                particleRenderers[index].sprite = sprites[legalSprites[Random.Range(0, spritesLength - 1)]];

                spriteChangeStartTime = Time.time;
                spriteChangeTime = GetSpriteChangeTime(timeAlive);
            }
        }   
    }

    private void GetWaypointDistance(Vector3 coinPosition)
    {
        waypointDistance = (coinPosition - waypoints[waypointsCount - 1]).magnitude;
    }

    private void GetParticleDistance()
    {
        particleDistance = Mathf.Max(distanceMax * (1 - ((float)spawnCount / particlesLength)), distanceMin);
    }

    private void GetParticleDirection()
    {
        direction = (waypoints[waypointsCount] - waypoints[waypointsCount - 1]).normalized;;
    }

    private Vector3 GetParticlePosition()
    {
        return waypoints[waypointsCount - 1] + direction * (particleDistance - (totalDistance - waypointDistance)) +
                Random.insideUnitSphere * positionRandomness;
    }

    private Vector3 GetParticleScale(int index, float chargeTime)
    {
        return new Vector3(1, 1, 1) * (sizeByChargeTimeMin + chargeTime / coinManager.maxChargeTimeLight * (sizeByChargeTimeMax - sizeByChargeTimeMin)) * 
                Mathf.Max((1 - ((float)index / particlesLength)), sizeMin);
    }

    private float GetParticleMoveSpeed(float chargeTime)
    {
        return (moveSpeedByChargeTimeMin + chargeTime / coinManager.maxChargeTimeLight * (moveSpeedByChargeTimeMax - moveSpeedByChargeTimeMin)) * 
                (1 + Random.Range(-moveSpeedRandomness, moveSpeedRandomness));
    }

    private float GetParticleShrinkSpeed()
    {
        return shrinkSpeedFlat * (1 + Random.Range(-shrinkSpeedRandomness, shrinkSpeedRandomness));
    }

    private Vector3 GetParticleMove(int index, Vector3 direction, float timeAlive, float speed)
    {
        return (moveSpeedByOrder * Mathf.Max(index - 1, 0) + speed) * 
                Mathf.Max(1 - (dragFlat + dragByOrder * (particlesLength - index)) * timeAlive, 0) * 
                Time.deltaTime * direction;
    }

    private Vector3 GetParticleShrinkScale(int index, float speed, float timeAlive)
    {
        return new Vector3(Mathf.Max(particles[index].localScale.x * (1 - shrinkSpeedPercent) - speed, 0), 
                            Mathf.Max(particles[index].localScale.y * (1 - shrinkSpeedPercent) - speed, 0), 1);
    }

    private void GetLegalSprites(int currentSprite)
    {
        for (int i = 0; i < spritesLength - 1; i++)
        {
            legalSprites[i] = currentSprite + i + 1 < spritesLength ? currentSprite + i + 1 : currentSprite + i + 1 - spritesLength;   
        }
    }

    private float GetSpriteChangeTime(float timeAlive)
    {
        return Random.Range(spriteChangeTimeMin, spriteChangeTimeMax) - spriteChangeOverTime * timeAlive;
    }
}
