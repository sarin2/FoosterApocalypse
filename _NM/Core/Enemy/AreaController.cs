using System;
using System.Collections;
using System.Collections.Generic;
using _NM.Core.Character;
using _NM.Core.Enemy;
using Unity.VisualScripting;
using UnityEngine;

public class AreaController : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;
    [SerializeField] private List<EnemyBase> enemyChilds = new();
    [SerializeField] private bool areaEngaging;
    [SerializeField] private float engagingTime = 10f;
    [SerializeField] private float engagingTotal = 0f;
    [SerializeField] public bool AreaEngaging => areaEngaging;
    [SerializeField] public bool OverEngagingTime => engagingTotal > engagingTime;

    private void OnValidate()
    {
        enemySpawner = GetComponentInParent<EnemySpawner>();
    }

    public void AddEnemyChild(EnemyBase child)
    {
        enemyChilds.Add(child);
    }

    public void SetAreaEngaging(bool state)
    {
        areaEngaging = true;
    }

    public void Update()
    {

        if (areaEngaging)
        {
            foreach (var child in enemyChilds)
            {
                if (!child.IsEnemySpotted)
                {
                    child.SetEnemySpotted(true);
                }
            }
        }
        else
        {
            foreach (var child in enemyChilds)
            {
                if (child.IsEnemySpotted)
                {
                    child.SetEnemySpotted(false);
                }
            }
        }
    
    }

    private void Awake()
    {
        areaEngaging = false;
    }


    public void ResetEngagingTime()
    {
        engagingTotal = 0f;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (transform.parent.TryGetComponent(out EnemySpawner spawner))
        {
            float range =spawner.AreaRange;
            Vector3 myPos = transform.position + Vector3.up * 0.5f;
            Gizmos.DrawWireSphere(myPos, range);
        }

    }
    

}
