using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using System;
using DG.Tweening;

public enum DotColor
{
    blue,
    coral,
    green,
    pink,
    purple,
    white,
    yellow
}

public class Dot : MonoBehaviour
{
    public DotColor color;
    public string address;

    // Board
    // public int col;
    // public int row;
    public Vector2Int position = new Vector2Int();
    
    // Touch
    private Camera mainCamera;

    public Action<Vector2> MouseDownAction;
    public Action<Vector2, Dot> MouseUpAction;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnMouseDown()
    {
        MouseDownAction(mainCamera.ScreenToWorldPoint(Input.mousePosition));
    }

    private void OnMouseUp()
    {
        MouseUpAction(mainCamera.ScreenToWorldPoint(Input.mousePosition), this);
    }

    // 입력된 좌표값에 따라 이동시킨다
    // public void MoveTo(int targetCol, int targetRow) 
    // {
    //     col = targetCol;
    //     row = targetRow;
    //     Vector2 targetPosition = new Vector2(col, row);
    //     transform.DOMove(targetPosition, swapDuration);
    //     board.allDots[col, row] = this;
    // }
}
