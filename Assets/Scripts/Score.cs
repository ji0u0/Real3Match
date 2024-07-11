using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    [Header("UI")]
    public Text scoreText;

    [Header("Score")]
    [SerializeField] private int _3MatchScore = 3;
    [SerializeField] private int _4MatchScore = 6;
    [SerializeField] private int _5MatchScore = 9;

    private int _score = 0;

    void Start()
    {
        UpdateScoreText();
    }

    public void UpdateScoreText()
    {
        scoreText.text = _score.ToString();
    }

    public void AddScore(int additionalScore)
    {
        _score += additionalScore;
    }

    public int GetAndResetScore()
    {
        int currentScore = _score;
        _score = 0;
        return currentScore;
    }

    public int returnScoreByMatchCount(int serialDotCount)
    {
        if (serialDotCount == 3)
        {
            return _3MatchScore;
        }
        else if (serialDotCount == 4)
        {
            return _4MatchScore;
        }
        else if (serialDotCount == 5)
        {
            return _5MatchScore;
        }
        return 0;
    }
}
