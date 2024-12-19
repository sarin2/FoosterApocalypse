using System;
using _NM.Core.Character;
using _NM.Core.Common.Combat;
using _NM.Core.Data;
using _NM.Core.Manager;
using _NM.Core.Utils;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using AnimationEvent = _NM.Core.Animation.AnimationEvent;

namespace _NM.Core.Enemy.Type
{
    public class BreadBearEnemy : EnemyBase
    {
        [SerializeField] private GameObject leftAttack;
        [SerializeField] private GameObject rightAttack;
        [SerializeField] private MonsterIKController ikController;
        [SerializeField] private EnemyAnimationController animationController;
        [SerializeField] private SerializableDictionary<TimelineAsset, GameObject> hitEffects;
        [SerializeField] private PlayableDirector playableDirector;
        [SerializeField] private Transform HitObject;
        [SerializeField] private CapsuleCollider playerBlockCollider;
        [SerializeField] private float backStepChance;
        [SerializeField] private float backStepCooldown;
        public bool BackStep { get; private set; }
        private float backstepCur;
        private float[] chanceProbs;
        protected override void OnValidate()
        {
            base.OnValidate();

            ikController = GetComponent<MonsterIKController>();

            if (!playableDirector)
            {
                playableDirector = GetComponent<PlayableDirector>();
            }
            
        } 
        
        
        protected override void Start()
        {
            GetStatFromInfo();
        }

        protected override void Awake()
        {
            base.Awake();
            HitObject.parent = null;
            backStepChance = (float)behaviorTree.GetVariable("BackStepChance").GetValue();
            backStepCooldown = (float)behaviorTree.GetVariable("BackStepCooldown").GetValue();
            chanceProbs = new float[2];
            chanceProbs[0] = backStepChance;
            chanceProbs[1] = 100f - backStepChance;
            backstepCur = 0f;
            GetEnemyData();
            leftAttack = AttackTriggers["Left"];
            rightAttack = AttackTriggers["Right"];
            AnimationEvent["On Left Attack Start"] += _ =>
            {
                leftAttack.SetActive(true);
            };

            AnimationEvent["On Left Attack End"] += _ =>
            {
                leftAttack.SetActive(false);
            };
            
            AnimationEvent["On Right Attack Start"] += _ =>
            {
                rightAttack.SetActive(true);
            };

            AnimationEvent["On Right Attack End"] += _ =>
            {
                rightAttack.SetActive(false);
            };
            AnimationEvent["On Move End"] += _ =>
            {
                Animator.SetBool("Moving",false);
            };
            
            AnimationEvent["On BackStep Start"] += _ =>
            {
                BackStep = true;
            };
            AnimationEvent["On BackStep End"] += _ =>
            {
                BackStep = false;
            };
            /*
            AnimationEvent["LeftWeight"] += animEvent =>
            {
                float weight = animEvent.floatParameter;
                int time = animEvent.intParameter;

                if (EnableIK)
                {
                    ikController.SetWeightWithTime(MonsterIKController.IKPosition.Left,time,weight).Forget();
                }
                
            };
            
            AnimationEvent["RightWeight"] += animEvent =>
            {
                float weight = animEvent.floatParameter;
                int time = animEvent.intParameter;

                if (EnableIK)
                {
                    ikController.SetWeightWithTime(MonsterIKController.IKPosition.Right,time,weight).Forget();
                }
                
            };
            */
            AnimationEvent["OnDeath"] += _ =>
            {
                RemoveEnemy(0.5f,dissolveTime).Forget();
                DropItem(0.5f).Forget();
            };


            Health.onHpChanged += (prev, cur) =>
            {
                if (prev > cur)
                {
                    Health.CurrentHp = cur;
                }
            };
            
        }

        private void Update()
        {
            if (!BackStep &&backstepCur > 0f)
            {
                backstepCur -= Time.deltaTime;
            }
            
        }

        public override void OnHit(HitInfo hitInfo)
        {
            base.OnHit(hitInfo);

            if (BackStep)
            {
                return;
            }
            
            if (health.IsAlive)
            {
                if (backstepCur <= 0f)
                {
                    int result = RandomGenerator.Choose(chanceProbs);
                    if (result == 0)
                    {
                        behaviorTree.SendEvent("BackStep");
                        backstepCur = backStepCooldown;
                        return;
                    }
                }

                
                Vector3 d = Vector3.Cross(hitInfo.Attacker.transform.position, hitInfo.Opponent.transform.position);
               
                if (d.z > 0f)
                {
                    Animator.SetTrigger("RightDamage");
                }
                else
                {
                    Animator.SetTrigger("LeftDamage");
                }

            }

        }

        public override void OnAttack(HitInfo hitInfo)
        {
            base.OnAttack(hitInfo);
            
            if (playableDirector.playableAsset is TimelineAsset timelineAsset &&  hitEffects.TryGetValue(timelineAsset,out GameObject hitObject))
            {
                hitObject.SetActive(true);
                hitObject.transform.SetPositionAndRotation(hitInfo.Point,Quaternion.LookRotation(hitInfo.Normal, Vector3.up));
            }
        }
    }
    
}