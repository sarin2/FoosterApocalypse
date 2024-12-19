using System;
using System.Collections;
using System.Collections.Generic;
using _NM.Core.Object;
using UnityEngine;
using UnityEngine.Rendering;

public class EffectManager : MonoBehaviour
{

    [SerializeField] private SerializedDictionary<string,GameObject> effectPrefabs;
    [SerializeField] private SerializedDictionary<string,ObjectPool> effectPools;

    private void Start()
    {
        foreach (var effectPrefab in effectPrefabs)
        {
            effectPools.Add(effectPrefab.Key,ObjectPool.GetPool(effectPrefab.Value));
        }
    }


    public ParticleSystem GetEffect(string effectName)
    {
        if (IsValid(effectName))
        { 
            return effectPools[effectName].Get<ParticleSystem>();
        }

        return null;
    }

    public void ReleaseEffect(string effectName,ParticleSystem effect)
    {
        if (IsValid(effectName,effect))
        {
            effectPools[effectName].Put(effect);
        }

    }

    private bool IsValid(string effectName)
    {
        effectPrefabs.TryGetValue(effectName, out GameObject effect);
        if (effect == null)
        {
            Debug.LogError("해당 이펙트가 오브젝트 풀에 등록되어 있지 않습니다.\n 오류 발생한 오브젝트 : " + gameObject.name);
            return false;
        }
        
        if (effect.GetComponent<ParticleSystem>() == null)
        {
            Debug.LogError("해당 오브젝트 프리팹에 파티클 시스템이 없습니다.\n 오류 발생한 오브젝트 : " + gameObject.name);
            return false;
        }

        return true;
    }

    private bool IsValid(string effectName,ParticleSystem effect)
    {
        effectPrefabs.TryGetValue(effectName, out GameObject effectPrefab);
        if (IsValid(effectName) && effectPrefab != null)
        {
            if (effectPrefab.GetInstanceID() != effect.gameObject.GetInstanceID())
            {
                Debug.LogError("반환하려는 이펙트가 오브젝트 풀의 이펙트와 일치하지 않습니다.\n 오류 발생한 오브젝트 : " + gameObject.name);
                return false;
            }
            
            return true;
        }

        return false;
    }
}
