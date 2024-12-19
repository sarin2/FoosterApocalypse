using System;
using System.Collections;
using System.Collections.Generic;
using _NM.Core.Enemy;
using UnityEngine;
using UnityEngine.AI;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private NavMeshAgent navMeshAgent;
    [SerializeField] public bool Attacking;

    private void OnValidate()
    {
        if (!animator)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void OnAnimatorMove()
    {
        //navMeshAgent.velocity = animator.deltaPosition / Time.smoothDeltaTime;
    }
}
