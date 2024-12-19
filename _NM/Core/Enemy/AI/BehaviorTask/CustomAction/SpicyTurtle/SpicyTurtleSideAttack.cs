using _NM.Core.Camera;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.SpicyTurtle
{
    public class SpicyTurtleSideAttack : SpicyTurtleSkillAction
    {
        public ShakeInfo BigShake;
        public FacingPlayerAsync facingPlayerNode;

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
            //LookAtPlayerWithOutY();

            return TaskStatus.Running;
        }

        protected override void OnAnimationEnd(AnimationEvent animEvent)
        {
            if (NodeData.ExecutionStatus == TaskStatus.Running)
            {
                actionEnd = true;
                facingPlayerNode.FacingSpeed = 3.0f;
            }
        }
    }
}