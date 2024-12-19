using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.BreadBear
{
    [TaskCategory("Bread Bear")]
    public class BreadBearRecover : EnemyActionBase
    {
        private bool recoverEnd;

        public override void OnAwake()
        {
            base.OnAwake();

            animationEvent["RecoverEnd"] += _ =>
            {
                recoverEnd = true;
            };
        }

        public override void OnStart()
        {
            base.OnStart();
            
            animator.SetTrigger("Recover");
            recoverEnd = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (!recoverEnd)
            {
                return TaskStatus.Running;
            }

            return TaskStatus.Success;
        }
    }
}