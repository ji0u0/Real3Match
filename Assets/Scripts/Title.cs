using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Title : MonoBehaviour
{
    [Header("UI")]
    public Text titleText;
    public Button playButton;
    public Text playText;
    public Button quitButton;
    public Text quitText;

    [Header("Name")]
    public string titleName = "Do-Do-Dot";
    
    [Header("Color")]
    public Color textColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 255f / 255f);

    public event UnityAction OnClickPlayButtonAction;
    public event UnityAction OnClickExitButtonAction;

    private void Start()
    {
        titleText.text = titleName;

        playButton.onClick.AddListener(OnClickPlayButtonAction);
        AddEventTrigger(playButton, EventTriggerType.PointerDown, PointerDownPlayButton);
        AddEventTrigger(playButton, EventTriggerType.PointerUp, PointerUpPlayButton);

        quitButton.onClick.AddListener(OnClickExitButtonAction);
        AddEventTrigger(quitButton, EventTriggerType.PointerDown, PointerDownQuitButton);
        AddEventTrigger(quitButton, EventTriggerType.PointerUp, PointerUpQuitButton);
    }

    private void AddEventTrigger(Button button, EventTriggerType eventTriggerType, Action<BaseEventData> callback)
    {
        EventTrigger trigger = button.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = eventTriggerType;
        entry.callback.AddListener((eventData) => { callback(eventData); });

        trigger.triggers.Add(entry);
    }

    private void PointerDownPlayButton(BaseEventData eventData)
    {
        playText.color = Color.white;
    }

    private void PointerUpPlayButton(BaseEventData eventData)
    {
        playText.color = textColor;
    }

    private void PointerDownQuitButton(BaseEventData eventData)
    {
        quitText.color = Color.white;
    }

    private void PointerUpQuitButton(BaseEventData eventData)
    {
        quitText.color = textColor;
    }
}
