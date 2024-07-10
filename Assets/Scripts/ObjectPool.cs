using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private readonly Dictionary<string, Queue<MonoBehaviour>> _pool = new Dictionary<string, Queue<MonoBehaviour>>(); // 프리팹 주소 & 오브젝트
    
    public void InitPool (string address, int initialSize, GameObject transformParent)
    {
        if (_pool.ContainsKey(address))
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

        _pool[address] = new Queue<MonoBehaviour>();

        for (int i = 0; i < initialSize; i++)
        {
            GameObject instantiatedObject = Instantiate(prefab);
            instantiatedObject.SetActive(false);
            instantiatedObject.transform.SetParent(transformParent.transform);
            MonoBehaviour pooledInstance = instantiatedObject.GetComponent<MonoBehaviour>();
            _pool[address].Enqueue(pooledInstance);
        }
    }

    public T GetObject<T>(string address) where T : MonoBehaviour
    {
        if (_pool.ContainsKey(address) && _pool[address].Count > 0)
        {
            T pooledInstance = _pool[address].Dequeue() as T;
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
            if (!_pool.ContainsKey(address))
            {
                Debug.LogError("Address error");
            }
            _pool[address].Enqueue(objectToReturn);
        }
        else
        {
            Debug.LogError("Object to return is null");
        }
    }
}
