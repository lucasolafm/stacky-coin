using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

public class ButtonPresser : MonoBehaviour
{
    [SerializeField] private HomeManager homeManager;
    [SerializeField] private AdBonusCoins adBonusCoins;
    [SerializeField] private ChestManager chestManager;
    [SerializeField] private Camera camera;

    void Update()
    {
        if (!Input.GetMouseButtonDown(0) || AdPlayer.IsLoadingAd) return;
        
        Ray ray = camera.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hitInfo)) return;
        
        if (hitInfo.transform.CompareTag("Bonus Button"))
        {
            homeManager.PressBonusCoinsButton();
        }
        else if (hitInfo.transform.CompareTag("Ad Bonus Button"))
        {
            adBonusCoins.PressBonusCoinsButton();
        }
        else if (hitInfo.transform.CompareTag("Chest"))
        {
            chestManager.PressChest(hitInfo.transform.GetComponent<Chest>().id);
        }
        else if (hitInfo.transform.CompareTag("Play Again"))
        {
            homeManager.PressPlayAgainButton();
        }
    }
}
