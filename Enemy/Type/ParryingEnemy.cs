using System;
using _NM;
using _NM.Core.Common.Combat;
using UnityEngine;

namespace _NM.Core.Enemy.Type
{
    public class ParryingEnemy : EnemyBase
    {
        private void Start()
        {
            SetAttackReady(true);
        }

        public override void OnHit(HitInfo hitInfo)
        {
            base.OnHit(hitInfo);
            
            Debug.Log("아얏!");
        }
    }
}
