using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    // Score
    public Text scoreText;
    
    private int _score = 0;


    // Start is called before the first frame update
    void Start()
    {
        UpdateScoreText();
    }

    public void UpdateScoreText()
    {
        scoreText.text = _score.ToString();
    }

    public void AddScore()
    {
        _score++;
    }
}
