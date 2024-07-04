using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject[] dotType;
    public GameObject[,] allDots;

    // Score
    public int score = 0;
    public Text scoreText;

    // Start is called before the first frame update
    void Start()
    {
        currentState = GameState.touch;
        allDots = new GameObject[width, height];

        scoreText.text = score.ToString();
        SetUp();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void SetUp()
    {  
        int dotToUse;  
        Vector2 tempPosition;
        GameObject piece;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Create & Set dot
                dotToUse = Random.Range(0, dotType.Length);
                tempPosition = new Vector2(i, j);
                piece = Instantiate(dotType[dotToUse], tempPosition, Quaternion.identity);
                piece.transform.parent = this.transform;
                allDots[i, j] = piece;

                piece.GetComponent<Dot>().row = j;
                piece.GetComponent<Dot>().col = i;
                piece.GetComponent<Dot>().board = this;
            }
        }

        // 시작할 때 Match 검사
        FindAllMatches();
    }

    public void FindAllMatches()
    {
        // 찾는다 : 매치된 dot의 isMatched를 모두 true로 만든다
        currentState = GameState.wait;
        bool bMatchesOnBoard = false;
        
        GameObject currentDot;
        GameObject leftDot;
        GameObject rightDot;
        GameObject upDot;
        GameObject downDot;

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
                            if (leftDot.tag == currentDot.tag && rightDot.tag == currentDot.tag)
                            {
                                currentDot.GetComponent<Dot>().isMatched = true;
                                leftDot.GetComponent<Dot>().isMatched = true;
                                rightDot.GetComponent<Dot>().isMatched = true;
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
                            if (upDot.tag == currentDot.tag && downDot.tag == currentDot.tag)
                            {
                                currentDot.GetComponent<Dot>().isMatched = true;
                                upDot.GetComponent<Dot>().isMatched = true;
                                downDot.GetComponent<Dot>().isMatched = true;
                                bMatchesOnBoard = true;
                            }
                        }
                    }
                }
            }
        }

        if (bMatchesOnBoard)
        {
            // Match된 Dot이 있다면 부순다
            DestroyMatches();
            return;
        }
        else
        {
            if (CheckCanMatch())
            {
                // Match된 Dot이 없다면 터치할 수 있도록 만든다
                currentState = GameState.touch;
            }
            else
            {
                Debug.Log("cannot match");
            }
        }
    }

    public void DestroyMatches()
    {
        // 부순다 : 매치된 Dot을 부순다
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j])
                {    
                    if (allDots[i, j].GetComponent<Dot>().isMatched)
                    {
                        Destroy(allDots[i, j]);
                        allDots[i, j] = null;

                        score++;
                    }
                }
            }
        }
        
        // Score 세팅
        scoreText.text = score.ToString();
        StartCoroutine(RefillRowCo());
    }

    private IEnumerator RefillRowCo()
    {
        int dotToUse;
        Vector2 tempPosition;
        GameObject piece;

        // 채운다 : Dot을 떨어트려 채운다
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
                    allDots[i, j].GetComponent<Dot>().row -= nullCount;
                    allDots[i, j] = null;
                }
            }

            for (int n = 0; n < nullCount; n++)
            {
                dotToUse = Random.Range(0, dotType.Length);
                tempPosition = new Vector2(i, height + n);
                piece = Instantiate(dotType[dotToUse], tempPosition, Quaternion.identity);
                piece.transform.parent = this.transform;
                allDots[i, height - nullCount + n] = piece;

                piece.GetComponent<Dot>().col = i;
                piece.GetComponent<Dot>().row = height - nullCount + n;
                piece.GetComponent<Dot>().board = this;
            }
            
            nullCount = 0;
        }

        yield return new WaitForSeconds(.5f); // wait animation
        FindAllMatches();
    }

    // private IEnumerator DecreaseRowCo()
    // {
    //     // 정리한다 : Dot이 아래로 떨어지도록 만든다
    //     int nullCount = 0;
    //     for (int i = 0; i < width; i++)
    //     {
    //         for (int j = 0; j < height; j++)
    //         {
    //             if (allDots[i, j] == null)
    //             {
    //                 nullCount++;
    //             }
    //             else if (nullCount > 0)
    //             {
    //                 allDots[i, j].GetComponent<Dot>().row -= nullCount;
    //                 allDots[i, j] = null;
    //             }
    //         }
    //         nullCount = 0;
    //     }
    //     yield return new WaitForSeconds(.5f); // wait animation
    //     StartCoroutine(RefillBoardCo());
    // }

    // private IEnumerator RefillBoardCo()
    // {
    //     // 채운다 : 빈 공간에 새 Dot를 채운다
    //     for (int i = 0; i < width; i++)
    //     {
    //         for (int j = 0; j < height; j++)
    //         {
    //             if (allDots[i, j] == null)
    //             {
    //                 Vector2 tempPosition = new Vector2(i, j);
    //                 int dotToUse = Random.Range(0, dotType.Length);
    //                 GameObject piece = Instantiate(dotType[dotToUse], tempPosition, Quaternion.identity);
    //                 allDots[i, j] = piece;

    //                 piece.GetComponent<Dot>().col = i;
    //                 piece.GetComponent<Dot>().row = j;
    //                 piece.GetComponent<Dot>().board = this;
    //             }
    //         }
    //     }
    //     yield return new WaitForSeconds(.5f);
    //     FindAllMatches();
    // }

    private bool CheckCanMatch()
    {
        GameObject currentDot;
        GameObject leftDot;
        GameObject rightDot;
        GameObject upDot;
        GameObject downDot;
        GameObject doubleLeftDot;
        GameObject doubleRightDot;
        GameObject doubleUpDot;
        GameObject doubleDownDot;
        GameObject tempDot;

        string tag;

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                currentDot = allDots[i, j];
                tag = currentDot.tag;
                
                if(currentDot)
                {
                    if (i > 0)
                    {
                        leftDot = allDots[i - 1, j];
                        if (leftDot != null && leftDot.tag == tag)
                        {
                            if (i < width - 1 && j < height - 1)
                            {
                                tempDot = allDots[i + 1, j + 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i < width - 1 && j > 0)
                            {
                                tempDot = allDots[i + 1, j - 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i < width - 2)
                            {
                                tempDot = allDots[i + 2, j];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i > 1 && j < height - 1)
                            {
                                tempDot = allDots[i - 2, j + 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i > 1 && j > 0)
                            {
                                tempDot = allDots[i - 2, j - 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i > 2)
                            {
                                tempDot = allDots[i - 3, j];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                        }
                    }
                    if (i < width - 1)
                    {
                        rightDot = allDots[i + 1, j];
                        if (rightDot != null && rightDot.tag == tag)
                        {
                            if (i > 0 && j < height - 1)
                            {
                                tempDot = allDots[i - 1, j + 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i > 0 && j > 0)
                            {
                                tempDot = allDots[i - 1, j - 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i > 1)
                            {
                                tempDot = allDots[i - 2, j];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i < width - 2 && j < height - 1)
                            {
                                tempDot = allDots[i + 2, j + 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i < width - 2 && j > 0)
                            {
                                tempDot = allDots[i + 2, j - 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i < width - 3)
                            {
                                tempDot = allDots[i + 3, j];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                        }
                    }
                    if (j < height - 1)
                    {
                        upDot = allDots[i, j + 1];
                        if (upDot != null && upDot.tag == tag)
                        {
                            if (j > 0 && i < width - 1)
                            {
                                tempDot = allDots[i + 1, j - 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j > 0 && i > 0)
                            {
                                tempDot = allDots[i - 1, j - 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j > 1)
                            {
                                tempDot = allDots[i, j - 2];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j < height - 2 && i < width - 1)
                            {
                                tempDot = allDots[i + 1, j + 2];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j < height - 2 && i > 0)
                            {
                                tempDot = allDots[i - 1, j + 2];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j < height - 3)
                            {
                                tempDot = allDots[i, j + 3];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                        }
                    }
                    if (j > 0)
                    {
                        downDot = allDots[i, j - 1];
                        if (downDot != null && downDot.tag == tag)
                        {
                            if (j < height - 1 && i < width - 1)
                            {
                                tempDot = allDots[i + 1, j + 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j < height - 1 && i > 0)
                            {
                                tempDot = allDots[i - 1, j + 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j < height - 2)
                            {
                                tempDot = allDots[i, j + 2];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j > 1 && i < width - 1)
                            {
                                tempDot = allDots[i + 1, j - 2];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j > 1 && i > 0)
                            {
                                tempDot = allDots[i - 1, j - 2];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j > 2)
                            {
                                tempDot = allDots[i, j - 3];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                        }
                    }
                    if (i > 1)
                    {
                        doubleLeftDot = allDots[i - 2, j];
                        if (doubleLeftDot != null && doubleLeftDot.tag == tag)
                        {
                            if (j < height - 1)
                            {
                                tempDot = allDots[i - 1, j + 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j > 0)
                            {
                                tempDot = allDots[i - 1, j - 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                        }
                    }
                    if (i < width - 2)
                    {
                        doubleRightDot = allDots[i + 2, j];
                        if (doubleRightDot != null && doubleRightDot.tag == tag)
                        {
                            if (j < height - 1)
                            {
                                tempDot = allDots[i + 1, j + 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (j > 0)
                            {
                                tempDot = allDots[i + 1, j - 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                        }
                    }
                    if (j < height - 2)
                    {
                        doubleUpDot = allDots[i, j + 2];
                        if (doubleUpDot != null && doubleUpDot.tag == tag)
                        {
                            if (i > 0)
                            {
                                tempDot = allDots[i - 1, j + 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i < width - 1)
                            {
                                tempDot = allDots[i + 1, j + 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                        }
                    }
                    if (j > 1)
                    {
                        doubleDownDot = allDots[i, j - 2];
                        if (doubleDownDot != null && doubleDownDot.tag == tag)
                        {
                            if (i > 0)
                            {
                                tempDot = allDots[i - 1, j - 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                            if (i < width - 1)
                            {
                                tempDot = allDots[i + 1, j - 1];
                                if (tempDot != null && tempDot.tag == tag) return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }
}
