using BehaviorDesigner.Runtime.Tasks;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional
{
    public class CheckGuardTime : EnemyConditionalBase
    {
        public override TaskStatus OnUpdate()
        {
            float time = (float)GetBT().GetVariable("GuardTime").GetValue();
            float currentTime = (float)GetBT().GetVariable("CurrentGuardTime").GetValue();

            if (currentTime > time)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;

        }
    }
}