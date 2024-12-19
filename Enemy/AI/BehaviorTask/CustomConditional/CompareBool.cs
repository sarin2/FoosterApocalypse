using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional
{
    public class CompareBool : EnemyConditionalBase
    {
        [RequiredField] public SharedBool Variable;

        public override TaskStatus OnUpdate()
        {
            return Variable.Value ? TaskStatus.Success : TaskStatus.Failure;
        }
    }
}