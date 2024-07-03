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
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                // Create & Set dot
                int dotToUse = Random.Range(0, dotType.Length);
                Vector2 tempPosition = new Vector2(i, j);
                GameObject piece = Instantiate(dotType[dotToUse], tempPosition, Quaternion.identity);
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
        
        for (int i = 0; i < width; i ++)
        {
            for (int j = 0; j < height; j ++)
            {
                GameObject currentDot = allDots[i, j];

                if(currentDot)
                {
                    if (i > 0 && i < width - 1)
                    {
                        GameObject leftDot = allDots[i - 1, j];
                        GameObject rightDot = allDots[i + 1, j];

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
                        GameObject upDot = allDots[i, j + 1];
                        GameObject downDot = allDots[i, j - 1];

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
            // Match된 Dot이 없다면 터치할 수 있도록 만든다
            currentState = GameState.touch;
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
        StartCoroutine(DecreaseRowCo());
    }

    private IEnumerator DecreaseRowCo()
    {
        // 정리한다 : Dot이 아래로 떨어지도록 만든다
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
            nullCount = 0;
        }

        yield return new WaitForSeconds(.5f); // wait animation
        RefillBoard();
    }

    private void RefillBoard()
    {
        // 채운다 : 빈 공간에 새 Dot를 채운다
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    Vector2 tempPosition = new Vector2(i, j);
                    int dotToUse = Random.Range(0, dotType.Length);
                    GameObject piece = Instantiate(dotType[dotToUse], tempPosition, Quaternion.identity);
                    allDots[i, j] = piece;

                    piece.GetComponent<Dot>().col = i;
                    piece.GetComponent<Dot>().row = j;
                    piece.GetComponent<Dot>().board = this;
                }
            }
        }

        FindAllMatches();
    }
}
