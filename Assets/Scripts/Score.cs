using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{
    // Score
    public int score = 0;
    public Text scoreText;

    // Start is called before the first frame update
    void Start()
    {
        SetScore();
    }

    public void SetScore()
    {
        scoreText.text = score.ToString();
    }

}
