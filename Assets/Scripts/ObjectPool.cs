using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{
    public int poolSize = 20;
    public GameObject[] dotPrefabs;
    private Dictionary<DotColor, Queue<GameObject>> pool = new Dictionary<DotColor, Queue<GameObject>>();

    private void Awake()
    {
        foreach (DotColor color in System.Enum.GetValues(typeof(DotColor)))
        {
            pool[color] = new Queue<GameObject>();
        }

        // 미리 풀에 dots 생성
        for (int i = 0; i < dotPrefabs.Length; i++)
        {
            for (int j = 0; j < poolSize; j++)
            {
                GameObject piece = Instantiate(dotPrefabs[i]);
                piece.GetComponent<Dot>().color = (DotColor)i;
                piece.SetActive(false);
                pool[(DotColor)i].Enqueue(piece);
            }
        }
    }

    public GameObject GetObject(DotColor color)
    {
        GameObject piece;

        if (pool[color].Count > 0)
        {
            piece = pool[color].Dequeue();
            piece.SetActive(true);
            return piece;
        }
        else
        {
            // 부족하면 추가
            piece = Instantiate(dotPrefabs[(int)color]);
            piece.GetComponent<Dot>().color = color;
            piece.SetActive(true);
            return piece;
        }
    }

    public void PoolObject(GameObject piece)
    {
        int color = (int)piece.GetComponent<Dot>().color;
        piece.SetActive(false);
        pool[(DotColor)color].Enqueue(piece);
    }
}
