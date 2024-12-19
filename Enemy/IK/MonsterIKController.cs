using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Rendering;

using UnityEngine;
using UnityEngine.Animations.Rigging;
using Cysharp.Threading.Tasks;

public class MonsterIKController : MonoBehaviour
{
    public enum IKPosition
    {
        Left,
        Right   
    }
    
    [SerializeField] private Rig enemyRig;

    [SerializeField] private ChainIKConstraint LeftToeIK;
    [SerializeField] private ChainIKConstraint RightToeIK;

    private void Awake()
    {
        enemyRig = GetComponent<Rig>();

        if (LeftToeIK == null || RightToeIK == null)
        {
            Debug.LogError("ChainIKConstraints are not assigned.");
        }
    }
    
    public void SetWeight(IKPosition position, float weight = 1f)
    {
        ChainIKConstraint currentConstraint = GetCurrentConstraint(position);
        currentConstraint.weight = weight;
    }

    public void SetChainRotationWeight(IKPosition position, float chainWeight = 1f)
    {
        ChainIKConstraint currentConstraint = GetCurrentConstraint(position);
        currentConstraint.data.chainRotationWeight = chainWeight;
    }
    
    public void SetTipWeight(IKPosition position, float tipWeight = 1f)
    {
        ChainIKConstraint currentConstraint = GetCurrentConstraint(position);
        currentConstraint.data.tipRotationWeight = tipWeight;
    }

    public async UniTaskVoid SetWeightWithTime(IKPosition position, int milliseconds, float weight = 1f)
    {
        ChainIKConstraint currentConstraint = GetCurrentConstraint(position);
        await LerpWeightWithTime(
            () => currentConstraint.weight,
            value => currentConstraint.weight = value,
            weight,
            milliseconds);
    }

    public async UniTaskVoid SetChainRotationWeightWithTime(IKPosition position, int milliseconds, float chainWeight = 1f)
    {
        ChainIKConstraint currentConstraint = GetCurrentConstraint(position);
        await LerpWeightWithTime(
            () => currentConstraint.data.chainRotationWeight,
            value => currentConstraint.data.chainRotationWeight = value,
            chainWeight,
            milliseconds);
    }
    
    public async UniTaskVoid SetTipWeightWithTime(IKPosition position, int milliseconds, float tipWeight = 1f)
    {
        ChainIKConstraint currentConstraint = GetCurrentConstraint(position);
        await LerpWeightWithTime(
            () => currentConstraint.data.tipRotationWeight,
            value => currentConstraint.data.tipRotationWeight = value,
            tipWeight,
            milliseconds);
    }
    
    private async UniTask LerpWeightWithTime(
        Func<float> getCurrentValue,
        Action<float> setCurrentValue,
        float targetValue,
        int milliseconds)
    {
        int time = milliseconds;
        int currentTime = 0;
        float currentValue = getCurrentValue();

        while (currentTime < time)
        {
            float t = (float)currentTime / time;
            setCurrentValue(Mathf.Lerp(currentValue, targetValue, t));
            currentTime += (int)(Time.deltaTime * 1000);
            await UniTask.Yield();
        }
        setCurrentValue(targetValue);
    }

    private ChainIKConstraint GetCurrentConstraint(IKPosition position)
    {
        return (position == IKPosition.Left) ? LeftToeIK : RightToeIK;
    }
}