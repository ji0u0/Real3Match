using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class ObjectPool : MonoBehaviour
{   
    public static ObjectPool instance;
    public int poolSize = 100;
    public GameObject[] dotPrefabs;
    private Dictionary<DotColor, Queue<GameObject>> poolDictionary;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(this.gameObject);

        InitializePool();
    }

    private void InitializePool()
    {
        poolDictionary = new Dictionary<DotColor, Queue<GameObject>>();

        for (int i = 0; i < dotPrefabs.Length; i++)
        {
            Queue<GameObject> objectPool = new Queue<GameObject>();

            for (int j = 0; j < poolSize; j++)
            {
                GameObject obj = Instantiate(dotPrefabs[i]);
                obj.SetActive(false);
                objectPool.Enqueue(obj);
            }

            poolDictionary.Add((DotColor)i, objectPool);
        }
    }

    public GameObject GetDotFromPool(DotColor dotColor)
    {
        if (poolDictionary[dotColor].Count == 0)
        {
            GameObject newObj = Instantiate(dotPrefabs[(int)dotColor]);
            return newObj;
        }

        GameObject objectToSpawn = poolDictionary[dotColor].Dequeue();
        objectToSpawn.SetActive(true);
        return objectToSpawn;
    }

    public void ReturnDotToPool(GameObject dot, DotColor dotColor)
    {
        dot.SetActive(false);
        poolDictionary[dotColor].Enqueue(dot);
    }
}
