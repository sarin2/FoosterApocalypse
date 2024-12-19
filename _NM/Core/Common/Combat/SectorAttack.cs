using System;
using System.Collections;
using System.Collections.Generic;
using _NM.Core.Character;
using _NM.Core.Common.Combat;
using UnityEngine;


public class SectorAttack : Attack
{

    [Range(0, 360)] [SerializeField] private float angle;

    [Header("Target")] [SerializeField] private LayerMask targetMask;
    [SerializeField] private LayerMask obstacleMask;

    [SerializeField]
    private bool isOutSectorAttack;

    private void Update()
    {
        Vector3 pos = (Character.Local.transform.position - transform.position);
        Vector3 direction = pos.normalized;
    }

    protected override void OnTriggerEnter(Collider other)
    {
        Vector3 pos = (other.transform.position - transform.position);
        Vector3 direction = pos.normalized;
        if (Vector3.Angle(transform.forward, direction) < angle * 0.5f)
        {
            float dist = pos.magnitude;
            if (!Physics.Raycast(transform.position, direction, dist, obstacleMask))
            {
                if (!isOutSectorAttack)
                {
                    base.OnTriggerEnter(other);
                }
            }
        }
        else if (isOutSectorAttack)
        {
            base.OnTriggerEnter(other);
        }

    }

}
