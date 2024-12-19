using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrailParts : MonoBehaviour
{
    [field:SerializeField] public MeshRenderer Renderer { get; private set; }
    [field:SerializeField] public MeshFilter Filter { get; private set; }
    [SerializeField] private Mesh mesh;
    [SerializeField] private Material partsMat;
    

    private void Awake()
    {
        mesh = new();
    }

    public void UpdateMesh(ref SkinnedMeshRenderer meshRenderer, Material mat)
    {
        meshRenderer.BakeMesh(mesh);
        Filter.mesh = mesh;
        Renderer.material = mat;
        partsMat = Renderer.material;

        Material[] materials = Renderer.materials;
        for (int i = 0; i < Renderer.materials.Length; i++)
        {
            materials[i] = Renderer.material;
        }

        Renderer.materials = materials;


    }
    
    public void UpdateComponent()
    {
        Renderer = GetComponent<MeshRenderer>();
        Filter = GetComponent<MeshFilter>();
    }
}
