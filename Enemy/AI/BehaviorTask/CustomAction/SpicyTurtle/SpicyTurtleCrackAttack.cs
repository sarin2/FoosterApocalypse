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
    public class SpicyTurtleCrackAttack : SpicyTurtleSkillAction
    {
        public ShakeInfo BigShake;
        

        public override void OnAwake()
        {
            base.OnAwake();

            animationEvent["BigShake"] += _ =>
            {
                if (NodeData.ExecutionStatus == TaskStatus.Running)
                {
                    cameraController.Shake(BigShake).Forget();
                }
            };
        }
        
        public override void OnStart()
        {
            base.OnStart();
            
            SetFrontLegWeight(0f);
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