using BehaviorDesigner.Runtime.Tasks;
using Unity.VisualScripting;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.BreadBear
{
    public class BreadBearAppear : EnemyActionBase
    {
        public string TriggerName;
        
        private bool appearEnd;
        public override void OnAwake()
        {
            base.OnAwake();
            appearEnd = false;
            animationEvent["AppearEnd"] += _ =>
            {
                appearEnd = NodeData.ExecutionStatus == TaskStatus.Running;
            };
        }

        public override void OnStart()
        {
            base.OnStart();
        }

        public override TaskStatus OnUpdate()
        {
            return appearEnd ? TaskStatus.Success : TaskStatus.Running;

        }

        public override void OnEnd()
        {
            base.OnEnd();
            
            animator.SetBool("Appeared",true);
        }
    }
}