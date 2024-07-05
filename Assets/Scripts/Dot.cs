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
    private Camera mainCamera;

    // Move
    public float swapDuration;

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
        swapDuration = board.swapDuration;
    }

    private void OnMouseDown()
    {
        board.MouseDownAction(mainCamera.ScreenToWorldPoint(Input.mousePosition));

    }

    private void OnMouseUp()
    {
        board.MouseUpAction(mainCamera.ScreenToWorldPoint(Input.mousePosition), this);
    }

    // 입력된 좌표값에 따라 이동시킨다
    public void MoveTo(int targetCol, int targetRow) 
    {
        col = targetCol;
        row = targetRow;
        Vector2 targetPosition = new Vector2(col, row);
        transform.DOMove(targetPosition, swapDuration);
        board.allDots[col, row] = this;
    }
}
