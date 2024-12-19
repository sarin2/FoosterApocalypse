using System;
using System.Collections;
using System.Collections.Generic;
using _NM.Core.Object;
using UnityEngine;

public class PuttableEffect : MonoBehaviour
{
    [SerializeField] private float duration;
    [SerializeField] private float currentTime;
    [SerializeField] private CapsuleCollider attackTrigger;
    [SerializeField] private float triggerDuration;
    [SerializeField] private Vector3 positionOffset;
    public event Action<GameObject> onPutObject;

    private void OnValidate()
    {
        if (!attackTrigger)
        {
            attackTrigger = GetComponent<CapsuleCollider>();
        }
    }

    private void Start()
    {
        currentTime = 0f;
    }

    private void OnEnable()
    {
        currentTime = 0f;
        attackTrigger.enabled = true;
        transform.position += positionOffset;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;

        if (currentTime > duration)
        {
            onPutObject?.Invoke(gameObject);
        }

        if (attackTrigger.enabled && currentTime > triggerDuration)
        {
            attackTrigger.enabled = false;
        }
    }
}
