using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool instance;
    public int poolSize = 100;
    public GameObject[] dotPrefabs;

    public IObjectPool<GameObject> Pool { get; set; }

    int dotToUse;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        Init();
    }

    private void Init()
    {
        Pool = new ObjectPool<GameObject>(CreatePooledItem, OnTakeFromPool, OnReturnedToPool,
        OnDestroyPoolObject, true, poolSize);

        // 미리 오브젝트 생성 해놓기
        for (int i = 0; i < poolSize; i++)
        {
            Dot dot = CreatePooledItem().GetComponent<Dot>();
            dot.Pool.Release(dot.gameObject);
        }
    }

    // 생성
    private GameObject CreatePooledItem()
    {
        dotToUse = Random.Range(0, dotPrefabs.Length);
        GameObject poolGo = Instantiate(dotPrefabs[dotToUse]);
        poolGo.GetComponent<Dot>().Pool = this.Pool;
        poolGo.GetComponent<Dot>().color = (DotColor)dotToUse;
        return poolGo;
    }

    // 사용
    private void OnTakeFromPool(GameObject poolGo)
    {
        poolGo.SetActive(true);
    }

    // 반환
    private void OnReturnedToPool(GameObject poolGo)
    {
        poolGo.SetActive(false);
    }

    // 삭제
    private void OnDestroyPoolObject(GameObject poolGo)
    {
        Destroy(poolGo);
    }
}
