using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject playPanel;
    public GameObject endPanel;

    public Board board;
    public Score score;
    private StartPanel _start;
    private EndPanel _end;

    private int _highScore;
    
    void Awake()
    {
        SetFlowAction();
    }

    void Start()
    {
        ShowStartScreen();
    }

    private void SetFlowAction()
    {
        board.EndPlayAction += ShowEndScreen;

        _start = startPanel.GetComponent<StartPanel>();
        _start.OnClickPlayButtonAction += PlayGame;
        _start.OnClickExitButtonAction += ExitGame;

        _end = endPanel.GetComponent<EndPanel>();
        _end.OnClickBackButton += ShowStartScreen;
    }

    public void ShowStartScreen()
    {
        startPanel.SetActive(true);
        playPanel.SetActive(false);
        endPanel.SetActive(false);
    }

    public void PlayGame()
    {
        startPanel.SetActive(false);
        playPanel.SetActive(true);
        endPanel.SetActive(false);

        board.InitPlay();
    }

    public void ShowEndScreen()
    {
        int currentScore = score.GetAndResetScore();
        UpdateHighScore(currentScore);
        _end.SetEndPanelText(currentScore, LoadHighScore());

        startPanel.SetActive(false);
        playPanel.SetActive(false);
        endPanel.SetActive(true);
    }

    public void UpdateHighScore(int currentScore)
    {
        int highScore = LoadHighScore();

        if (currentScore > highScore)
        {
            SaveHighScore(currentScore);
        }
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void SaveHighScore(int score)
    {
        PlayerPrefs.SetInt("Score", score);
    }

    public int LoadHighScore()
    {
        _highScore = PlayerPrefs.GetInt("Score");
        return _highScore;
    }
}
