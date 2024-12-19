using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.BreadBear
{
    
    [TaskCategory("Bread Bear")]
    public class BreadBearDash : EnemyActionBase
    {
        public bool dashEnd;
        public string animationName = "Dash";
        private SharedVariable<Vector3> rootOffset;

        public override void OnAwake()
        {
            base.OnAwake();

            animationEvent["On Dash End"] += _ =>
            {
                dashEnd = true;
            };
            
            rootOffset = Owner.GetVariable("RootOffset") as SharedVariable<Vector3>;
            
        }

        public override void OnStart()
        {
            base.OnStart();
            LookAtPlayerWithOutY();
            navMesh.ResetPath();
            navMesh.velocity = Vector3.zero;
            animator.SetTrigger(animationName);
            dashEnd = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (!dashEnd)
            {
                LookAtPlayerWithOutY();
                return TaskStatus.Running;
            }

            return TaskStatus.Success;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            dashEnd = false;
        }
    }
}