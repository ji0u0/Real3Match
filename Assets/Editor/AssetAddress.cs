using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class AssetAddress : Editor
{
    [MenuItem("Tools/Update Asset Addresses")]
    private static void UpdateAssetAddresses()
    {
        Board board = FindObjectOfType<Board>();
        if (board == null)
        {
            Debug.LogError("ObjectPool component not found in the scene.");
            return;
        }

        board.dotAddresses = new List<string>();
        board.DotPrefabs = new List<GameObject>();
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] {board.searchFolderAddress});

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Dot dot = prefab.GetComponent<Dot>();

            if (dot != null)
            {
                board.dotAddresses.Add(path);
                board.DotPrefabs.Add(prefab);
                // Debug.Log(board.assetAddresses[dot.color]);
                EditorUtility.SetDirty(board);
            }
        }

        Debug.Log("Asset addresses updated");
    }
}
