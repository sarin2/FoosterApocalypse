using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using Random = UnityEngine.Random;

public class EffectRotator : MonoBehaviour
{
    [SerializeField] private float Angle;

    private void OnEnable()
    {
        float halfAngle = Angle * 0.5f;
        float randomValue = Random.Range(-Angle, halfAngle);
        transform.localRotation = Quaternion.Euler(-90, 0, randomValue );
    }
    
    
}
