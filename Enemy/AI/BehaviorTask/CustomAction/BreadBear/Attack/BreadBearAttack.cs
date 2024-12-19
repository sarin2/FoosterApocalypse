using System.Collections.Generic;
using System.Linq;
using _NM.Core.Enemy.Effect;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.BreadBear.Attack
{
    [TaskCategory("Bread Bear")]
    public class BreadBearAttack : EnemyActionBase
    {
        public string AttackTriggerName;
        public TimelineAsset attackTimeline;
        public string EffectName;
        public bool isCancelableAttack;
        public float CancelTime;
        public bool dontPlayFromEffectManager;
        public List<string> EffectNameList;


        private bool keepLook;
        private List<BreadBearAttack> attacks;
        private bool animationFinished;
        private bool animationStarted;
        private float currentCancelTime;
        private double effectDelay;
        private double effectDuration;
        

        public override void OnAwake()
        {
            attacks = null;
            base.OnAwake();
            
            attacks = Owner.FindTasks<BreadBearAttack>();

            weight = 1f;

            if (attackTimeline)
            {
                foreach (var track in attackTimeline.GetOutputTracks())
                {
                    if (track.isEmpty == false)
                    {
                        foreach (var clip in track.GetClips())
                        {
                            if (clip.asset is ControlPlayableAsset controlPlayableAsset && controlPlayableAsset.name.Equals("MainFX"))
                            {
                                effectDelay = clip.start;
                                effectDuration = clip.duration;
                                break;
                            }
                        }
                    }
                }
            }
            

                        
            animationEvent["On Attack Anim Start"] += _ =>
            {
                animationStarted= true;
            };

            animationEvent["On Attack Anim End"] += _ =>
            {
                animationFinished = true;
            };

            animationEvent["Look At Start"] += _ =>
            {
                keepLook = true;
            };
            
            animationEvent["Look At End"] += _ =>
            {
                keepLook = false;
            };
        }

        public override void OnStart()
        {
            animationFinished = false;
            animator.SetTrigger(AttackTriggerName);
            animationController.Attacking = true;
            keepLook = false;
            playableDirector.Stop();
            playableDirector.Play(attackTimeline);
            LookAtPlayerWithOutY();

            if (!dontPlayFromEffectManager)
            {
                enemyNoticeEffectManager.StopEffects(EffectType.SkillEffect);
                enemyNoticeEffectManager.PlayEffects(attackTimeline);
            }
            
        }

        public override TaskStatus OnUpdate()
        {
            if (keepLook)
            {
                LookAtPlayerWithOutY();
            }

            
            //transform.position += animator.deltaPosition;
            
            currentCancelTime += Time.deltaTime;
            if (isCancelableAttack && currentCancelTime > CancelTime)
            {
                currentCancelTime = 0f;
                Owner.SendEvent("CancelAction");
                animator.SetTrigger("Cancel");
                return TaskStatus.Success;
            }

            if (animationStarted && attackTimeline)
            {
                animationStarted = false;
            }
            
            if (animationFinished)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        public override float GetUtility()
        {
            return Chance * (weight * executedOrder);
        }

        public override void OnEnd()
        {

            playableDirector.Stop();
            playableDirector.playableAsset = null;
            
            for(int i = 0; i < attacks.Count; i++)
            {
                if (attacks[i] != this)
                {
                    if (attacks[i].weight - WeightToOtherNode <= 1f)
                    {
                        attacks[i].AddWeight(WeightToOtherNode);
                    }
                    attacks[i].AddExecutedOrder();
                }
            }
            SpecificAttackNode?.AddWeight(WeightForSpecificAttackNode);
            weight = 0.1f;
            executedOrder = 1;
            animationController.Attacking = false;
            enemyBase.SetAttackReady(false);
            animationFinished = true;
            keepLook = false;
        }

        public override void OnConditionalAbort()
        {
            base.OnConditionalAbort();
            playableDirector.Stop();
            keepLook = false;
            animationFinished = true;
            animationController.Attacking = false;
            enemyBase.SetAttackReady(false);
        }
        
        private void KeepLookAtTarget(bool state)
        {
            keepLook = state;
        }
    }
}