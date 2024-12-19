using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using FIMSpace.FProceduralAnimation;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using AnimationEvent = _NM.Core.Animation.AnimationEvent;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction
{
    public class EnemyActionBase : Action
    {
        public float Priority;
        public float Chance;
        public bool usingLookatWithParent;
        public TimelineAsset ActionTimelineAsset;
        
        [BehaviorDesigner.Runtime.Tasks.Tooltip("실행 이후 다른 노드들에게 추가 할 공통 가중치입니다.")]
        public float WeightToOtherNode;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("특정 가중치를 추가할 공격 노드입니다.")]
        public EnemyActionBase SpecificAttackNode;
        [BehaviorDesigner.Runtime.Tasks.Tooltip("위에서 선택한 공격노드에 얼마나 가중치를 추가할지 결정합니다.")]
        public float WeightForSpecificAttackNode;
        protected EnemyBase enemyBase;
        protected NavMeshAgent navMesh;
        protected Animator animator;
        protected PlayableDirector playableDirector;
        protected EnemyAnimationController animationController;
        protected EnemyNoticeEffectManager enemyNoticeEffectManager; 
        protected MonsterIKController ikController;
        protected Character.Character player;
        protected CancellationTokenSource actionCts;
        protected AnimationEvent animationEvent;
        protected EnemyEffectManager effectManager;
        protected LegsAnimator legsAnimator;
        public int executedOrder;
        public  float weight;
        private BehaviorTree owner;

        private Transform parryingObject;
        public override void OnAwake()
        {
            base.OnAwake();
            OnInitialize();
        }

        public virtual void OnInitialize()
        {
            legsAnimator = transform.GetComponentInParent<LegsAnimator>();
            enemyBase = GetComponent<EnemyBase>();
            playableDirector = GetComponent<PlayableDirector>();
            animationController = GetComponent<EnemyAnimationController>();
            enemyNoticeEffectManager = GetComponent<EnemyNoticeEffectManager>();
            //ikController = GetComponent<MonsterIKController>();
            effectManager = GetComponent<EnemyEffectManager>();
            navMesh = enemyBase.NavMeshAgent;
            animator = enemyBase.Animator;
            animationEvent = enemyBase.AnimationEvent;
            player = Character.Character.Local;
            owner = GetComponent<BehaviorTree>();
            
            weight = 1f;

        }

        public override float GetPriority()
        {
            return Priority;
        }

        public BehaviorTree GetBT()
        {
            return owner;
        }

        public override void OnStart()
        {
            base.OnStart();
            enemyBase.SetAttackReady(false);
            if (ActionTimelineAsset)
            {
                playableDirector.Stop();
                playableDirector.Play(ActionTimelineAsset);
            }
        }

        public void LookAtPlayerWithOutY()
        {
            
            Vector3 playerPos = player.transform.position;
            Vector3 rotation = new Vector3(playerPos.x, transform.position.y, playerPos.z);
            
            
            if (usingLookatWithParent&& enemyBase.RootObject)
            {
                enemyBase.RootObject.transform.LookAt(rotation);
            }
            else
            {
                transform.LookAt(rotation);
            }
        }

        public void SetFrontLegWeight(float value = 0.25f)
        {
            legsAnimator.Legs[0].LegBlendWeight = value;
            legsAnimator.Legs[1].LegBlendWeight = value;
        }

        public override void OnConditionalAbort()
        {
            base.OnConditionalAbort();
            enemyBase.SetAttackReady(false);
        }

        public override float GetUtility()
        {
            return Chance * (weight * executedOrder);
        }
        
        public void AddWeight(float amount)
        {
            weight += amount;
        }

        public void AddExecutedOrder()
        {
            executedOrder++;
        }

        public float GetAnimationDuration(string guardName)
        {
            TrackAsset animationTrack = ActionTimelineAsset?.GetOutputTracks()
                .FirstOrDefault(track => track is AnimationTrack);
            TimelineClip guardLoopClip = animationTrack?.GetClips().FirstOrDefault(clip => clip.displayName.Equals(guardName));
            return guardLoopClip != null ? (float)guardLoopClip.duration: 0f;
        }

        public void CalculateParryingTime()
        {
            TrackAsset controlTrack = ActionTimelineAsset?.GetOutputTracks().FirstOrDefault(track => track.name.Equals("ParryingTime"));
            
            TimelineClip parryingClip = controlTrack?.GetClips().FirstOrDefault(clip=> clip.displayName.Equals("ParryingTime"));
            ControlPlayableAsset playableAsset = parryingClip?.asset as ControlPlayableAsset;
            
            float duration = (float) (parryingClip?.duration ?? 0f);
            if(playableAsset != null)
            {
                parryingObject = playableAsset.sourceGameObject.Resolve(playableDirector).transform;
                for(int i = 0; i < parryingObject.childCount; i++)
                {
                    ParticleSystem currentChild = parryingObject.GetChild(i).GetComponent<ParticleSystem>();

                    var module = currentChild.main;
                    module.duration = duration;
                    module.startLifetime = duration;

                }
            }
        }

    }
}