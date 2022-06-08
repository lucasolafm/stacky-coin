using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utilities
{
    public static string coinTag = "Coin", handTag = "Hand", floorTag = "Floor", fallOffAreaTag = "FallOffArea", perfectHitTag = "PerfectHit";

    public static float EaseInSine(float x)
    {
        return x * x;
    }
    
    public static float EaseOutSine(float x)
    {
        return Mathf.Sin(x * Mathf.PI / 2);
    }

    public static float EaseInOutSine(float x)
    {
        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
    }
    
    public static float EaseOutQuad(float x)
    {
        return (1 - (1 - x) * (1 - x));
    }

    public static float EaseInOutQuad(float x)
    {
        return (x < 0.5 ? 2 * x * x : 1 - Mathf.Pow(-2 * x + 2, 2) / 2);
    }

    public static float EaseOutBack(float x)
    {
        return 1 + 2.70158f * Mathf.Pow(x - 1, 3) + 1.70158f * Mathf.Pow(x - 1, 2);
    }
}
