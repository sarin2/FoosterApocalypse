using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomComposites
{
    public class ChanceSetter : Composite
    {
        public float Chance;
        private int currentChildIndex = 0;
        private TaskStatus executionStatus = TaskStatus.Inactive;
        
        public override int CurrentChildIndex()
        {
            return currentChildIndex;
        }

        public override bool CanExecute()
        {
            return currentChildIndex < children.Count && executionStatus != TaskStatus.Failure;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            currentChildIndex = children.Count;
            executionStatus = childStatus;
        }

        public override void OnConditionalAbort(int childIndex)
        {
            currentChildIndex = children.Count;
            executionStatus = TaskStatus.Inactive;
        }

        public override void OnEnd()
        {
            executionStatus = TaskStatus.Inactive;
            currentChildIndex = 0;
        }
    }
}