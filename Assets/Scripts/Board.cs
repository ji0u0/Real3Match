using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Random = UnityEngine.Random;
using UnityEngine.Events;

public enum GameState
{
    touch, // 터치 가능
    wait // 대기
}

public class Board : MonoBehaviour
{
    public int width;
    public int height;
    private GameState _currentState;

    [Header("Assets")]
    public int initialPoolSize = 10;
    public string searchFolderAddress = "Assets/Prefabs/Dots";
    public List<string> assetAddresses = new List<string>(); // 프리팹 주소
    private List<GameObject> _colorParents = new List<GameObject>(); // 부모 오브젝트

    [Header("Dots")]
    private Dot[,] _allDots;
    private HashSet<Dot> _matchedDots = new HashSet<Dot>();

    [Header("Touch")]
    private Vector2 _initialTouchPosition;
    private Vector2 _finalTouchPosition;
    private Vector2Int _previousPosition;
    private bool _checkTouch;

    [Header("Swap")]
    public float swapDuration = .3f;
    public float termDuration = .1f;
    public float swapResist = 1f;
    
    [Header("Managers")]
    public Score scoreManager;
    public ObjectPool objectPoolManager;

    public event UnityAction EndPlayAction;

    private void Awake()
    {
        EndPlayAction += EndPlay;
        // InitObjectPool();
    }

    void Start()
    {
        // SetUpDots();
    }

    public void InitPlay()
    {
        InitObjectPool();
        SetUpDots();
    }

    public void EndPlay()
    {
        ReturnAllDots();
    }

    private void InitObjectPool()
    {
        for (int i = 0; i < assetAddresses.Count; i++)
        {
            string address = assetAddresses[i];
            DotColor color = (DotColor)i;
            GameObject colorParent = new GameObject(color.ToString() + " Pool");
            _colorParents.Add(colorParent);

            objectPoolManager.InitPool(address, initialPoolSize, colorParent);
        }
    }

    private void ReturnAllDots()
    {
        foreach(var pt in ForAllDots())
        {
            Dot dot = GetDotFromBoard(pt);
            objectPoolManager.ReturnToPool(dot, dot.address);
            SetDotFromBoard(pt, null);
        }
    }

    private Dot GetDotFromPool()
    {
        int randomIndex = Random.Range(0, assetAddresses.Count);
        string address = assetAddresses[randomIndex];

        Dot piece = objectPoolManager.GetObject<Dot>(address);
        piece.transform.SetParent(_colorParents[randomIndex].transform);
        piece.MouseDownAction = MouseDown;
        piece.MouseUpAction = MouseUp;
        piece.address = address;

        return piece;
    }

    private void SetUpDots()
    {
        // Dots 생성
        _allDots = new Dot[width, height];
        foreach(var pt in ForAllDots())
        {
            GameObject piece = GetDotFromPool().gameObject;
            piece.transform.position = (Vector2)pt;
            SetDotFromBoard(pt, piece.GetComponent<Dot>());
            GetDotFromBoard(pt).position = pt;
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
                foreach (Dot dot in FindAllMatches())
                {
                    // match된 dot 삭제
                    Vector2Int tempPosition = dot.position;
                    objectPoolManager.ReturnToPool(dot, dot.address);
                    SetDotFromBoard(tempPosition, null);

                    // 삭제된 dot 다시 생성
                    GameObject piece = GetDotFromPool().gameObject;
                    piece.transform.position = (Vector2)tempPosition;
                    SetDotFromBoard(tempPosition, piece.GetComponent<Dot>());
                    GetDotFromBoard(tempPosition).position = tempPosition;
                }   
            }
            else if (!CheckCanMatch()) // match할 수 있는 dot이 없다면 전체 초기화한다
            {
                foreach(var pt in ForAllDots())
                {
                    // 보드 초기화
                    objectPoolManager.ReturnToPool(GetDotFromBoard(pt), GetDotFromBoard(pt).address);
                    SetDotFromBoard(pt, null);

                    // 다시 채우기
                    GameObject piece = GetDotFromPool().gameObject;
                    piece.transform.position = (Vector2)pt;
                    SetDotFromBoard(pt, piece.GetComponent<Dot>());
                    GetDotFromBoard(pt).position = pt;
                }
            }
            else // 조건을 만족하면 반복을 멈추고 게임을 시작한다
            {
                _currentState = GameState.touch;
                break;
            }
        }
    }

    // Match 프로세스를 시작
    private IEnumerator ProcessMatchesCo(Dot currentDot, Dot otherDot)
    {
        // 매치가 시작되면 터치가 불가능하게 만든다
        _currentState = GameState.wait;
        yield return new WaitForSeconds(swapDuration); // wait animation

        // match를 찾는다
        HashSet<Dot> matchedDots = FindAllMatches();
        
        // match된 dot이 없다면 되돌린다
        if (matchedDots.Count == 0)
        {
            yield return DotSwapCo(currentDot, otherDot);
            yield break;
        }

        while (matchedDots.Count != 0)
        {
            // 점수를 업데이트한다
            UpdateScore(matchedDots);
            // Match된 Dot이 있다면 부순다
            yield return DestroyMatches(matchedDots);
            // 부수고 나면 리필한다
            yield return RefillRowCo();

            // 다시 검사
            matchedDots = FindAllMatches();
        }

        // Match된 Dot이 없다면 Match할 수 있는 조합이 있는지 확인한다
        if (CheckCanMatch())
        {
            // 진행이 가능하다면 터치할 수 있도록 만든다
            _currentState = GameState.touch;
        }
        else
        {
            // 진행이 불가능하다면?
            Debug.Log("cannot match");
            EndPlayAction?.Invoke();
        }
    }

    // 찾는다 : 매치된 dot을 저장해 반환한다
    private HashSet<Dot> FindAllMatches()
    {
        var leftDir = new Vector2Int(-1, 0);
        var rightDir = new Vector2Int(1, 0);
        var upDir = new Vector2Int(0, 1);
        var downDir = new Vector2Int(0, -1);
        
        _matchedDots.Clear();

        foreach(var pt in ForAllDots())
        {
            Dot currentDot = GetDotFromBoard(pt);
            DotColor currentColor = currentDot.color;
            if (currentDot != null)
            {
                if (pt.x > 0 && pt.x < width - 1) // 가로 체크
                {
                    Dot leftDot = GetDotFromBoard(pt + leftDir);
                    Dot rightDot = GetDotFromBoard(pt + rightDir);

                    if (leftDot != null && leftDot.color == currentColor &&
                        rightDot != null && rightDot.color == currentColor)
                    {
                        _matchedDots.Add(currentDot);
                        _matchedDots.Add(leftDot);
                        _matchedDots.Add(rightDot);
                    }
                }
                if (pt.y > 0 && pt.y < height - 1) // 세로 체크
                {
                    Dot upDot = GetDotFromBoard(pt + upDir);
                    Dot downDot = GetDotFromBoard(pt + downDir);

                    if (upDot != null && upDot.color == currentDot.color &&
                        downDot != null && downDot.color == currentDot.color)
                    {
                        _matchedDots.Add(currentDot);
                        _matchedDots.Add(upDot);
                        _matchedDots.Add(downDot);
                    }
                }
            }
        }
        return _matchedDots;
    }

    // 점수를 업데이트 한다
    private void UpdateScore(HashSet<Dot> matchedDots)
    {
        scoreManager.AddScore(CalculateScore(matchedDots));
        scoreManager.UpdateScoreText();
    }

    private int CalculateScore(HashSet<Dot> matchedDots)
    {
        int score = 0;
        int[,] board = new int[width, height];

        foreach (Dot matchedDot in matchedDots)
        {
            board[matchedDot.position.x, matchedDot.position.y] = 1;
        }

        for (int i = 0; i < 2; i++)
        {
            int outerLimit = (i == 0) ? height : width;
            int innerLimit = (i == 0) ? width : height;
            for (int outer = 0; outer < outerLimit; outer++)
            {
                int serialDotCount = 0;
                for (int inner = 0; inner < innerLimit; inner++)
                {
                    int pt = (i == 0) ? board[inner, outer] : board[outer, inner];
                    if (pt == 1)
                    {
                        serialDotCount++;
                    }
                    else
                    {
                        score += scoreManager.returnScoreByMatchCount(serialDotCount);
                        serialDotCount = 0;
                    }
                }
                score += scoreManager.returnScoreByMatchCount(serialDotCount);
            }
        }
        return score;
    }

    // 부순다 : 매치된 Dot을 부순다 + 스코어 업뎃
    private IEnumerator DestroyMatches(HashSet<Dot> matchedDots)
    {
        foreach (Dot dot in matchedDots)
        {
            SetDotFromBoard(dot.position, null);
            objectPoolManager.ReturnToPool(dot, dot.address);
        }

        yield return new WaitForSeconds(termDuration);
    }

    // 채운다 : Dot을 떨어트려 채운다
    private IEnumerator RefillRowCo()
    {
        int nullCount = 0;
        for (int i = 0; i < width; i++)
        {
            // 있는 dots들 떨어트리기
            for (int j = 0; j < height; j++)
            {
                if (_allDots[i, j] == null)
                {
                    nullCount++;
                }
                else if (nullCount > 0)
                {
                    Vector2Int targetPosition = new Vector2Int(i, j - nullCount);
                    DotMoveTo(_allDots[i, j], targetPosition);
                    _allDots[i, j] = null;
                }
            }
            
            // 새 dots 생성해서 떨어트리기
            for (int n = 0; n < nullCount; n++)
            {
                Vector2Int tempPosition = new Vector2Int(i, height + n);
                GameObject piece = GetDotFromPool().gameObject;
                piece.transform.position = (Vector2)tempPosition;
                _allDots[i, height - nullCount + n] = piece.GetComponent<Dot>();

                Vector2Int targetPosition = new Vector2Int(i, height - nullCount + n);
                DotMoveTo(_allDots[i, height - nullCount + n], targetPosition);
            }
            nullCount = 0;
        }
        yield return new WaitForSeconds(swapDuration); // wait animation
    }

    // Mouse control
    public void MouseDown(Vector2 vector)
    {
        if (_currentState == GameState.touch)
        {
            _initialTouchPosition = vector;
            _checkTouch = true;
        }
    }

    public void MouseUp(Vector2 vector, Dot dot)
    {
        if (_checkTouch && _currentState == GameState.touch)
        {
            _finalTouchPosition = vector;
            HandleDotSwap(dot);
        }
        _checkTouch = false;
    }

    // 입력 받은 dot의 위치를 바꾼다 (currentDot <-> otherDot)
    private void HandleDotSwap(Dot dot)
    {
        float distanceX = _finalTouchPosition.x - _initialTouchPosition.x;
        float distanceY = _finalTouchPosition.y - _initialTouchPosition.y;

        if (Mathf.Abs(distanceX) > swapResist || Mathf.Abs(distanceY) > swapResist)
        {
            // Calculate Angle
            float swapAngle = Mathf.Atan2(distanceY, distanceX) * 180 / Mathf.PI;
            
            // Swap Pieces
            var currentDot = dot;
            Vector2Int newPosition = dot.position + JudgeDirection(swapAngle);
            if (0 <= newPosition.x && newPosition.x < width && 0 <= newPosition.y && newPosition.y < height)
            {
                var otherDot = GetDotFromBoard(newPosition);
                _previousPosition = dot.position;
                DotMoveTo(currentDot, newPosition);
                DotMoveTo(otherDot, _previousPosition);

                // 매치 시작 -> 아니면 되돌아가기
                StartCoroutine(ProcessMatchesCo(currentDot, otherDot));
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

    // 매치된 dot이 없다면 되돌린다
    private IEnumerator DotSwapCo(Dot currentDot, Dot otherDot)
    {
        if (currentDot && currentDot.isActiveAndEnabled &&
            otherDot && otherDot.isActiveAndEnabled)
        {
            _currentState = GameState.wait;
            DotMoveTo(otherDot.GetComponent<Dot>(), currentDot.position);
            DotMoveTo(currentDot, _previousPosition);

            yield return new WaitForSeconds(swapDuration); // wait animation
            _currentState = GameState.touch;
        }
    }

    private void DotMoveTo(Dot dot, Vector2Int targetPosition) 
    {
        dot.position = targetPosition;
        dot.transform.DOMove((Vector2)targetPosition, swapDuration);
        SetDotFromBoard(targetPosition, dot);
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

        foreach(var currentPt in ForAllDots())
        {
            Dot currentDot = GetDotFromBoard(currentPt);
            DotColor color = currentDot.color;

            if (currentDot == null) continue;
            foreach (var direction in directions)
            {
                // 연달아 같은 색이 있고 대각선 위,아래나 한칸 떨어져 있는 경우 체크
                if (CheckSameColor(currentPt + direction, color))
                {
                    Vector2Int tempPoint = currentPt + GetDiagonalDirection(direction, 2, 1);
                    if (CheckSameColor(tempPoint, color))
                        return true;

                    tempPoint = currentPt + GetDiagonalDirection(direction, 2, -1);
                    if (CheckSameColor(tempPoint, color))
                        return true;
                    
                    tempPoint = currentPt + direction * 3;
                    if (CheckSameColor(tempPoint, color))
                        return true;
                }
                
                // 한칸 떨어져 같은 색이 있고 그 사이 위,아래에 같은 색이 있는 경우.
                if (CheckSameColor(currentPt + 2 * direction, color))
                {
                    Vector2Int tempPoint = currentPt + GetDiagonalDirection(direction, 1, 1);
                    if (CheckSameColor(tempPoint, color))
                        return true;
                    
                    tempPoint = currentPt + GetDiagonalDirection(direction, 1, -1);
                    if (CheckSameColor(tempPoint, color))
                        return true;
                }
            }
        }
        return false;
    }

    private Vector2Int GetDiagonalDirection(Vector2Int direction, int dist, int diagonalDir)
    {
        return new Vector2Int(
            direction.x == 0 ? diagonalDir : dist * direction.x, 
            direction.y == 0 ? diagonalDir : dist * direction.y);
    }

    private bool IsValidPosition(in Vector2Int checkPoint)
    {
        return 0 <= checkPoint.x && checkPoint.x <= width - 1 &&
               0 <= checkPoint.y && checkPoint.y <= height - 1;
    }

    private bool CheckSameColor(in Vector2Int pt, DotColor color)
    {
        if (IsValidPosition(pt))
        {
            Dot tempDot = _allDots[pt.x, pt.y];
            if (tempDot != null && tempDot.color == color) return true;
        }
        
        return false;
    }

    private IEnumerable<Vector2Int> ForAllDots()
    {
        for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                yield return new Vector2Int(i, j);
            }
        }
    }

    private Dot GetDotFromBoard(in Vector2Int currentPt)
    {
        if (!IsValidPosition(currentPt))
            return null; 
        return _allDots[currentPt.x, currentPt.y];
    }

    private void SetDotFromBoard(in Vector2Int newPt, Dot newDot)
    {
        if (!IsValidPosition(newPt))
            return;
        _allDots[newPt.x, newPt.y] = newDot;
    }
}
