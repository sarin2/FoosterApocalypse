using BehaviorDesigner.Runtime.Tasks;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional
{
    [TaskCategory("Enemy Conditional")]
    public class HasReached : EnemyConditionalBase
    {
        public override TaskStatus OnUpdate()
        {
            if (navMesh.remainingDistance < 0.1f)
            {
                return TaskStatus.Success;
            }
            else
            {
                return TaskStatus.Failure;
            }
        }
    }
}