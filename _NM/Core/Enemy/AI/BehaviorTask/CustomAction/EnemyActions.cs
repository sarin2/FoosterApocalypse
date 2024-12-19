using System;
using System.Linq;
using _NM.Core.Common.Combat;
using _NM.Core.Data;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using Sirenix.Utilities;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using Random = UnityEngine.Random;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction
{

    [TaskCategory("Enemy Action")]
    public class PatrolAction : EnemyActionBase
    {
        public override void OnAwake()
        {
            base.OnAwake();
            base.OnInitialize();
            enemyBase.SetAreaRange(5.0f);
            enemyBase.BeginPosition = transform.position;
            navMesh.stoppingDistance = 0.01f;

        }

        public override void OnStart()
        {
            base.OnStart();
        }

        public override TaskStatus OnUpdate()
        {
            if (navMesh.remainingDistance <= navMesh.stoppingDistance)
            {
                navMesh.SetDestination(enemyBase.MoveRandomPosition());
                return TaskStatus.Success;
            }
            return TaskStatus.Running;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            if (enemyBase.IsEnemySpotted)
            {
                enemyBase.NoticeEnemySpotted();
            }
        }
    }
    
    [TaskCategory("Enemy Action")]
    public class ChaseAction : EnemyActionBase
    {
        public float StoppingDist;
        public float AttackRange;
        public override void OnAwake()
        {
            base.OnAwake();
            
        }

        public override void OnStart()
        {
            if (!Mathf.Approximately(navMesh.stoppingDistance, StoppingDist))
            {
                navMesh.stoppingDistance = StoppingDist;
            }
            
        }


        public override TaskStatus OnUpdate()
        {
            base.OnUpdate();
            
            navMesh.SetDestination(player.transform.position);
            
            return TaskStatus.Success;
        }
    }
    
    [TaskCategory("Enemy Action")]
    public class PlayAttackEffect : EnemyActionBase
    {
        public float EffectPlayTime;
        public TimelineAsset effectTimeline;

        private float currentTime;
        public override void OnAwake()
        {
            base.OnAwake();
        }

        public override void OnStart()
        {
            base.OnStart();
            currentTime = 0f;
            playableDirector.playableAsset = effectTimeline;
            EffectPlayTime = (float)effectTimeline.duration;
            playableDirector.Play();
            navMesh.ResetPath();
            navMesh.velocity = Vector3.zero;
            LookAtPlayerWithOutY();
        }

        public override TaskStatus OnUpdate()
        {
            currentTime += Time.deltaTime;
            if (currentTime < EffectPlayTime)
            {
                return TaskStatus.Running;
            }
            return TaskStatus.Success;
        }

        public override void OnEnd()
        {
            base.OnEnd();

            playableDirector.playableAsset = null;
            currentTime = 0f;
        }

        public override void OnConditionalAbort()
        {
            base.OnConditionalAbort();
            
            playableDirector.Stop();
        }
    }
    
    [TaskCategory("Enemy Action")]
    public class AttackAction : EnemyActionBase
    {
        public Wait attackAfter;
        public float attackCastTime;
        public float currentTime;
        public override void OnAwake()
        {
            base.OnAwake();
            
            if (ActionTimelineAsset)
            {
                foreach (var track in ActionTimelineAsset.GetOutputTracks())
                {
                    if (track.isEmpty == false)
                    {
                        foreach (var clip in track.GetClips())
                        {
                            if (clip.asset is AnimationPlayableAsset animationPlayableAsset)
                            {
                                if (attackAfter != null)
                                {
                                    attackAfter.waitTime.Value = (float)animationPlayableAsset.duration;
                                }
                                break;
                            }
                        }
                    }
                }
            }
        }

        public override void OnStart()
        {
            base.OnStart();
            
            enemyBase.SetSuperArmor(false);
            enemyBase.SetGuard(false);
            animator.Play("Attack");
            
            playableDirector.Stop();
            playableDirector.Play(ActionTimelineAsset);

            LookAtPlayerWithOutY();
            navMesh.ResetPath();
        }

        public override TaskStatus OnUpdate()
        {
            return TaskStatus.Success;
        }
    }
    
    [TaskCategory("Enemy Action")]
    public class DamageAction : EnemyActionBase
    {
        public AnimationKey LeftSmallDamageAnimKey =  new AnimationKey("Left Small Damage");
        public AnimationKey LeftBigDamageAnimKey =  new AnimationKey("Left Big Damage");
        public AnimationKey RightSmallDamageAnimKey =  new AnimationKey("Right Small Damage");
        public AnimationKey RightBigDamageAnimKey =  new AnimationKey("Right Big Damage");
        
        private SharedVariable Damaged;
        private GuardAction guardAction;
        enum HitPosition
        {
            Left,
            Middle,
            Right
        }
        
        public float KnockbackTime;
        public Wait WaitAction;
        private float knockbackCur;
        private Vector3 knockbackDest;
        private Vector3 knockbackStart;
        private HitInfo currentHitInfo;
        private string currentKey;
        HitPosition hitPos = HitPosition.Middle;
        private bool hitByBigSword = false;

        public override void OnAwake()
        {
            base.OnAwake();

            Damaged = GetBT().GetVariable("Damaged");
            Damaged.SetValue(false);
            
            LeftSmallDamageAnimKey.UpdateHash();
            LeftBigDamageAnimKey.UpdateHash();
            RightSmallDamageAnimKey.UpdateHash();
            RightBigDamageAnimKey.UpdateHash();

            guardAction = Owner.FindTask<GuardAction>();
        }

        public override void OnStart()
        {
            enemyBase.ChangeFacial("Damage");
            knockbackCur = 0.01f;
            LookAtPlayerWithOutY();
            
            var position = transform.position;
            knockbackStart = position;
            currentHitInfo = enemyBase.GetCurrentHitInfo();
            Damaged.SetValue(true);
            hitByBigSword = currentHitInfo.Type != HitType.SmallKnife;

            //y가 -0.1 보다 작으면 오른쪽, 0.1a보다 크면 왼쪽, 그 사이 값은 중간
            Vector3 hitDirection = Vector3.Cross(position, currentHitInfo.Point);
            if (hitDirection.y > 0f)
            {
                hitPos = HitPosition.Left;
            }
            else
            {
                hitPos = HitPosition.Right;
            }

            PlayDamageAction();
            WaitAction.waitTime.Value = 0.5f;
            Vector3 knockbackDistance;
            if (currentHitInfo.Data is ComboData currentSkillData)
            {
                knockbackDistance = (transform.forward * currentSkillData.KnockbackDistance);
                KnockbackTime = currentSkillData.KnockbackTime;
                knockbackDest = usingLookatWithParent
                    ? enemyBase.RootObject.transform.position - knockbackDistance
                    : transform.position - knockbackDistance;
            }
            else if (currentHitInfo.Type == HitType.Ego)
            {
                knockbackDistance = (transform.forward * enemyBase.EgoKnockbackDist);
                KnockbackTime = enemyBase.EgoKnockbackTime;
                knockbackDest = usingLookatWithParent
                    ? enemyBase.RootObject.transform.position - knockbackDistance
                    : transform.position - knockbackDistance;
            }

            if (enemyBase.IsGuard && enemyBase.GuardGauge <= 0f)
            {
                enemyBase.SetGuardGauge(0f);
                enemyBase.SetAttackReady(false);
            }
            
        }

        public override TaskStatus OnUpdate()
        {
            navMesh.isStopped = true;
            navMesh.ResetPath();
            if (usingLookatWithParent && enemyBase.RootObject)
            {
                enemyBase.RootObject.transform.position = Vector3.Lerp(knockbackStart,knockbackDest,knockbackCur / KnockbackTime);
            }
            else
            {
                transform.position = Vector3.Lerp(knockbackStart,knockbackDest,knockbackCur / KnockbackTime);
            }
            
            if (Vector3.Distance(transform.position, knockbackDest) < 0.1f)
            {
                navMesh.isStopped = false;
                Damaged.SetValue(false);
                return TaskStatus.Success;
            }

            knockbackCur += Time.deltaTime;
            return TaskStatus.Running;
        }

        public void PlayDamageAction()
        {
            if (hitByBigSword)
            {
                currentKey = Random.Range(0,2) > 0 ? LeftBigDamageAnimKey.key : RightBigDamageAnimKey.key;
            }
            else
            {
                currentKey = Random.Range(0,2) > 0 ? LeftSmallDamageAnimKey.key : RightSmallDamageAnimKey.key;
            }
            
            animator.SetTrigger(currentKey);
        }

        public override void OnEnd()
        {
            base.OnEnd();
            enemyBase.ChangeFacial("Idle");
            animator.ResetTrigger(currentKey);
        }
    }

    [TaskCategory("Enemy Action")]
    public class GuardBreakAction : EnemyActionBase
    {
        public TimelineAsset GuardBreakAsset;
        public DamageAction DamageAction;

        public override void OnStart()
        {
            base.OnStart();
            GetBT().GetVariable("GuardBreak").SetValue(false);
            enemyBase.SetSuperArmor(false);
            enemyBase.SetGuard(false);
            enemyBase.SetAttackReady(false);
            playableDirector.Stop();
            playableDirector.playableAsset = GuardBreakAsset;
            playableDirector.Play();
            enemyBase.OnDamage(enemyBase.DataInfo.GuardBreakDamage,false);
            DamageAction.PlayDamageAction();
        }


        public override TaskStatus OnUpdate()
        {
            if (playableDirector.state == PlayState.Playing)
            {
                return TaskStatus.Running;
            }
            return TaskStatus.Success;
        }
    }
    
    [TaskCategory("Enemy Action")]
    public class DeathAction : EnemyActionBase
    {
        public Wait DeathWait;
        public string triggerName;

        public override void OnAwake()
        {
            base.OnAwake();

            if (DeathWait != null && ActionTimelineAsset)
            {
                DeathWait.waitTime = (float)ActionTimelineAsset.duration + 1f;
            }
            
        }

        public override void OnStart()
        {
            base.OnStart();
            if (ActionTimelineAsset)
            {
                playableDirector.Play(ActionTimelineAsset);
            }

            animator.Play("Death");
            enemyBase.ChangeFacial("Damage");
            navMesh.ResetPath();
            navMesh.isStopped = true;
        }
    }
    
    [TaskCategory("Enemy Action")]
    public class GuardAction : EnemyActionBase
    {
        public string GuardAttackTrigger = "GuardAttack";
        [field:SerializeField]private Wait waitTime;
        [field:SerializeField]private SharedBool GuardBreakVariable;
        
        private bool actionEnd;
        public float guardDuration;
        public float currentGuardTime;
        private bool guardAttack;
        private string guardTrigger = "Guard";
        public override void OnAwake()
        {
            base.OnAwake();

            animationEvent["On Animation End"] += _ =>
            {
                if (NodeData.ExecutionStatus == TaskStatus.Running)
                {
                    enemyBase.SetGuard(false);
                    actionEnd = true;
                }
            };
            
            TrackAsset animationTrack = ActionTimelineAsset.GetOutputTracks()
                .FirstOrDefault(track => track is AnimationTrack);
            TimelineClip guardLoopClip = animationTrack?.GetClips().FirstOrDefault(clip => clip.displayName.Equals("Guard_Loop"));
            guardDuration = guardLoopClip != null ? (float)(guardLoopClip.duration + guardLoopClip.start) : 0f;
        }

        public override void OnStart()
        {
            base.OnStart();
            animator.SetTrigger("Guard");
            guardAttack = false;
            currentGuardTime = 0f;
            actionEnd = false;
            enemyBase.SetGuard(true);
            GuardBreakVariable.Value = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (enemyBase.GuardGauge <= 0f)
            {
                GuardBreakVariable.Value = true;
            }
            if (currentGuardTime < guardDuration)
            {
                currentGuardTime += Time.deltaTime;
            }
            else if (!guardAttack && currentGuardTime >= guardDuration)
            {
                guardAttack = true;
                LookAtPlayerWithOutY();
                animator.SetTrigger(GuardAttackTrigger);
            }
            return actionEnd ? TaskStatus.Success : TaskStatus.Running;
        }
    }
    
    [TaskCategory("Enemy Action")]
    public class GuardAttackAction : EnemyActionBase
    {
        private SharedVariable guardAttackVariable;
        
        

        public override void OnAwake()
        {
            base.OnAwake();
            
            guardAttackVariable = GetBT().GetVariable("GuardAttack");
        }

        public override void OnStart()
        {
            base.OnStart();
            animator.SetFloat("Speed",0f);
            guardAttackVariable.SetValue(false);
            playableDirector.Stop();
            if (ActionTimelineAsset)
            {
                playableDirector.Play(ActionTimelineAsset);
            }
            
            enemyBase.SetSuperArmor(true);
            enemyBase.SetGuard(true);
            animator.SetTrigger("GuardAttack");
        }

        public override TaskStatus OnUpdate()
        {
            if (playableDirector.state == PlayState.Playing)
            {
                return TaskStatus.Running;
            }
            
            enemyBase.SetSuperArmor(false);
            return TaskStatus.Success;
        }
    }

    [TaskCategory("Enemy Action"),
     TaskDescription("현재 애니메이션에서 다음 애니메이션으로 서서히 변화 시키는 액션입니다.")]
    public class ChangeEnemyAnimParamValue : EnemyActionBase
    {
        public string AnimationName;
        public float GoalValue;
        public float ChangeTime;

        private float currentTime;
        private float currentValue;
        public override void OnStart()
        {
            base.OnAwake();
            currentValue = animator.GetFloat(AnimationName);
            currentTime = 0.01f;
        }

        public override TaskStatus OnUpdate()
        {
            currentValue = Mathf.Lerp(currentValue, GoalValue, currentTime / ChangeTime);
            float maxValue = Mathf.Max(currentValue, GoalValue);
            float minValue = Mathf.Min(currentValue, GoalValue);
            if (maxValue - minValue <= 0f)
            {
                animator.SetFloat(AnimationName,GoalValue);
                return TaskStatus.Success;
            }
            currentTime += Time.deltaTime;
            animator.SetFloat(AnimationName,currentValue);
            return TaskStatus.Running;
        }

        public override void OnConditionalAbort()
        {
            animator.SetFloat(AnimationName,GoalValue);
        }
    }
    
    [TaskCategory("Enemy Action")]
    public class ChasePlayer : EnemyActionBase
    {
        


        public override TaskStatus OnUpdate()
        {
            navMesh.SetDestination(player.transform.position);

            return TaskStatus.Success;
        }
    }
    
    [TaskCategory("Enemy Action")]
    public class MoveForward: EnemyActionBase
    {
        public float MoveDist;
        public float MoveTime;

        private Vector3 startPos;
        private Vector3 endPos;
        private float currentTime;

        public override void OnStart()
        {
            transform.LookAt(player.transform);
            startPos = transform.position;
            endPos = startPos + transform.forward * MoveDist;
            currentTime = 0f;
        }

        public override TaskStatus OnUpdate()
        {
            currentTime += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, currentTime / MoveTime);


            if (currentTime > MoveTime)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;

        }
    }
    
    
    [TaskCategory("Enemy Action"),
     TaskDescription("현재 몬스터의 NavMes 스피드를 서서히 변화 시키는 액션입니다.")]
    public class ChangeEnemyNavSpeed : EnemyActionBase
    {
        public SharedFloat GoalValueParam;
        public float GoalValue;
        public float ChangeTime;

        private float currentTime;
        private float currentValue;
        public override void OnAwake()
        {
            base.OnAwake();
            
            if (GoalValueParam.Value > 0f)
            {
                GoalValue = GoalValueParam.Value;
            }
        }

        public override void OnStart()
        {
            base.OnAwake();

            currentValue = navMesh.speed;
            currentTime = 0.01f;
        }
        
        

        public override TaskStatus OnUpdate()
        {
            currentValue = Mathf.Lerp(currentValue, GoalValue, currentTime / ChangeTime);
            float maxValue = Mathf.Max(currentValue, GoalValue);
            float minValue = Mathf.Min(currentValue, GoalValue);
            
            if (maxValue - minValue <= 0)
            {
                currentValue = GoalValue;
                return TaskStatus.Success;
            }
            
            currentTime += Time.deltaTime;
            navMesh.speed = currentValue;
            return TaskStatus.Running;
        }
        
        public override void OnConditionalAbort()
        {
            base.OnConditionalAbort();

            navMesh.speed = GoalValue;
        }
        
    }

    
}