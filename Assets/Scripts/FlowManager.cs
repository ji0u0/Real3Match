using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlowManager : MonoBehaviour
{
    public GameObject startPanel;
    public GameObject playPanel;
    public GameObject endPanel;

    public Board board;
    public Title title;
    
    void Awake()
    {
        board.EndPlayAction += ShowEndScreen;
        title.OnClickPlayButtonAction += PlayGame;
        title.OnClickExitButtonAction += ExitGame;
    }

    void Start()
    {
        ShowTitleScreen();
    }

    public void ShowTitleScreen()
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
        startPanel.SetActive(false);
        playPanel.SetActive(false);
        endPanel.SetActive(true);
    }

    public void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Appliction.Quit();
        #endif
    }
}
