﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayManager : MonoBehaviour
{
    [SerializeField] private bool testStartAsReset;
    public CoinManager coinManager;
    public HandManager handManager;
    [SerializeField] private TutorialManager tutorialManager;

    public new Camera camera;
    [SerializeField] private Transform fallOffArea;
    [SerializeField] private Button reloadButton, resetButton, reloadButtonYes, reloadButtonNo;
    public GameObject reloadPanel;
    public AudioClip[] pileCollapseClips;

    public float[] pileCollapseSizes;
    public float timeToAscendToNextStage = 0.3f;

    [HideInInspector] public PlayState State;
    [HideInInspector] public int score;
    [HideInInspector] public int nextStageTarget;
    [HideInInspector] public float currentHighestPoint;

    private float coinPileHeight;

    void Start()
    {
        EventManager.CoinScores.AddListener(OnCoinScores);
        EventManager.CoinFalls.AddListener(OnCoinFalls);
        EventManager.CoinsFallOffPile.AddListener(OnCoinsFallOffPIle);
        EventManager.CoinPileFallsOver.AddListener(OnCoinPileFallsOver);
        EventManager.HandAscendedCoinPile.AddListener(OnHandAscendedCoinPile);
        EventManager.GoingGameOver.AddListener(OnGoingGameOver);
        reloadButton.onClick.AddListener(PressReloadButton);
        resetButton.onClick.AddListener(PressResetButton);
        reloadButtonYes.onClick.AddListener(PressReloadButtonYes);
        reloadButtonNo.onClick.AddListener(PressReloadButtonNo);

        coinManager.Initialize();
        tutorialManager.Initialize();
        if (tutorialManager.tutorialActive || GameManager.I.previousScene == -1) coinManager.SpawnCoin();

        nextStageTarget = 10;

        SetState(new PlayDefault(this));
    }

    void Update()
    {
        State.Update();

        if (Input.GetMouseButtonDown(0) && !IsPointerOverGameObject())
        {
            State.PressDownBlank();
        }
        
        if (Input.GetMouseButtonUp(0) && !IsPointerOverGameObject())
        {
            if (ScreenshotTool.PauseOnRelease) return;
            State.PressUp();
        }
    }

    void OnApplicationQuit()
    {
        State.OnApplicationQuit();
    }

    private void OnCoinScores(Coin coin)
    {
        State.OnCoinScores(coin);

        coinPileHeight = coin.transform.position.y;
    }

    private void OnCoinFalls(Coin coin)
    {
        State.OnCoinFalls(coin);
    }

    private void OnCoinsFallOffPIle(Coin[] coins)
    {
        foreach (Coin coin in coins)
        {
            State.OnCoinFallsOffPile(coin);
        }
    }

    private void OnHandAscendedCoinPile()
    {
        State.OnHandAscendedCoinPile();

        // Move the fall off area up
        fallOffArea.position = new Vector3(fallOffArea.position.x, handManager.hand.transform.position.y + 0.57f, fallOffArea.position.z);
    }

    private void OnCoinPileFallsOver()
    {
        State.OnCoinPileFallsOver();
    }

    private void OnGoingGameOver()
    {
        // Save score and high score
        GameManager.I.finalScore = score;
        GameManager.I.finalPileHeight = coinPileHeight;
    }

    private void PressReloadButton()
    {
        State.PressReloadButton();
    }

    private void PressResetButton()
    {
        Data.firstTimePlaying = 1;
        Data.firstTimeInHome = 1;
    }

    private void PressReloadButtonYes()
    {
        State.PressReloadButtonYes();
    }

    private void PressReloadButtonNo()
    {
        State.PressReloadButtonNo();
    }

    public void SetState(PlayState state)
    {
        if (State != null) State.Exit();
        State = state;
        State.Enter();
    }

    public void ChangeScore(int addition)
    {
        score += addition;
        EventManager.ScoreChanges.Invoke(score);
    }

    public void SetScore(int value)
    {
        score = value;
        EventManager.ScoreChanges.Invoke(score);
    }

    private bool IsPointerOverGameObject()
    {
        return EventSystem.current.currentSelectedGameObject != null;
        
        // Check mouse
        if (EventSystem.current.IsPointerOverGameObject()) return true;
            
        // Check touch
        if (Input.touchCount > 0 && Input.touches[0].phase == TouchPhase.Began && EventSystem.current.IsPointerOverGameObject(Input.touches[0].fingerId)) return true;
             
        return false;
    }
}
