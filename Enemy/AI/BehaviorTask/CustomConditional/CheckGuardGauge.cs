using BehaviorDesigner.Runtime.Tasks;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional
{
    [TaskCategory("Enemy Conditional")]
    public class CheckGuardGauge : EnemyConditionalBase
    {
        public float GaugeGoal;
        
        public override TaskStatus OnUpdate()
        {
            if (enemyBase.GuardGauge >= GaugeGoal)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}