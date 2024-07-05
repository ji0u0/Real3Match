using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
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
    public bool isMatched;
    public DotColor color;

    // Board
    public int col;
    public int row;
    public Board board;
    
    // Touch
    private bool checkTouch; // Mouse down 시 true, up 시 false
    private Camera mainCamera;

    // Move
    public float swapDuration = .5f;
    private Vector2 targetPosition;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        swapDuration = board.swapDuration;
    }

    private void OnMouseDown()
    {
        if (board.currentState == GameState.touch)
        {
            board.initialTouchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            checkTouch = true;
        }
    }

    private void OnMouseUp()
    {
        if (checkTouch && board.currentState == GameState.touch)
        {
            board.finalTouchPosition = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            board.HandleDotSwap(this);
        }
        checkTouch = false;
    }

    // 입력된 좌표값에 따라 이동시킨다
    public void MoveTo(int targetCol, int targetRow) 
    {
        col = targetCol;
        row = targetRow;
        targetPosition = new Vector2(col, row);
        transform.DOMove(targetPosition, swapDuration);
        board.allDots[col, row] = this;
    }
}
