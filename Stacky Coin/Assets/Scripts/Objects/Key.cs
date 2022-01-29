using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Key : Coin 
{
    public new Rigidbody rigidbody;
	public TrailRenderer gemTrail;

    public int level;
    public Material gold;
    public Material gemMatLvl1;
    public Material gemMatLvl2;
    public Material gemMatLvl3;
    public List<Material> gemTrailMats = new List<Material>();
    public int chestPlace;
    public int chestCoinPosition;
    public bool cannotFall;

    public override int GetId()
    {
        return level;
    }

    public override void ToggleTrail()
    {
        base.ToggleTrail();

        gemTrail.enabled = !gemTrail.enabled;
    }

    public void SetLevel(float lvl1Chance, float lvl2Chance, float lvl3Chance)
    {
        float rng = Random.Range(0f, 1f);
        if (rng < lvl3Chance)
            level = 3;
        else if (rng < lvl2Chance)
            level = 2;
        else if (rng < lvl1Chance)
            level = 1;

        SetGemMaterial();
    }

    public void SetGemMaterial()
    {
        Material gemMat = null;
        if (level == 1)
            gemMat = gemMatLvl1;
        else if (level == 2)
            gemMat = gemMatLvl2;
        else if (level == 3)
            gemMat = gemMatLvl3;

		renderer.materials = new Material[2] { gold, gemMat };
	}
}
