using _NM.Core.Animation;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.SpicyTurtle
{
    [TaskCategory("Spicy Turtle")]
    public class SpicyTurtleGroggy : SpicyTurtleSkillAction
    {
        public float GuardTime;
        private bool guardAttackStarted;
        private float currentGuardTime;
        private float totalTime;
        private float AttackDuration;
        public SharedBool guardBreaked;
        public TimelineAsset guardBreakAsset;
        public override void OnAwake()
        {
            base.OnAwake();
        }

        public override void OnStart()
        {
            base.OnStart();
            
            animator.SetTrigger("Groggy");
            currentGuardTime = 0f;
            totalTime = GetAnimationDuration("Boss_Groggy");
            
            playableDirector.Play(guardBreaked.Value ? guardBreakAsset : ActionTimelineAsset);
            guardBreaked.Value = false;
        }

        public override TaskStatus OnUpdate()
        {
            
            currentGuardTime += Time.deltaTime;
            if (currentGuardTime > totalTime)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        protected override void OnAnimationStart(UnityEngine.AnimationEvent animEvent)
        {
            
        }

        protected override void OnAnimationEnd(UnityEngine.AnimationEvent animEvent)
        {

        }
    }
}