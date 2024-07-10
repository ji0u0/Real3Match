using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    // Score
    [Tooltip("현재 플레이어 점수")]
    public int score = 0;
    
    [Tooltip("점수를 표시할 UI Text 객체")]
    public Text scoreText;

    [Tooltip("3, 4, 5개 매치에 따른 추가 점수")]
    public int[] scoreByMatchCount = {3, 6, 9};

    void Start()
    {
        SetScore();
    }

    public void SetScore()
    {
        scoreText.text = score.ToString();
    }

}
