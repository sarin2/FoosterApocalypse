using _NM.Core.Animation;
using _NM.Core.Character;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.SpicyTurtle
{
    [TaskCategory("Spicy Turtle")]
    public class SpicyTurtleGuard : SpicyTurtleSkillAction
    {
        public float GuardTime;
        private bool guardAttackStarted;
        private float currentGuardTime;
        private float totalTime;
        private float AttackDuration;
        public SharedBool guardBreaked;
        public override void OnAwake()
        {
            base.OnAwake();

            GuardTime = GetAnimationDuration("Boss Guard");
            AttackDuration= GetAnimationDuration("Boss Guard Attack");


            totalTime = GuardTime + AttackDuration;
        }

        public override void OnStart()
        {
            base.OnStart();
            currentGuardTime = 0f;
            guardAttackStarted = false;
            enemyBase.SetGuard(true);
            animator.SetTrigger("Guard");
            guardBreaked.Value = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (enemyBase.GuardGauge < 0)
            {
                enemyBase.SetGuard(false);
                enemyBase.OnDamage(enemyBase.DataInfo.GuardBreakDamage);
                guardBreaked.Value = true;
                player.Camera.PlayParryingEffect(CharacterCamera.ParryingType.BigKnife).Forget();
                enemyBase.SendBehaviorTreeEvent("Groggy");
                return TaskStatus.Failure;
            }

            if (playableDirector.state == PlayState.Playing)
            {
                currentGuardTime += Time.deltaTime;
            }
            if (currentGuardTime >= GuardTime && !guardAttackStarted)
            {
                enemyBase.SetGuard(false);
                animator.SetTrigger("GuardAttack");
                guardAttackStarted = true;
            }

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