using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyEffect : MonoBehaviour
{
    private Transform parent;
    private void OnEnable()
    {
        parent = transform.parent;
        transform.parent = null;
    }

    private void Update()
    {
        //transform.rotation = Quaternion.Euler(-90, 0, parent.eulerAngles.y);
    }
}
