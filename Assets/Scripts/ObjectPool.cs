using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public int poolSize = 20;
    public Dictionary<DotColor, string> assetAddresses = new Dictionary<DotColor, string>(); // 색 & 프리팹 주소 쌍
    private Dictionary<DotColor, GameObject> colorParents = new Dictionary<DotColor, GameObject>(); // 색 & 부모 오브젝트 쌍
    private Dictionary<string, Queue<Dot>> pool = new Dictionary<string, Queue<Dot>>(); // 프리팹 주소 & 풀 쌍

    private void Awake()
    {
        // Asset 경로로 가져오기
        string[] searchFolders = new[] {"Assets/Prefabs/Dots"};
        string[] guids = AssetDatabase.FindAssets("t:Prefab", searchFolders);

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Dot dot = prefab.GetComponent<Dot>();

            if (dot != null)
            {
                assetAddresses[dot.color] = path;
                Debug.Log(assetAddresses[dot.color]);
            }
        }

        // 미리 풀에 dots 생성
        foreach (KeyValuePair<DotColor, string> kvp in assetAddresses)
        {
            DotColor color = kvp.Key;
            string address = kvp.Value;
            pool[address] = new Queue<Dot>();

            GameObject colorParent = new GameObject(color.ToString() + " Pool");
            colorParents[color] = colorParent;

            for (int j = 0; j < poolSize; j++)
            {
                GameObject piece = Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(address));
                piece.transform.parent = colorParent.transform;
                piece.GetComponent<Dot>().color = color;
                piece.SetActive(false);
                pool[address].Enqueue(piece.GetComponent<Dot>());
            }
        }
    }

    public GameObject GetObject()
    {
        DotColor color = (DotColor)Random.Range(0, assetAddresses.Count);
        string address = assetAddresses[color];
        GameObject piece;

        if (pool.ContainsKey(address) && pool[address].Count > 0)
        {
            piece = pool[address].Dequeue().gameObject;
            piece.SetActive(true);
            return piece;
        }
        else
        {
            // 부족하면 추가
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(address);
            if (prefab != null)
            {
                piece = Instantiate(prefab, colorParents[color].transform);
                piece.GetComponent<Dot>().color = color;
                piece.SetActive(true);
                return piece;
            }
            else
            {
                Debug.LogError("Prefab not found at " + address);
                return null;
            }
        }
    }

    public void ReturnToPool(GameObject piece)
    {
        string address = assetAddresses[piece.GetComponent<Dot>().color];
        piece.SetActive(false);
        pool[address].Enqueue(piece.GetComponent<Dot>());
    }
}
