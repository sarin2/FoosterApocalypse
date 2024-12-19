using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class MeshCombiner : MonoBehaviour
{
    public Transform root;
    public Transform Parent;
    public Material material;
    public bool DeactivateParentAfterMerge = true;
    public bool DestroyParentAfterMerge = false;
    public List<GameObject> destroyObject;
    
    [ContextMenu("Merge")]
    public void MergeMeshes()
    {
        destroyObject = new List<GameObject>();
        root = gameObject.transform;
        MeshFilter[] meshFilters;
        List<CombineInstance> combineList = new List<CombineInstance>();

        for (int parents = 0; parents < root.childCount; parents++)
        {
            Transform parent = root.GetChild(parents);
            if (parent.GetComponentInChildren<MeshFilter>())
            {
                meshFilters = parent.GetComponentsInChildren<MeshFilter>();
                for (int i = 0; i < meshFilters.Length; i++)
                {
                    if (meshFilters[i].sharedMesh != null) 
                    {
                        CombineInstance combineInstance = new CombineInstance
                        {
                            mesh = meshFilters[i].sharedMesh,
                            transform = meshFilters[i].transform.localToWorldMatrix
                        };
                        if (meshFilters[i].TryGetComponent(out Renderer render) && render.sharedMaterial == material)
                        {
                            combineList.Add(combineInstance);
                            destroyObject.Add(meshFilters[i].gameObject);
                        }

                    }
                }
            }
            else
            {
                for (int child = 0; child < parent.childCount; child++)
                {
                    meshFilters = parent.GetChild(child).GetComponentsInChildren<MeshFilter>();
                    
                    for (int i = 0; i < meshFilters.Length; i++)
                    {
                        if (meshFilters[i].sharedMesh != null) 
                        {
                            CombineInstance combineInstance = new CombineInstance
                            {
                                mesh = meshFilters[i].sharedMesh,
                                transform = meshFilters[i].transform.localToWorldMatrix
                            };
                            if (meshFilters[i].GetComponent<Renderer>().sharedMaterial == material)
                            {
                                combineList.Add(combineInstance);
                                destroyObject.Add(meshFilters[i].gameObject);
                                
                            }
                        }
                    }
                }
            }
        }
        
 

 
        GameObject combinedObject = new GameObject("Combined Mesh");
        combinedObject.AddComponent<MeshFilter>();
        combinedObject.AddComponent<MeshRenderer>();
        combinedObject.GetComponent<MeshFilter>().sharedMesh = new Mesh();
        combinedObject.GetComponent<MeshFilter>().sharedMesh.indexFormat = IndexFormat.UInt32;
        combinedObject.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combineList.ToArray());
        combinedObject.GetComponent<MeshRenderer>().material = material;
        combinedObject.transform.parent = root;
        combinedObject.name = root.name;
        foreach (var dObject in destroyObject)
        {
            if (dObject?.GetComponent<Collider>())
            {
                if (dObject.TryGetComponent(out Renderer objectRenderer))
                {
                    objectRenderer.enabled = false;
                }
            }
            else
            {
                if (DestroyParentAfterMerge)
                {
                    DestroyImmediate(dObject);
                }
                else
                {
                    dObject.SetActive(false);
                }
            }
            
        }
        
        if (DestroyParentAfterMerge)
        {
            DestroyImmediate(Parent);
        }
        
        destroyObject.Clear();
    
    }
 
}