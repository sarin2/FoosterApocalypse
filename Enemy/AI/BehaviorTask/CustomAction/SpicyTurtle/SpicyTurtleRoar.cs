using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.SpicyTurtle
{
    [TaskCategory("Spicy Turtle")]
    public class SpicyTurtleRoar : SpicyTurtleSkillAction
    {
        public float roarDuration;
        public float roarIntensity;
        public float roarEndDuration;
        public SharedBool combatStarted;
        public override void OnAwake()
        {
            base.OnAwake();

            animationEvent["Roar"] += _ =>
            {
                owner.RoarManager.Roar(roarDuration, roarIntensity,roarEndDuration).Forget();
            };
        }

        public override void OnStart()
        {
            base.OnStart();
            
            SetFrontLegWeight();
            combatStarted.SetValue(true);
        }

        public override TaskStatus OnUpdate()
        {
            if (actionEnd)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        protected override void OnAnimationStart(AnimationEvent animEvent)
        {
            
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