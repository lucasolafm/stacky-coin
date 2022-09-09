
using System.Collections;
using UnityEngine;

public class CameraShaker
{
    private float strength;
    private float speed;
    private float duration;
    private Transform cameraTransform;
    private Quaternion rotation;
    private float maxMultiplier;
    private float time;
    private float t;
    private float shakeMultiplier;
    private float currentStrength;

    public CameraShaker(float strength, float speed, float duration, ref Transform cameraTransform, Quaternion rotation)
    {
        this.strength = strength;
        this.speed = speed;
        this.duration = duration;
        this.cameraTransform = cameraTransform;
        this.rotation = rotation;
    }

    public void AddShakeMultiplier(float value = 1)
    {
        shakeMultiplier += value;
        t = 0;
    }

    public void StabilizeShakeMultiplier()
    {
        t = 0;
    }

    public void Tick()
    {
        time += Time.deltaTime;

        shakeMultiplier *= 1 - Utilities.EaseOutQuad(t); 
        
        t = Mathf.Min(t + Time.deltaTime / duration, 1);

        currentStrength = Mathf.Min(shakeMultiplier, 10) * strength;

        cameraTransform.localPosition = GetShakePosition();
    }
    
    private Vector3 GetShakePosition()
    {
        return rotation * new Vector3((Mathf.PerlinNoise(time * speed, time * speed * 2) - 0.5f) * currentStrength,
            (Mathf.PerlinNoise(time * speed * 2, time * speed) - 0.5f) * currentStrength);
    }
}