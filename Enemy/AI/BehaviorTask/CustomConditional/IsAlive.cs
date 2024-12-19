using BehaviorDesigner.Runtime.Tasks;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional
{
    [TaskCategory("Enemy Conditional")]
    public class IsAlive : EnemyConditionalBase
    {

        public override TaskStatus OnUpdate()
        {
            if (enemyBase.Health.IsAlive)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}