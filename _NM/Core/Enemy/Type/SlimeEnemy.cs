using System;
using _NM.Core.Common.Combat;
using _NM.Core.Enemy;
using _NM.Core.Manager;
using _NM.Core.Utils;
using _NM.Core.Utils.Extension;
using BehaviorDesigner.Runtime;
using UnityEngine;
using AnimationEvent = _NM.Core.Animation.AnimationEvent;

namespace _NM.Core.Enemy
{
    public class SlimeEnemy : EnemyBase
    {
        
        private static readonly int Opacity = Shader.PropertyToID("_Opacity");

        protected override void OnValidate()
        {
            base.OnValidate();
            
            if (!behaviorTree) behaviorTree = GetComponent<BehaviorTree>();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InitComponent();
            InitStat();
            InitState();
            NavMeshAgent.enabled = true;
            GetStatFromInfo();
        }

        protected override void Awake()
        {
            base.OnValidate();
            base.Awake();
            GetEnemyData();

            transform.localPosition = Vector3.zero;

            AnimationEvent["AnimationDamageEnded"] += _ =>
            {
                if (Vector3.Distance(BeginPosition, transform.position) > 1f)
                {
                    //CurrentState = EnemyState.BackToHome;
                }
            };

            AnimationEvent["OnDeath"] += _ =>
            {
                RemoveEnemy(0.5f,dissolveTime).Forget();
                DropItem(0.5f).Forget();
                NavMeshAgent.ResetPath();
                NavMeshAgent.velocity = Vector3.zero;
                BlockPlayer = false;
            };

            AnimationEvent["OnAttack"] += @event =>
            {
                AttackTriggers[@event.intParameter.ToString()].SetActive(true);
            };

            AnimationEvent["OnAttackEnd"] += @event =>
            {
                AttackTriggers[@event.intParameter.ToString()].SetActive(false);
            };
        }

        protected override void Start()
        {
            base.Start();
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
            
            if (GuardGauge >= DataInfo.Guard && guardRecoveryTotal != 0f)
            {
                guardRecoveryTotal = 0f;
            }
            
            if (!IsGuard)
            {
                if (GuardGauge < DataInfo.Guard)
                {
                    if (guardRecoveryTime > guardRecoveryTotal)
                    {
                        guardRecoveryTotal += Time.deltaTime;
                    }

                    if (guardRecoveryTime <= guardRecoveryTotal)
                    {
                        AddGuardGauge(DataInfo.GuardRecovery * Time.deltaTime);
                    }
                    
                }
            }

            
            if (attackDelay > 0)
            {
                attackDelay -= Time.deltaTime;
            }
        }
        

        public override void OnHit(HitInfo hitInfo)
        {
            base.OnHit(hitInfo);
            
            transform.LookAt(hitInfo.Attacker.transform);
            
            transform.rotation = Quaternion.Euler(0, transform.eulerAngles.y, 0);
        }
        
    }
}
