using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public Dictionary<string, Queue<MonoBehaviour>> pool = new Dictionary<string, Queue<MonoBehaviour>>(); // 프리팹 주소 & 풀 쌍

    public T GetObject<T>(string address) where T : MonoBehaviour
    {
        if (pool.ContainsKey(address) && pool[address].Count > 0)
        {
            T piece = pool[address].Dequeue() as T;
            piece.gameObject.SetActive(true);
            return piece;
        }
        else
        {
            // 부족하면 추가
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(address);
            if (prefab != null)
            {
                GameObject instantiatedObject = Instantiate(prefab);
                T piece = instantiatedObject.GetComponent<T>();
                piece.gameObject.SetActive(true);
                return piece;
            }
            else
            {
                Debug.LogError("Prefab not found at " + address);
                return null;
            }
        }
    }

    public void ReturnToPool<T>(T obj, string address) where T : MonoBehaviour
    {
        if (obj != null)
        {
            obj.gameObject.SetActive(false);
            if (!pool.ContainsKey(address))
            {
                Debug.LogError("Address error");
            }
            pool[address].Enqueue(obj);
        }
        else
        {
            Debug.LogError("Object to return is null");
        }
    }
}
