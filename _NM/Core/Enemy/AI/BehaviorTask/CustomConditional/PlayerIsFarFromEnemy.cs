using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional
{
    [TaskCategory("Enemy Conditional")]
    public class PlayerIsFarFromEnemy : EnemyConditionalBase
    {
        public float FarRange;
        
        public override TaskStatus OnUpdate()
        {
            if (Vector3.Distance(player.transform.position, transform.position) < FarRange)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}