using System;
using _NM.Core.Common.Combat;
using _NM.Core.Enemy;
using UnityEngine;
using AnimationEvent = _NM.Core.Animation.AnimationEvent;

namespace _NM.Core.Enemy.Type
{
    public class EnemyCommon : EnemyBase
    {
        [Space(10f)]
        [Header("Animation")]
        [SerializeField] private AnimationEvent animationEvent;
        [SerializeField] private GameObject attackTrigger;
        [SerializeField] private GameObject hitBack;
        private bool isAttacking;
        [SerializeField] private float hitBackTime;
        [SerializeField] private float hitBackTotal;
        [SerializeField] private Vector3 hitBackPos;

        protected override void InitState()
        {
            base.InitState();
            
        }

        protected override void Awake()
        {
            base.Awake();
            InitComponent();
            InitStat();
        }

        private void Start()
        {
            InitState();
        }
        
        
    }
}