using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional.BreadBear
{
    [TaskCategory("Enemy Conditional/Bread Bear")]
    public class CheckChaseRange : EnemyConditionalBase
    {
        private SharedVariable<float> chaseMinRange;
        private SharedVariable<float> chaseMaxRange;

        public override void OnAwake()
        {
            player = Character.Character.Local;
            chaseMinRange = Owner.GetVariable("AttackMaxRange") as SharedVariable<float>;
            chaseMaxRange = Owner.GetVariable("ChaseMaxRange") as SharedVariable<float>;
        }

        public override TaskStatus OnUpdate()
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if ( distance >= chaseMinRange.Value && 
                 distance <= chaseMaxRange.Value)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}