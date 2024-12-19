using System;
using System.Collections;
using System.Collections.Generic;
using _NM.Core.Object;
using _NM.Core.Utils;
using Unity.VisualScripting;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    private float disableTime;
    
    [field: SerializeField] private SerializableDictionary<SkinnedMeshRenderer,MeshTrailParts> bodyParts = new();
    private ObjectPool objectPool;

    private void Awake()
    {
        objectPool = GetComponentInParent<ObjectPool>();

    }

    public void AddParts(ref SkinnedMeshRenderer meshRenderer, Material mat)
    {
        GameObject partsObject = new();

        int materialCount = meshRenderer.materials.Length;
        
        MeshTrailParts meshParts = partsObject.AddComponent<MeshTrailParts>();
        MeshRenderer partsRenderer = meshParts.AddComponent<MeshRenderer>();
        partsRenderer.materials = new Material[materialCount];
        for(int i =0; i < materialCount; i++)
        {
            partsRenderer.materials[i] = mat;
        }
        meshParts.AddComponent<MeshFilter>();
        
        meshParts.UpdateComponent();
        
        partsObject.transform.SetParent(transform);
        partsObject.transform.position = transform.position;
        partsObject.transform.localRotation = Quaternion.Euler(-90, 0, 0);
        
        meshParts.UpdateMesh(ref meshRenderer,mat);
        bodyParts.TryAdd(meshRenderer,meshParts);
        
    }

    private void Update()
    {
        disableTime -= Time.deltaTime;

        if (disableTime < 0f)
        {
            objectPool.Put(gameObject);
        }
    }

    public void UpdateParts(ref SkinnedMeshRenderer meshRenderer, Material mat)
    {
        bodyParts[meshRenderer].UpdateMesh(ref meshRenderer,mat);
    }

    public Material GetBodyMaterial(SkinnedMeshRenderer meshRenderer)
    {
        return bodyParts[meshRenderer].Renderer.material;

    }
    
    
    public void SpawnWithDelay(float Time)
    {
        disableTime = Time;
    }
}
