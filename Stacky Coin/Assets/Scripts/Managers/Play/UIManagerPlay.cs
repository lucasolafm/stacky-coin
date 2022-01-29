using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManagerPlay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;

    void Start()
    {
        EventManager.ScoreChanges.AddListener(OnNewScore);
    }

    private void OnNewScore(int newScore)
    {
        scoreText.text = newScore.ToString();
    }
}
