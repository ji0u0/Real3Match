using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum GameState
{
    touch, // 터치 가능
    wait // 대기
}

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    public GameState currentState;

    // Dots
    public GameObject[] dotPrefabs;
    public Dot[,] allDots;

    // Swap
    public Vector2 initialTouchPosition;
    public Vector2 finalTouchPosition;
    private Vector2 previousPosition;
    private Dot currentDot;
    private Dot otherDot;
    private float swapAngle;
    private const float swapResist = 1f;

    // Score
    private Score score;
    // Object Pool
    private ObjectPool dotPool;

    // Start is called before the first frame update
    void Start()
    {
        allDots = new Dot[width, height];
        score = FindObjectOfType<Score>();
        dotPool = ObjectPool.instance;

        SetUpDots();
    }

    private void SetUpDots()
    {
        Vector2 tempPosition;
        GameObject piece;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Create & Set dot
                // dotToUse = Random.Range(0, dotPrefabs.Length);
                tempPosition = new Vector2(i, j);
                // piece = Instantiate(dotPrefabs[dotToUse], tempPosition, Quaternion.identity);
                // piece.transform.parent = this.transform;
                piece = dotPool.Pool.Get();
                piece.transform.position = tempPosition;
                allDots[i, j] = piece.GetComponent<Dot>();

                allDots[i, j].row = j;
                allDots[i, j].col = i;
                // allDots[i, j].color = (DotColor)dotToUse;
                allDots[i, j].board = this;
            }
        }

        // 시작할 때 Match 검사
        ProcessMatches();
    }

    // Match 프로세스를 시작한다.
    private void ProcessMatches()
    {
        // 매치가 시작되면 터치가 불가능하게 만든다
        currentState = GameState.wait;

        // 찾는다 -> 매치된 Dot이 있는가?
        while (FindAllMatches())
        {
            // Match된 Dot이 있다면 부순다
            DestroyMatches();
            // 부수고 나면 바로 리필한다
            StartCoroutine(RefillRowCo());
        }

        // Match된 Dot이 없다면 Match할 수 있는 조합이 있는지 확인한다
        if (CheckCanMatch())
        {
            // 진행이 가능하다면 터치할 수 있도록 만든다
            currentState = GameState.touch;
        }
        else
        {
            // 진행이 불가능하다면?
            Debug.Log("cannot match");
        }
    }

    // 찾는다 : 매치된 dot의 isMatched를 모두 true로 만든다
    // 매치된 dot이 하나라도 존재한다면 true를, 그렇지 않다면 false를 반환한다
    private bool FindAllMatches()
    {
        bool bMatchesOnBoard = false;
        
        Dot currentDot;
        Dot leftDot;
        Dot rightDot;
        Dot upDot;
        Dot downDot;

        for (int i = 0; i < width; i ++)
        {
            for (int j = 0; j < height; j ++)
            {
                currentDot = allDots[i, j];

                if(currentDot)
                {
                    if (i > 0 && i < width - 1)
                    {
                        leftDot = allDots[i - 1, j];
                        rightDot = allDots[i + 1, j];

                        if (leftDot && rightDot)
                        {
                            if (leftDot.color == currentDot.color && rightDot.color == currentDot.color)
                            {
                                currentDot.isMatched = true;
                                leftDot.isMatched = true;
                                rightDot.isMatched = true;
                                bMatchesOnBoard = true;
                            }
                        }
                    }

                    if (j > 0 && j < height - 1)
                    {
                        upDot = allDots[i, j + 1];
                        downDot = allDots[i, j - 1];

                        if (upDot && downDot)
                        {
                            if (upDot.color == currentDot.color && downDot.color == currentDot.color)
                            {
                                currentDot.isMatched = true;
                                upDot.isMatched = true;
                                downDot.isMatched = true;
                                bMatchesOnBoard = true;
                            }
                        }
                    }
                }
            }
        }

        return bMatchesOnBoard;
    }

    // 부순다 : 매치된 Dot을 부순다 + 스코어 업뎃
    private void DestroyMatches()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j])
                {    
                    if (allDots[i, j].isMatched)
                    {
                        // Destroy(allDots[i, j].gameObject);
                        allDots[i, j].Pool.Release(allDots[i, j].gameObject);
                        allDots[i, j] = null;
                        score.score++;
                    }
                }
            }
        }
        score.SetScore();
    }

    // 채운다 : Dot을 떨어트려 채운다
    private IEnumerator RefillRowCo()
    {
        int dotToUse;
        Vector2 tempPosition;
        GameObject piece;

        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    allDots[i, j].MoveTo(i, j - nullCount);
                    allDots[i, j] = null;
                }
            }

            for (int n = 0; n < nullCount; n++)
            {
                dotToUse = Random.Range(0, dotPrefabs.Length);
                tempPosition = new Vector2(i, height + n);
                // piece = Instantiate(dotPrefabs[dotToUse], tempPosition, Quaternion.identity);
                // piece.transform.parent = this.transform;
                piece = dotPool.Pool.Get();
                piece.transform.position = tempPosition;
                allDots[i, height - nullCount + n] = piece.GetComponent<Dot>();

                allDots[i, height - nullCount + n].board = this;
                // allDots[i, height - nullCount + n].color = (DotColor)dotToUse;
                allDots[i, height - nullCount + n].MoveTo(i, height - nullCount + n);
            }
            
            nullCount = 0;
        }

        yield return new WaitForSeconds(.5f); // wait animation
    }

    // 입력 받은 dot의 위치를 바꾼다 (currentDot <-> otherDot)
    public void HandleDotSwap(Dot dot)
    {
        if (Mathf.Abs(finalTouchPosition.x - initialTouchPosition.x) > swapResist
            || Mathf.Abs(finalTouchPosition.y - initialTouchPosition.y) > swapResist)
        {
            // Calculate Angle
            swapAngle = Mathf.Atan2(finalTouchPosition.y - initialTouchPosition.y,
                                    finalTouchPosition.x - initialTouchPosition.x) * 180 / Mathf.PI;
            
            // Swap Pieces
            currentDot = dot;
            if (swapAngle > -45 && swapAngle <= 45 && dot.col < width - 1) // Right
            {
                otherDot = allDots[dot.col + 1, dot.row];
                previousPosition = new Vector2(dot.col, dot.row);
                currentDot.MoveTo(dot.col + 1, dot.row);
                otherDot.GetComponent<Dot>().MoveTo(dot.col - 1, dot.row);
            }
            else if (swapAngle > 45 && swapAngle <= 135 && dot.row < height-1) // Up
            {
                otherDot = allDots[dot.col, dot.row + 1];
                previousPosition = new Vector2(dot.col, dot.row);
                currentDot.MoveTo(dot.col, dot.row + 1);
                otherDot.GetComponent<Dot>().MoveTo(dot.col, dot.row - 1);
            }
            else if (swapAngle > 135 || swapAngle <= -135 && dot.col > 0) // Left
            {
                otherDot = allDots[dot.col - 1, dot.row];
                previousPosition = new Vector2(dot.col, dot.row);
                currentDot.MoveTo(dot.col - 1, dot.row);
                otherDot.GetComponent<Dot>().MoveTo(dot.col + 1, dot.row);
            }
            else if(swapAngle < -45 && swapAngle >= -135 && dot.row > 0) // Down
            {
                otherDot = allDots[dot.col, dot.row - 1];
                previousPosition = new Vector2(dot.col, dot.row);
                currentDot.MoveTo(dot.col, dot.row - 1);
                otherDot.GetComponent<Dot>().MoveTo(dot.col, dot.row + 1);
            }
            else 
            {
                return;
            }

            StartCoroutine(DotSwapCo());
        }
    }

    public IEnumerator DotSwapCo()
    {
        yield return new WaitForSeconds(.5f); // wait animation
        ProcessMatches(); // 매치 프로세스를 시작한다

        // Match되지 않았다면 되돌린다
        if (currentDot && otherDot)
        {
            if (!currentDot.isMatched && !otherDot.GetComponent<Dot>().isMatched)
            {
                otherDot.GetComponent<Dot>().MoveTo(currentDot.col, currentDot.row);
                currentDot.MoveTo((int)previousPosition.x, (int)previousPosition.y);

                yield return new WaitForSeconds(.5f); // wait animation
                currentState = GameState.touch;
            }
        }

        currentDot = null;
        otherDot = null;
    }

    // 매치할 수 있는게 있는가? = 게임 진행이 가능한가?
    private bool CheckCanMatch()
    {
        Dot currentDot;
        Dot leftDot;
        Dot rightDot;
        Dot upDot;
        Dot downDot;
        Dot doubleLeftDot;
        Dot doubleRightDot;
        Dot doubleUpDot;
        Dot doubleDownDot;
        Dot tempDot;

        DotColor color;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                currentDot = allDots[i, j];
                color = currentDot.color;
                
                if(currentDot)
                {
                    if (i > 0)
                    {
                        leftDot = allDots[i - 1, j];
                        if (leftDot != null && leftDot.color == color)
                        {
                            if (i < width - 1 && j < height - 1)
                            {
                                tempDot = allDots[i + 1, j + 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i < width - 1 && j > 0)
                            {
                                tempDot = allDots[i + 1, j - 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i < width - 2)
                            {
                                tempDot = allDots[i + 2, j];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i > 1 && j < height - 1)
                            {
                                tempDot = allDots[i - 2, j + 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i > 1 && j > 0)
                            {
                                tempDot = allDots[i - 2, j - 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i > 2)
                            {
                                tempDot = allDots[i - 3, j];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                        }
                    }
                    if (i < width - 1)
                    {
                        rightDot = allDots[i + 1, j];
                        if (rightDot != null && rightDot.color == color)
                        {
                            if (i > 0 && j < height - 1)
                            {
                                tempDot = allDots[i - 1, j + 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i > 0 && j > 0)
                            {
                                tempDot = allDots[i - 1, j - 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i > 1)
                            {
                                tempDot = allDots[i - 2, j];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i < width - 2 && j < height - 1)
                            {
                                tempDot = allDots[i + 2, j + 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i < width - 2 && j > 0)
                            {
                                tempDot = allDots[i + 2, j - 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i < width - 3)
                            {
                                tempDot = allDots[i + 3, j];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                        }
                    }
                    if (j < height - 1)
                    {
                        upDot = allDots[i, j + 1];
                        if (upDot != null && upDot.color == color)
                        {
                            if (j > 0 && i < width - 1)
                            {
                                tempDot = allDots[i + 1, j - 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j > 0 && i > 0)
                            {
                                tempDot = allDots[i - 1, j - 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j > 1)
                            {
                                tempDot = allDots[i, j - 2];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j < height - 2 && i < width - 1)
                            {
                                tempDot = allDots[i + 1, j + 2];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j < height - 2 && i > 0)
                            {
                                tempDot = allDots[i - 1, j + 2];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j < height - 3)
                            {
                                tempDot = allDots[i, j + 3];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                        }
                    }
                    if (j > 0)
                    {
                        downDot = allDots[i, j - 1];
                        if (downDot != null && downDot.color == color)
                        {
                            if (j < height - 1 && i < width - 1)
                            {
                                tempDot = allDots[i + 1, j + 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j < height - 1 && i > 0)
                            {
                                tempDot = allDots[i - 1, j + 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j < height - 2)
                            {
                                tempDot = allDots[i, j + 2];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j > 1 && i < width - 1)
                            {
                                tempDot = allDots[i + 1, j - 2];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j > 1 && i > 0)
                            {
                                tempDot = allDots[i - 1, j - 2];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j > 2)
                            {
                                tempDot = allDots[i, j - 3];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                        }
                    }
                    if (i > 1)
                    {
                        doubleLeftDot = allDots[i - 2, j];
                        if (doubleLeftDot != null && doubleLeftDot.color == color)
                        {
                            if (j < height - 1)
                            {
                                tempDot = allDots[i - 1, j + 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j > 0)
                            {
                                tempDot = allDots[i - 1, j - 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                        }
                    }
                    if (i < width - 2)
                    {
                        doubleRightDot = allDots[i + 2, j];
                        if (doubleRightDot != null && doubleRightDot.color == color)
                        {
                            if (j < height - 1)
                            {
                                tempDot = allDots[i + 1, j + 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (j > 0)
                            {
                                tempDot = allDots[i + 1, j - 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                        }
                    }
                    if (j < height - 2)
                    {
                        doubleUpDot = allDots[i, j + 2];
                        if (doubleUpDot != null && doubleUpDot.color == color)
                        {
                            if (i > 0)
                            {
                                tempDot = allDots[i - 1, j + 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i < width - 1)
                            {
                                tempDot = allDots[i + 1, j + 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                        }
                    }
                    if (j > 1)
                    {
                        doubleDownDot = allDots[i, j - 2];
                        if (doubleDownDot != null && doubleDownDot.color == color)
                        {
                            if (i > 0)
                            {
                                tempDot = allDots[i - 1, j - 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                            if (i < width - 1)
                            {
                                tempDot = allDots[i + 1, j - 1];
                                if (tempDot != null && tempDot.color == color) return true;
                            }
                        }
                    }
                }
            }
        }

        return false;
    }
}
