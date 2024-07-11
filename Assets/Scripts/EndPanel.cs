using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class EndPanel : MonoBehaviour
{
    [Header("UI")]
    public Text playerScoreText;
    public Text highScoreText;
    public Button backButton;
    public Text backText;
    public Color textColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 255f / 255f);

    public event UnityAction OnClickBackButton;

    private void Start()
    {
        backButton.onClick.AddListener(OnClickBackButton);
        AddEventTrigger(backButton, EventTriggerType.PointerDown, PointerDownRetryButton);
        AddEventTrigger(backButton, EventTriggerType.PointerUp, PointerUpRetryButton);
    }

    public void SetEndPanelText(int playerScore, int highScore)
    {
        playerScoreText.text = playerScore.ToString();
        highScoreText.text = "High Score : " + highScore.ToString();
    }

    private void AddEventTrigger(Button button, EventTriggerType eventTriggerType, Action<BaseEventData> callback)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventTriggerType;
        entry.callback.AddListener((eventData) => { callback(eventData); });

        trigger.triggers.Add(entry);
    }

    private void PointerDownRetryButton(BaseEventData eventData)
    {
        backText.color = Color.white;
    }

    private void PointerUpRetryButton(BaseEventData eventData)
    {
        backText.color = textColor;
    }
}
