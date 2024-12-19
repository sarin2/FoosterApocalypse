using BehaviorDesigner.Runtime.Tasks;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomComposites
{
    public class PrioritySetter : Composite
    {
        public float Priority;
        public override float GetPriority()
        {
            return Priority;
        }
    }
}