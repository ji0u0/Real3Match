using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class Dot : MonoBehaviour
{
    public bool isMatched;

    // Board
    public int col;
    public int row;
    public Board board;

    // Move
    private int targetX;
    private int targetY;
    private Vector2 tempPosition;
    
    // Touch
    private bool checkTouch;
    private Vector2 initialTouchPosition;
    private Vector2 finalTouchPosition;

    // Swap
    private float swapAngle;
    private GameObject otherDot;
    private const float swapResist = 1f;
    
    // Return swap
    private int prevCol;
    private int prevRow;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Autometic Move
        targetX = col;
        targetY = row;

        if (Mathf.Abs(targetX - transform.position.x) > .00000000001f)
        {
            tempPosition = new Vector2(targetX, transform.position.y);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .5f);
            board.allDots[col, row] = this.gameObject;
        }

        if (Mathf.Abs(targetY - transform.position.y) > .00000000001f)
        {
            tempPosition = new Vector2(transform.position.x, targetY);
            transform.position = Vector2.Lerp(transform.position, tempPosition, .5f);
            board.allDots[col, row] = this.gameObject;
        }
    }

    private void OnMouseDown()
    {
        if (board.currentState == GameState.touch)
        {
            initialTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            checkTouch = true;
        }
    }

    private void OnMouseUp()
    {
        if (checkTouch && board.currentState == GameState.touch)
        {
            finalTouchPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            MovePieces();
        }

        checkTouch = false;
    }

    public void MovePieces()
    {
        if (Mathf.Abs(finalTouchPosition.x - initialTouchPosition.x) > swapResist
            || Mathf.Abs(finalTouchPosition.y - initialTouchPosition.y) > swapResist)
        {
            // Calculate Angle
            swapAngle = Mathf.Atan2(finalTouchPosition.y - initialTouchPosition.y,
                                    finalTouchPosition.x - initialTouchPosition.x) * 180 / Mathf.PI;
            
            // Swap Pieces
            if (swapAngle > -45 && swapAngle <= 45 && col < board.width - 1) // Right
            {
                otherDot = board.allDots[col + 1, row];
                prevCol = col;
                prevRow = row;
                this.col += 1;
                otherDot.GetComponent<Dot>().col -= 1;
            }
            else if (swapAngle > 45 && swapAngle <= 135 && row < board.height-1) // Up
            {
                otherDot = board.allDots[col, row + 1];
                prevCol = col;
                prevRow = row;
                this.row += 1;
                otherDot.GetComponent<Dot>().row -= 1;
            }
            else if (swapAngle > 135 || swapAngle <= -135 && col > 0) // Left
            {
                otherDot = board.allDots[col - 1, row];
                prevCol = col;
                prevRow = row;
                this.col -= 1;
                otherDot.GetComponent<Dot>().col += 1;
            }
            else if(swapAngle < -45 && swapAngle >= -135 && row > 0) // Down
            {
                otherDot = board.allDots[col, row - 1];
                prevCol = col;
                prevRow = row;
                this.row -= 1;
                otherDot.GetComponent<Dot>().row += 1;
            }
            else 
            {
                return;
            }

            board.currentState = GameState.wait;
            StartCoroutine(MovePiecesCo());
        }
    }

    public IEnumerator MovePiecesCo()
    {
        yield return new WaitForSeconds(.5f); // wait animation
        board.FindAllMatches();

        // Match되지 않았다면 되돌린다
        if (otherDot)
        {
            if (!isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                otherDot.GetComponent<Dot>().col = col;
                otherDot.GetComponent<Dot>().row = row;
                col = prevCol;
                row = prevRow;

                yield return new WaitForSeconds(.5f); // wait animation
                board.currentState = GameState.touch;
            }
        }
    }
}
