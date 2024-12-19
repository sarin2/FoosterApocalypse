using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional
{
    public class IsDashRange : EnemyConditionalBase
    {
        public SharedFloat DashRange;
        public SharedFloat AttackMaxRange;

        public override TaskStatus OnUpdate()
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if ( distance <= DashRange.Value && 
                 distance >= AttackMaxRange.Value)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}