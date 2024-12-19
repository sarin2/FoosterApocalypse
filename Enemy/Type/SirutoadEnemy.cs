using System;
using System.Threading;
using _NM.Core.Common.Combat;
using _NM.Core.Enemy.AI.EnemyAI;
using _NM.Core.Manager;
using _NM.Core.UI.Combat;
using _NM.Core.Utils.Extension;
using BehaviorDesigner.Runtime;
using Cysharp.Threading.Tasks;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using AnimationEvent = _NM.Core.Animation.AnimationEvent;

namespace _NM.Core.Enemy.Type
{
    public class SirutoadEnemy : EnemyBase
    {
        
        [Space(10f)]
        [Header("Animation")]
        [SerializeField] private AnimationEvent animationEvent;
        [SerializeField] private GameObject attackTrigger;
        [SerializeField] private GameObject tailAttackTrigger;
        [SerializeField] private GameObject hitBack;
        [field: Range(0,360),SerializeField] public float SweepAngle { get; private set; }
        [field: Range(0,100), SerializeField] public float SweepRadius { get; private set; }
        [SerializeField] private float hitBackTime;
        [SerializeField] private float hitBackTotal;
        [SerializeField] private Vector3 hitBackPos;
        
        protected override void OnValidate()
        {
            base.OnValidate();

            if (!animationEvent) animationEvent = GetComponent<AnimationEvent>();
            if (!behaviorTree) behaviorTree = GetComponent<BehaviorTree>();
        }

        protected override void Awake()
        {
            base.Awake();
            BlockPlayer = true;
            InitComponent();
            GetEnemyData();
            hitBackTotal = 0;
            hitBackTime = 0.5f;
            animationEvent["AnimationDamageEnded"] += _ =>
            {
                
            };

            animationEvent["OnAttack"] += param =>
            {
                AttackTriggers[param.intParameter.ToString()].SetActive(true);
            };

            animationEvent["OnAttackEnd"] += param =>
            {
                AttackTriggers[param.intParameter.ToString()].SetActive(false);
            };

            animationEvent["OnTailAttack"] += _ =>
            {
                tailAttackTrigger.SetActive(true);
            };
                
            animationEvent["OnTailAttackEnd"] += _ =>
            {
                NavMeshAgent.ResetPath();
                NavMeshAgent.velocity = Vector3.zero;
                BlockPlayer = false;
                tailAttackTrigger.SetActive(false);
                attackDelay = 1f;
            };

            animationEvent["OnDeath"] += _ =>
            {
                RemoveEnemy(0.5f,dissolveTime).Forget();
                DropItem(0.5f).Forget();
            };

        }
        

        private void FixedUpdate()
        {
            if (IsDamaged && hitBackTotal < hitBackTime)
            {
                hitBackTotal += Time.fixedDeltaTime;
                NavMeshAgent.speed = 0;
                transform.position = hitBackPos;
            }
        }
        

        public override void OnAttack(HitInfo hitInfo)
        {
            VfxManager.I.Spawn(Vfx.MonsterAttack, hitInfo.Point, Quaternion.LookRotation(hitInfo.Normal, Vector3.up), 1f);
        }

        private void Update()
        {
            if (Health.IsAlive && BlockPlayer && this.Distance(Character.Character.Local) < 1f)
            {
                Vector3 dir = (transform.position - Character.Character.Local.transform.position).normalized;
                Vector3 yFix = new Vector3(dir.x, 0, dir.z);
                transform.position += yFix * (Time.deltaTime * 2f);
            }
            if (!IsGuard)
            {
                if (GuardGauge >= DataInfo.Guard && guardRecoveryTotal != 0f)
                {
                    guardRecoveryTotal = 0f;
                }
                if (GuardGauge < DataInfo.Guard && guardRecoveryTime > guardRecoveryTotal)
                {
                    guardRecoveryTotal += Time.deltaTime;
                }
                else if (GuardGauge < DataInfo.Guard && guardRecoveryTime <= guardRecoveryTotal)
                {
                    AddGuardGauge(DataInfo.GuardRecovery * Time.deltaTime);
                }
            }
            if (attackDelay > 0)
            {
                attackDelay -= Time.deltaTime;
            }
                
        }
        
        
        public Vector3 GetHitBack()
        {
            return hitBack.transform.position;
        }
        
        protected override void OnEnable()
        {
            base.OnEnable();
            InitState();
            NavMeshAgent.enabled = true;
            InitStat();
            GetStatFromInfo();
        }
        
        
    }
}

