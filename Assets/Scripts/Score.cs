using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    [Tooltip("점수를 표시할 UI Text 객체")]
    public Text scoreText;

    [Tooltip("3, 4, 5개 매치에 따른 추가 점수")]
    public int[] scoreByMatchCount = {3, 6, 9};
    
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

    public void AddScore(int additionalScore)
    {
        _score += additionalScore;
    }
}
