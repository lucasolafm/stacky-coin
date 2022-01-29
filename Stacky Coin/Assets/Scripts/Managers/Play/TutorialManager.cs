using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TutorialManager : MonoBehaviour
{
    [Serializable]
    public struct TutorialLevel
    {
        public TutorialState state;
        public string text;
        [HideInInspector] public Coin[] coins;
    }

    public CoinManager coinManager;
    public InstantiationManagerPlay instantiationManager;
    public RectTransform panel;
    public TextMeshProUGUI tutorialText;
    [HideInInspector] public bool tutorialActive;
    [HideInInspector] public int tutorialObjectsSpawned = -1;
    private TutorialState state;
    public TutorialLevel[] tutorialLevels;
    public int currentLevelNr;
    private Vector3 endScale;

    void Awake()
    {
        EventManager.CoinFlips.AddListener(OnFlip);

        endScale = panel.localScale;
    }

    public void SetState(TutorialState newState)
    {
        if (state != null) state.Exit();
        state = newState;
        state.Enter();
    }

    public void Initialize()
    {
        currentLevelNr = Data.tutorialLevel;

        if (currentLevelNr >= tutorialLevels.Length) return;

        tutorialActive = true;
        panel.gameObject.SetActive(true);

        SetTutorialStates();
        SpawnTutorialObjects();

        state = null;
        SetState(tutorialLevels[currentLevelNr].state);

        tutorialObjectsSpawned = coinManager.startingCoinStackAmount;
    }

    public void AdvanceLevel()
    {
        currentLevelNr++;
        Data.tutorialLevel = currentLevelNr;
    }

    public void OnFlip(Coin coin, float chargeTime)
    {
        if (!tutorialActive) return;

        tutorialObjectsSpawned++;

        state.OnFlip(chargeTime);
    }

    public void AnimateTextBox()
    {
        panel.localScale = Vector3.zero;
        LeanTween.scale(panel, endScale, 0.3f).setEase(LeanTweenType.easeOutBack);
    }

    public void InsertNextNewObjects(Coin[] objects)
    {
        for (int i = 0; i < objects.Length; i++)
        {
            coinManager.Coins.Remove(objects[i]);
            coinManager.Coins[coinManager.spawnedCoinsCount + i] = objects[i];
            objects[i].SetState(new CoinInactive(objects[i]));
        }
    }

    private void SetTutorialStates()
    {
        tutorialLevels[0].state = new TutorialFlip(this);
        tutorialLevels[1].state = new TutorialFlipAgain(this);
        tutorialLevels[2].state = new TutorialFlipHigher(this);
        tutorialLevels[3].state = new TutorialHeavyObj(this);
        tutorialLevels[4].state = new TutorialGem(this);
        tutorialLevels[5].state = new TutorialPause(this);
        tutorialLevels[6].state = new TutorialPause2(this);
        tutorialLevels[7].state = new TutorialKey(this);
        tutorialLevels[8].state = new TutorialKey2(this);
        tutorialLevels[9].state = new TutorialEnd(this);
    }

    private void SpawnTutorialObjects()
    {
        Coin obj1 = null;
        Coin obj2 = null;
        Coin obj3 = null;

        // Flip
        obj1 = instantiationManager.InstantiateObject(CoinType.Coin);
        obj1.SetState(new CoinInactive(obj1));
        tutorialLevels[0].coins = new Coin[1] { obj1 };
        

        // Flip again
        obj1 =  instantiationManager.InstantiateObject(CoinType.Coin);
        obj1.SetState(new CoinInactive(obj1));
        tutorialLevels[1].coins = new Coin[1] { obj1 };
        

        // Flip higher
        obj1 = instantiationManager.InstantiateObject(CoinType.Coin);
        obj2 = instantiationManager.InstantiateObject(CoinType.Coin);
        obj3 = instantiationManager.InstantiateObject(CoinType.Coin); 
        obj1.SetState(new CoinInactive(obj1));
        obj2.SetState(new CoinInactive(obj2));
        obj3.SetState(new CoinInactive(obj3));
        tutorialLevels[2].coins = new Coin[3] { obj1, obj2, obj3 };
        

        // Heavy objects
        obj1 = instantiationManager.InstantiateObject(CoinType.Gem);
        obj1.SetState(new CoinInactive(obj1));
        tutorialLevels[3].coins = new Coin[1] { obj1 };
        

        // Gems
        obj1 = instantiationManager.InstantiateObject(CoinType.Gem, -1, 1);
        obj1.SetState(new CoinInactive(obj1));
        tutorialLevels[4].coins = new Coin[1] { obj1 };
        

        // Pause
        obj1 = instantiationManager.InstantiateObject(CoinType.Coin);
        obj1.SetState(new CoinInactive(obj1));
        tutorialLevels[5].coins = new Coin[1] { obj1 };
        
        // Pause 2
        obj1 = instantiationManager.InstantiateObject(CoinType.Coin);
        obj1.SetState(new CoinInactive(obj1));
        tutorialLevels[6].coins = new Coin[1] { obj1 };

        // Key
        obj1 = instantiationManager.InstantiateObject(CoinType.Key, -1, 0);
        obj1.SetState(new CoinInactive(obj1));
        tutorialLevels[7].coins = new Coin[1] { obj1 };
        
        // Key 2
        obj1 = instantiationManager.InstantiateObject(CoinType.Coin);
        obj1.SetState(new CoinInactive(obj1));
        tutorialLevels[8].coins = new Coin[1] { obj1 };
    }

    public void EndTutorial()
    {
        LeanTween.scale(panel, Vector3.zero, 0.2f).setEase(LeanTweenType.easeInBack);
        tutorialActive = false;
    }
}
