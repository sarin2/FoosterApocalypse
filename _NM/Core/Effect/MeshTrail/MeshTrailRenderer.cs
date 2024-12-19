using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _NM.Core.Object;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

public class MeshTrailRenderer : MonoBehaviour
{
    [SerializeField] private ObjectPool meshTrailPool;
    [SerializeField] private int capacity;

    [Header("메시 제어")]
    [field:LabelText("메시 트레일 사용하는 오브젝트(SK 오브젝트를 지정할 것)"),SerializeField] private Transform rootObejct;
    [field:LabelText("메시 트레일 활성 시간(초)"),SerializeField] private  float activetime = 2f;
    [field:LabelText("메시 나타나는 간격"),SerializeField] private  float meshRefreshRate  = 0.1f;
    [field:LabelText("메시 사라지는 시간"),SerializeField] private  float meshDestroyDelay = 3f;

    [Header("셰이더 제어")] 
    [field:LabelText("트레일 머테리얼"),SerializeField] private Material trailMaterial;
    [field:LabelText("셰이더 변수명"),SerializeField] private  string shaderVarRef ;
    [field:LabelText("셰이더 변수 시간 당 감소 값"),SerializeField] private float shaderVarRate = 0.1f;
    [field:LabelText("셰이더 변수 감소 간격"),SerializeField] private float shaderVarRefreshRate= 0.05f;
    
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    private CancellationTokenSource trailCts;

    private TimeSpan refreshTime;
    private TimeSpan meshRefreshTime;
    private bool initialized;

    private void OnValidate()
    {
        if (!meshTrailPool)
        {
            meshTrailPool = GetComponentInChildren<ObjectPool>();
        }
    }

    private void Awake()
    {
        meshTrailPool.transform.parent = null;
        initialized = false;
    }

    private void Start()
    {
        Initialize();
        gameObject.SetActive(false);
        
    }

    private void OnEnable()
    {
        trailCts = new();
        if (initialized)
        {
            ActiveTrail(activetime).Forget();
        }
        
    }

    private void OnDisable()
    {
        trailCts.Cancel();
        trailCts.Dispose();
    }


    void Initialize()
    {
        refreshTime = TimeSpan.FromSeconds(shaderVarRefreshRate);
        meshRefreshTime = TimeSpan.FromSeconds(meshRefreshRate);
        skinnedMeshRenderers = rootObejct.GetComponentsInChildren<SkinnedMeshRenderer>();

        for (int i = 0; i < capacity; i++)
        {
            var poolObject = meshTrailPool.Get();
            var meshTrail = poolObject.GetComponent<MeshTrail>();
            for (int j = 0; j < skinnedMeshRenderers.Length; j++)
            {
                meshTrail.AddParts(ref skinnedMeshRenderers[j],trailMaterial);
            }
            
            meshTrailPool.Put(poolObject);
        }

        initialized = true;


    }

    private async UniTask ActiveTrail(float duration)
    {
        while (duration > 0f && !trailCts.IsCancellationRequested)
        {
            duration -= meshRefreshRate;
            MeshTrail meshTrail = meshTrailPool.Get<MeshTrail>();
            meshTrail.SpawnWithDelay(meshDestroyDelay);
            for(int i =0; i < skinnedMeshRenderers.Length; i++)
            {
                meshTrail.UpdateParts(ref skinnedMeshRenderers[i],trailMaterial);
                meshTrail.transform.SetPositionAndRotation(rootObejct.position,rootObejct.rotation);
                AnimateMaterialFloat(meshTrail.GetBodyMaterial(skinnedMeshRenderers[i]),0,shaderVarRate).Forget();
            }

            await UniTask.Delay(meshRefreshTime);
        }
        
        gameObject.SetActive(false);
        
    }

    private async UniTask AnimateMaterialFloat(Material mat, float goal, float rate)
    {
        if (shaderVarRef != string.Empty)
        {
            float valueToAnimate = mat.GetFloat(shaderVarRef);
            
            while (valueToAnimate > goal)
            {
                valueToAnimate -= rate;
                mat.SetFloat(shaderVarRef, valueToAnimate );
                await UniTask.Delay(refreshTime);
            }
        }

    }
}
