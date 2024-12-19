using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshRotator : MonoBehaviour
{
    [SerializeField] private bool rotateX;
    [SerializeField] private bool rotateY;
    [SerializeField] private bool rotateZ;

    [SerializeField] private float angle;

    [SerializeField] private float angleX;
    [SerializeField] private float angleY;
    [SerializeField] private float angleZ;

    [SerializeField] private Vector3 initialRotation;
    [SerializeField] private float timeToRotate;
    [SerializeField] private float currentTime;
    [SerializeField] private bool reverse;
    
    private void Awake()
    {
        angle = 0f;
        currentTime = 0f;

        initialRotation = transform.eulerAngles;
    }

    private void Update()
    {
        currentTime += Time.deltaTime;
        
        angle = currentTime/timeToRotate * 360f;
        
        if (angle >= 360f)
        {
            currentTime = 0f;
            angle = 0f;
        }
        
        angleX = rotateX ? (reverse ? -1 : 1) * angle : initialRotation.x;
        angleY = rotateY ? (reverse ? -1 : 1) * angle : initialRotation.y;
        angleZ = rotateZ ? (reverse ? -1 : 1) * angle : initialRotation.z;
        
        
        transform.rotation = Quaternion.Euler(angleX,angleY,angleZ);
    }
}
