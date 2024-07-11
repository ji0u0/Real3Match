using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum DotColor
{
    blue,
    coral,
    green,
    orange,
    pink,
    purple,
    white,
    yellow
}

public class Dot : MonoBehaviour
{
    public string address;
    public DotColor color;
    public Vector2Int position;
    
    // Touch
    public Action<Vector2> MouseDownAction;
    public Action<Vector2, Dot> MouseUpAction;
    private Camera _mainCamera;

    void Start()
    {
        _mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        MouseDownAction(_mainCamera.ScreenToWorldPoint(Input.mousePosition));
    }

    private void OnMouseUp()
    {
        MouseUpAction(_mainCamera.ScreenToWorldPoint(Input.mousePosition), this);
    }
}
