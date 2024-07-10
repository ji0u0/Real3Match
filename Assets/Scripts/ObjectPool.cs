using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public Dictionary<string, Queue<MonoBehaviour>> pool = new Dictionary<string, Queue<MonoBehaviour>>(); // 프리팹 주소 & 오브젝트

    public void InitPool (string address, int initialSize, GameObject transformParent)
    {
        if (pool.ContainsKey(address))
        {
            Debug.LogWarning("Pool already initialized at " + address);
            return;
        }

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(address);
        if (prefab == null)
        {
            Debug.LogError("Prefab not found at " + address);
            return;
        }

        pool[address] = new Queue<MonoBehaviour>();

        for (int i = 0; i < initialSize; i++)
        {
            GameObject instantiatedObject = Instantiate(prefab);
            instantiatedObject.SetActive(false);
            instantiatedObject.transform.SetParent(transformParent.transform);
            MonoBehaviour pooledInstance = instantiatedObject.GetComponent<MonoBehaviour>();
            pool[address].Enqueue(pooledInstance);
        }
    }

    public T GetObject<T>(string address) where T : MonoBehaviour
    {
        if (pool.ContainsKey(address) && pool[address].Count > 0)
        {
            T pooledInstance = pool[address].Dequeue() as T;
            pooledInstance.gameObject.SetActive(true);
            return pooledInstance;
        }
        else
        {
            // 부족하면 추가
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(address);
            if (prefab != null)
            {
                GameObject instantiatedObject = Instantiate(prefab);
                instantiatedObject.SetActive(true);
                T pooledInstance = instantiatedObject.GetComponent<T>();
                return pooledInstance;
            }
            else
            {
                Debug.LogError("Prefab not found at " + address);
                return null;
            }
        }
    }

    public void ReturnToPool<T>(T objectToReturn, string address) where T : MonoBehaviour
    {
        if (objectToReturn != null)
        {
            objectToReturn.gameObject.SetActive(false);
            if (!pool.ContainsKey(address))
            {
                Debug.LogError("Address error");
            }
            pool[address].Enqueue(objectToReturn);
        }
        else
        {
            Debug.LogError("Object to return is null");
        }
    }
}
