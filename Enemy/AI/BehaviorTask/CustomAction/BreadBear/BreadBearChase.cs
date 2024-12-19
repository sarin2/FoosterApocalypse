using System.Text;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.BreadBear
{
    [TaskCategory("Bread Bear")]
    public class BreadBearChase : EnemyActionBase
    {
        public string AnimationLayer;
        public string AnimationState;

        private StringBuilder animationHash;
        private SharedVariable<float> attackMinRange;
        private SharedVariable<float> attackMaxRange;
        private SharedVariable<Vector3> rootOffset;
        private SharedVariable<float> moveForwardDist;
        public bool moveEnd;
        public float StoppingDist;
        public override void OnAwake() 
        {
            base.OnAwake();
            attackMaxRange = Owner.GetVariable("AttackMaxRange") as SharedVariable<float>;
            attackMinRange = Owner.GetVariable("AttackMinRange") as SharedVariable<float>;
            rootOffset = Owner.GetVariable("RootOffset") as SharedVariable<Vector3>;
            moveForwardDist = Owner.GetVariable("MoveForwardDist") as SharedVariable<float>;
            animationHash = new StringBuilder();
            animationHash.Append(AnimationState);

            animationEvent["On Move End"] = _ =>
            {
                moveEnd = true;
            };
        }

        public override void OnStart()
        {
            base.OnStart();
            moveEnd = false;
            navMesh.SetDestination(player.transform.position);
        }
        
        public override TaskStatus OnUpdate()
        {
            base.OnUpdate();
            
                
            if (!moveEnd)
            {
                LookAtPlayerWithOutY();
                return TaskStatus.Running;
                
            }
            return TaskStatus.Success;


        }
        
        bool AnimationStateCheck(string stateName)
        {
            var currentInfo = animator.GetCurrentAnimatorStateInfo(0);
            var nextInfo = animator.GetNextAnimatorStateInfo(0);

            if (currentInfo.shortNameHash == Animator.StringToHash(stateName) ||
                nextInfo.shortNameHash == Animator.StringToHash(stateName))
            {
                return true;
            }
            
            return false;
        }

        public override void OnEnd()
        {
            base.OnEnd();
        }
        

        public override void OnConditionalAbort()
        {
            base.OnConditionalAbort();
            
            navMesh.ResetPath();
            navMesh.velocity = Vector3.zero;
        }
    }
}