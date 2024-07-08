using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AssetAddress : MonoBehaviour
{
    [MenuItem("Tools/Update Asset Addresses")]
    private static void UpdateAssetAddresses()
    {
        ObjectPool objectPool = FindObjectOfType<ObjectPool>();
        if (objectPool == null)
        {
            Debug.LogError("ObjectPool component not found in the scene.");
            return;
        }

        string[] searchFolders = new[] {
            "Assets/Prefabs/Dots"
        };

        objectPool.assetAddresses = new Dictionary<DotColor, string>();

        string[] guids = AssetDatabase.FindAssets("t:Prefab", searchFolders);
        
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Dot dot = prefab.GetComponent<Dot>();

            if (dot != null)
            {
                // objectPool.assetAddresses.Add(dot.color, path);
                objectPool.assetAddresses[dot.color] = path;
                Debug.Log(objectPool.assetAddresses[dot.color]);
                EditorUtility.SetDirty(objectPool);
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log("Asset addresses updated");
    }
}
