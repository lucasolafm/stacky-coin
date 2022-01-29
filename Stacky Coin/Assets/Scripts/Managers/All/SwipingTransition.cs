using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwipingTransition : MonoBehaviour
{
    [SerializeField] private float minSwipeDistance;

    private bool isDragging;
    private bool hasSwiped;
    private float lastPressPositionX;
    private float swipe;

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (hasSwiped) return;

            if (!isDragging) 
            {
                isDragging = true;
                lastPressPositionX = Input.mousePosition.x;
                return;
            }

            swipe = Input.mousePosition.x - lastPressPositionX;

            if (swipe > minSwipeDistance || swipe < -minSwipeDistance)
            {
                EventManager.SwipesScreen.Invoke(swipe > 0);
                hasSwiped = true;
            }

            lastPressPositionX = Input.mousePosition.x;

            isDragging = true;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
            hasSwiped = false;
        }
    }
}
