using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public enum GameState
{
    touch, // 터치 가능
    wait // 대기
}

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    private GameState currentState;

    // Asset + Object pool
    public int poolSize = 10;
    public string searchFolderAddress = "Assets/Prefabs/Dots";
    public List<string> assetAddresses = new List<string>(); // 프리팹 주소
    private List<GameObject> colorParents = new List<GameObject>(); // 부모 오브젝트

    // Dots
    private Dot[,] allDots;
    HashSet<Dot> matchedDots = new HashSet<Dot>();

    // Touch
    private Vector2 initialTouchPosition;
    private Vector2 finalTouchPosition;
    private Vector2Int previousPosition;
    private bool checkTouch;

    // Swap
    private Dot currentDot;
    private Dot otherDot;
    public float swapDuration = .3f;
    public float termDuration = .1f;
    public float swapResist = 1f;

    // Manager
    public Score scoreManager;
    public ObjectPool objectPoolManager;

    private void Awake()
    {
        InitObjectPool();
    }

    void Start()
    {
        SetUpDots();
    }

    private void InitObjectPool()
    {
        for (int i = 0; i < assetAddresses.Count; i++)
        {
            string address = assetAddresses[i];
            objectPoolManager.pool[address] = new Queue<MonoBehaviour>();

            DotColor color = (DotColor)i;
            GameObject colorParent = new GameObject(color.ToString() + " Pool");
            colorParents.Add(colorParent);

            for (int j = 0; j < poolSize; j++)
            {
                GameObject piece = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(address));
                piece.transform.parent = colorParent.transform;
                piece.GetComponent<Dot>().color = color;
                piece.SetActive(false);
                objectPoolManager.pool[address].Enqueue(piece.GetComponent<Dot>());
            }
        }
    }

    private Dot GetDotFromPool()
    {
        int randomIndex = Random.Range(0, assetAddresses.Count);
        string address = assetAddresses[randomIndex];

        Dot piece = objectPoolManager.GetObject<Dot>(address);
        piece.gameObject.transform.parent = colorParents[randomIndex].transform;
        piece.MouseDownAction = MouseDown;
        piece.MouseUpAction = MouseUp;
        piece.address = address;

        return piece;
    }

    private void SetUpDots()
    {
        // Dots 생성
        allDots = new Dot[width, height];
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Vector2 tempPosition = new Vector2(i, j);
                GameObject piece = GetDotFromPool().gameObject;
                piece.transform.position = tempPosition;
                allDots[i, j] = piece.GetComponent<Dot>();
                allDots[i, j].position.x = i;
                allDots[i, j].position.y = j;
            }
        }

        // 시작하자 마자 Match된 경우, Match할 게 없는 경우를 배제한다
        ReplaceDots();
    }

    private void ReplaceDots()
    {
        while (true)
        {
            if (FindAllMatches().Count != 0) // Match된 dot들을 교체한다
            {
                Vector2Int tempPosition;

                foreach (Dot dot in FindAllMatches())
                {
                    // match된 dot 삭제
                    tempPosition = dot.position;
                    objectPoolManager.ReturnToPool(allDots[tempPosition.x, tempPosition.y], allDots[tempPosition.x, tempPosition.y].address);
                    allDots[tempPosition.x, tempPosition.y] = null;

                    // 삭제된 dot 다시 생성
                    GameObject piece = GetDotFromPool().gameObject;
                    piece.transform.position = (Vector2)tempPosition;
                    allDots[tempPosition.x, tempPosition.y] = piece.GetComponent<Dot>();
                    allDots[tempPosition.x, tempPosition.y].position = tempPosition;
                }   
            }
            else if (!CheckCanMatch()) // match할 수 있는 dot이 없다면 전체 초기화한다
            {
                for (int i = 0; i < width; i++)
                {
                    for (int j = 0; j < height; j++)
                    {
                        objectPoolManager.ReturnToPool(allDots[i, j], allDots[i, j].address);
                        allDots[i, j] = null;

                        Vector2Int tempPosition = new Vector2Int(i, j);
                        GameObject piece = GetDotFromPool().gameObject;
                        piece.transform.position = (Vector2)tempPosition;
                        allDots[i, j] = piece.GetComponent<Dot>();
                        allDots[i, j].position = tempPosition;
                    }
                }
            }
            else // 조건을 만족하면 반복을 멈추고 게임을 시작한다
            {
                currentState = GameState.touch;
                break;
            }
        }
    }

    // Match 프로세스를 시작 + GameState 제어
    private IEnumerator ProcessMatchesCo()
    {
        // 매치가 시작되면 터치가 불가능하게 만든다
        currentState = GameState.wait;
        yield return new WaitForSeconds(swapDuration); // wait animation

        // 찾는다 -> 매치된 Dot이 있는가?
        HashSet<Dot> matchedDots = FindAllMatches();
        while (matchedDots.Count != 0)
        {
            // Match된 Dot이 있다면 부순다
            DestroyMatches(matchedDots);
            yield return new WaitForSeconds(termDuration);

            // 부수고 나면 리필한다
            StartCoroutine(RefillRowCo());
            yield return new WaitForSeconds(swapDuration + termDuration);

            // 다시 검사
            matchedDots = FindAllMatches();
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

    // 찾는다 : 매치된 dot을 저장해 반환한다
    private HashSet<Dot> FindAllMatches() // Matchset return 값으로
    {
        matchedDots.Clear();

        for (int i = 0; i < width; i ++)
        {
            for (int j = 0; j < height; j ++)
            {
                Dot currentDot = allDots[i, j];
                DotColor currentColor = currentDot.color;
                if(currentDot != null)
                {
                    if (i > 0 && i < width - 1) // 가로 체크
                    {
                        Dot leftDot = allDots[i - 1, j];
                        Dot rightDot = allDots[i + 1, j];

                        if (leftDot != null && leftDot.color == currentColor &&
                            rightDot != null && rightDot.color == currentColor)
                        {
                            matchedDots.Add(currentDot);
                            matchedDots.Add(leftDot);
                            matchedDots.Add(rightDot);
                        }
                    }
                    if (j > 0 && j < height - 1) // 세로 체크
                    {
                        Dot upDot = allDots[i, j + 1];
                        Dot downDot = allDots[i, j - 1];

                        if (upDot != null && upDot.color == currentDot.color &&
                            downDot != null && downDot.color == currentDot.color)
                        {
                            matchedDots.Add(currentDot);
                            matchedDots.Add(upDot);
                            matchedDots.Add(downDot);
                        }
                    }
                }
            }
        }

        return matchedDots;
    }

    // 부순다 : 매치된 Dot을 부순다 + 스코어 업뎃
    // private void DestroyMatches(HashSet<Dot> matchedDots)
    private void DestroyMatches(HashSet<Dot> matchedDots)
    {
        foreach (Dot dot in matchedDots)
        {
            allDots[dot.position.x, dot.position.y] = null;
            objectPoolManager.ReturnToPool(dot, dot.address);
            scoreManager.score++;
        }

        scoreManager.SetScore();
    }

    // 채운다 : Dot을 떨어트려 채운다
    private IEnumerator RefillRowCo()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            Vector2Int tempPosition;

            // 있는 dots들 떨어트리기
            for (int j = 0; j < height; j++)
            {
                if (allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    tempPosition = new Vector2Int(i, j - nullCount);
                    DotMoveTo(allDots[i, j], tempPosition);
                    allDots[i, j] = null;
                }
            }
            
            // 새 dots 생성해서 떨어트리기
            for (int n = 0; n < nullCount; n++)
            {
                tempPosition = new Vector2Int(i, height + n);
                GameObject piece = GetDotFromPool().gameObject;
                piece.transform.position = (Vector2)tempPosition;
                allDots[i, height - nullCount + n] = piece.GetComponent<Dot>();
                tempPosition = new Vector2Int(i, height - nullCount + n);
                DotMoveTo(allDots[i, height - nullCount + n], tempPosition);
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(swapDuration); // wait animation
    }

    // Mouse control
    public void MouseDown(Vector2 vector)
    {
        if (currentState == GameState.touch)
        {
            initialTouchPosition = vector;
            checkTouch = true;
        }
    }

    public void MouseUp(Vector2 vector, Dot dot)
    {
        if (checkTouch && currentState == GameState.touch)
        {
            finalTouchPosition = vector;
            HandleDotSwap(dot);
        }
        checkTouch = false;
    }

    // 입력 받은 dot의 위치를 바꾼다 (currentDot <-> otherDot)
    private void HandleDotSwap(Dot dot)
    {
        float distanceX = finalTouchPosition.x - initialTouchPosition.x;
        float distanceY = finalTouchPosition.y - initialTouchPosition.y;

        if (Mathf.Abs(distanceX) > swapResist || Mathf.Abs(distanceY) > swapResist)
        {
            // Calculate Angle
            float swapAngle = Mathf.Atan2(distanceY, distanceX) * 180 / Mathf.PI;
            
            // Swap Pieces
            currentDot = dot;
            Vector2Int newPosition = dot.position + JudgeDirection(swapAngle);
            if (0 <= newPosition.x && newPosition.x < width && 0 <= newPosition.y && newPosition.y < height)
            {
                otherDot = allDots[newPosition.x, newPosition.y];
                previousPosition = dot.position;
                DotMoveTo(currentDot, newPosition);
                DotMoveTo(otherDot, previousPosition);

                // 매치 시작 -> 아니면 되돌아가기
                StartCoroutine(ProcessMatchesCo());
                StartCoroutine(DotSwapCo());
            }
        }
    }

    private Vector2Int JudgeDirection(float angle)
    {
        if (angle > -45 && angle <= 45) // Right
        {
            return new Vector2Int(1, 0);
        }
        else if (angle > 45 && angle <= 135) // Up
        {
            return new Vector2Int(0, 1);
        }
        else if (angle > 135 || angle <= -135) // Left
        {
            return new Vector2Int(-1, 0);
        }
        else if (angle > -135 && angle <= -45) // Down
        {
            return new Vector2Int(0, -1);
        }
        else
        {
            return new Vector2Int(0, 0);
        }
    }

    private IEnumerator DotSwapCo()
    {
        yield return new WaitForSeconds(swapDuration); // wait animation

        // Match되지 않았다면 되돌린다
        if (currentDot && currentDot.isActiveAndEnabled &&
            otherDot && otherDot.isActiveAndEnabled)
        {
            currentState = GameState.wait;
            DotMoveTo(otherDot.GetComponent<Dot>(), currentDot.position);
            DotMoveTo(currentDot, previousPosition);

            yield return new WaitForSeconds(swapDuration); // wait animation
            currentState = GameState.touch;
        }

        currentDot = null;
        otherDot = null;
    }

    private void DotMoveTo(Dot dot, Vector2Int targetPosition) 
    {
        dot.position = targetPosition;
        dot.transform.DOMove((Vector2)targetPosition, swapDuration);
        allDots[targetPosition.x, targetPosition.y] = dot;
    }

    // 매치할 수 있는게 있는가? = 게임 진행이 가능한가?
    private bool CheckCanMatch()
    {
        Vector2Int[] directions = new Vector2Int[] // 상하 좌우
        {
            new Vector2Int(0, 1), // 위쪽 방향
            new Vector2Int(0, -1), // 아래쪽 방향
            new Vector2Int(-1, 0), // 왼쪽 방향
            new Vector2Int(1, 0), // 오른쪽 방향
        };

        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                Dot currentDot = allDots[i, j];
                DotColor color = currentDot.color;

                if(currentDot == null) continue;
                for (int d = 0; d < 4; d++)
                {
                    if (0 <= i + directions[d].x && i + directions[d].x <= width - 1
                        && 0 <= j + directions[d].y && j + directions[d].y <= height - 1)
                    {
                        // 연달아 같은 색이 있는 경우
                        Dot checkDot = allDots[i + directions[d].x, j + directions[d].y];
                        if (checkDot == null || checkDot.color != color) continue;

                        if (CheckSameColor(i + (directions[d].x == 0 ? 1 : 2 * directions[d].x), j + (directions[d].y == 0 ? 1 : 2 * directions[d].y), color)) return true;
                        
                        if (CheckSameColor(i + (directions[d].x == 0 ? -1 : 2 * directions[d].x), j + (directions[d].y == 0 ? -1 : 2 * directions[d].y), color)) return true;
                        
                        if (CheckSameColor(i + 3 * directions[d].x, j + 3 * directions[d].y, color)) return true;
                    }
                }

                for (int d = 0; d < 4; d++)
                {
                    if (0 <= i + 2 * directions[d].x && i + 2 * directions[d].x <= width - 1
                        && 0 <= j + 2 * directions[d].y && j + 2 * directions[d].y <= height - 1)
                    {
                        // 한 칸 띄우고 같은 색이 있는 경우
                        Dot checkDot = allDots[i + 2 * directions[d].x, j + 2 * directions[d].y];
                        if (checkDot == null || checkDot.color != color) continue;
                        
                        if (CheckSameColor(i + (directions[d].x == 0 ? 1 : directions[d].x), j + (directions[d].y == 0 ? 1 : directions[d].y), color)) return true;

                        if (CheckSameColor(i + (directions[d].x == 0 ? -1 : directions[d].x), j + (directions[d].y == 0 ? -1 : directions[d].y), color)) return true;
                    }
                }
            }
        }
        return false;
    }

    private bool CheckSameColor(int x, int y, DotColor color)
    {
        if (0 <= x && x <= width - 1 && 0 <= y && y <= height - 1)
        {
            Dot tempDot = allDots[x, y];
            if (tempDot != null && tempDot.color == color) return true;
        }
        
        return false;
    }

        // { 
        // Dot currentDot;
        // Dot leftDot;
        // Dot rightDot;
        // Dot upDot;
        // Dot downDot;
        // Dot doubleLeftDot;
        // Dot doubleRightDot;
        // Dot doubleUpDot;
        // Dot doubleDownDot;
        // Dot tempDot;

        // for (int i = 0; i < width; i++)
        // {
        //     for (int j = 0; j < height; j++)
        //     {
        //         currentDot = allDots[i, j];
        //         DotColor color = currentDot.color;
                
        //         if(currentDot)
        //         {
        //             if (i > 0)
        //             {
        //                 leftDot = allDots[i - 1, j];
        //                 if (leftDot != null && leftDot.color == color)
        //                 {
        //                     if (i < width - 1 && j < height - 1)
        //                     {
        //                         tempDot = allDots[i + 1, j + 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i < width - 1 && j > 0)
        //                     {
        //                         tempDot = allDots[i + 1, j - 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i < width - 2)
        //                     {
        //                         tempDot = allDots[i + 2, j];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i > 1 && j < height - 1)
        //                     {
        //                         tempDot = allDots[i - 2, j + 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i > 1 && j > 0)
        //                     {
        //                         tempDot = allDots[i - 2, j - 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i > 2)
        //                     {
        //                         tempDot = allDots[i - 3, j];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                 }
        //             }
        //             if (i < width - 1)
        //             {
        //                 rightDot = allDots[i + 1, j];
        //                 if (rightDot != null && rightDot.color == color)
        //                 {
        //                     if (i > 0 && j < height - 1)
        //                     {
        //                         tempDot = allDots[i - 1, j + 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i > 0 && j > 0)
        //                     {
        //                         tempDot = allDots[i - 1, j - 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i > 1)
        //                     {
        //                         tempDot = allDots[i - 2, j];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i < width - 2 && j < height - 1)
        //                     {
        //                         tempDot = allDots[i + 2, j + 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i < width - 2 && j > 0)
        //                     {
        //                         tempDot = allDots[i + 2, j - 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i < width - 3)
        //                     {
        //                         tempDot = allDots[i + 3, j];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                 }
        //             }
        //             if (j < height - 1)
        //             {
        //                 upDot = allDots[i, j + 1];
        //                 if (upDot != null && upDot.color == color)
        //                 {
        //                     if (j > 0 && i < width - 1)
        //                     {
        //                         tempDot = allDots[i + 1, j - 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j > 0 && i > 0)
        //                     {
        //                         tempDot = allDots[i - 1, j - 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j > 1)
        //                     {
        //                         tempDot = allDots[i, j - 2];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j < height - 2 && i < width - 1)
        //                     {
        //                         tempDot = allDots[i + 1, j + 2];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j < height - 2 && i > 0)
        //                     {
        //                         tempDot = allDots[i - 1, j + 2];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j < height - 3)
        //                     {
        //                         tempDot = allDots[i, j + 3];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                 }
        //             }
        //             if (j > 0)
        //             {
        //                 downDot = allDots[i, j - 1];
        //                 if (downDot != null && downDot.color == color)
        //                 {
        //                     if (j < height - 1 && i < width - 1)
        //                     {
        //                         tempDot = allDots[i + 1, j + 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j < height - 1 && i > 0)
        //                     {
        //                         tempDot = allDots[i - 1, j + 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j < height - 2)
        //                     {
        //                         tempDot = allDots[i, j + 2];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j > 1 && i < width - 1)
        //                     {
        //                         tempDot = allDots[i + 1, j - 2];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j > 1 && i > 0)
        //                     {
        //                         tempDot = allDots[i - 1, j - 2];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j > 2)
        //                     {
        //                         tempDot = allDots[i, j - 3];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                 }
        //             }
        //             if (i > 1)
        //             {
        //                 doubleLeftDot = allDots[i - 2, j];
        //                 if (doubleLeftDot != null && doubleLeftDot.color == color)
        //                 {
        //                     if (j < height - 1)
        //                     {
        //                         tempDot = allDots[i - 1, j + 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j > 0)
        //                     {
        //                         tempDot = allDots[i - 1, j - 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                 }
        //             }
        //             if (i < width - 2)
        //             {
        //                 doubleRightDot = allDots[i + 2, j];
        //                 if (doubleRightDot != null && doubleRightDot.color == color)
        //                 {
        //                     if (j < height - 1)
        //                     {
        //                         tempDot = allDots[i + 1, j + 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (j > 0)
        //                     {
        //                         tempDot = allDots[i + 1, j - 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                 }
        //             }
        //             if (j < height - 2)
        //             {
        //                 doubleUpDot = allDots[i, j + 2];
        //                 if (doubleUpDot != null && doubleUpDot.color == color)
        //                 {
        //                     if (i > 0)
        //                     {
        //                         tempDot = allDots[i - 1, j + 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i < width - 1)
        //                     {
        //                         tempDot = allDots[i + 1, j + 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                 }
        //             }
        //             if (j > 1)
        //             {
        //                 doubleDownDot = allDots[i, j - 2];
        //                 if (doubleDownDot != null && doubleDownDot.color == color)
        //                 {
        //                     if (i > 0)
        //                     {
        //                         tempDot = allDots[i - 1, j - 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                     if (i < width - 1)
        //                     {
        //                         tempDot = allDots[i + 1, j - 1];
        //                         if (tempDot != null && tempDot.color == color) return true;
        //                     }
        //                 }
        //             }
        //         }
        //     }
        // }
}
