using System;
using System.Threading;
using _NM.Core.Camera;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.SpicyTurtle
{
    public class SpicyTurtleRush: SpicyTurtleSkillAction
    {
        public ShakeInfo BigShake;
        public TimeSpan breathDuration;
        public CancellationTokenSource breathCts;

        public override void OnAwake()
        {
            base.OnAwake();

            animationEvent["BigShake"] += _ =>
            {
                cameraController.Shake(BigShake).Forget();
                
            };
        }

        public override void OnStart()
        {
            base.OnStart();
            animator.SetTrigger(triggerName);
            currentLookAtTime = 0f;
        }

        protected override void OnAnimationStart(AnimationEvent animEvent)
        {
            navMesh.ResetPath();
            navMesh.velocity = Vector3.zero;
        }

        public override TaskStatus OnUpdate()
        {
            if (actionEnd)
            {
                return TaskStatus.Success;
            }
            
            FacingToPlayer();
            //LookAtPlayerWithOutY();

            return TaskStatus.Running;
        }

        protected override void OnAnimationEnd(AnimationEvent animEvent)
        {
            if (NodeData.ExecutionStatus == TaskStatus.Running)
            {
                actionEnd = true;
            }
        }
        
    }
}