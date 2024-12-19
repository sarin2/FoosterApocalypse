using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace _NM.Editor
{
    public class MeshToPrefab : MonoBehaviour
    {
        [MenuItem("Assets/NM/Mesh To Prefab", priority = 0)]
        public static void CreatePrefab()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is GameObject mesh)
                {
                    var path = AssetDatabase.GetAssetPath(obj);
                    var directory = Path.GetDirectoryName(path);
                    var parentObject = new GameObject(obj.name);
                    Instantiate(mesh, parentObject.transform);

                    var prefabName = obj.name;
                    var underBarIndex = obj.name.IndexOf("_", StringComparison.Ordinal);
                    if (underBarIndex < 4)
                    {
                        prefabName = obj.name.Substring(underBarIndex + 1);
                    }

                    PrefabUtility.SaveAsPrefabAsset(parentObject, $@"{directory}\{prefabName}.prefab");
                    
                    DestroyImmediate(parentObject);
                }
            }
            AssetDatabase.Refresh();
        }
    }
}
