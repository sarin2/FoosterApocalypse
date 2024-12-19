using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction
{
    [TaskCategory("Enemy Action")]
    public class SirutoadChasePlayer : EnemyActionBase
    {

        private SharedVariable<float> attackMinRange;
        private SharedVariable<float> attackMaxRange;
        private Vector3 playerPos;
        private float distance;

        public override void OnAwake()
        {
            base.OnAwake();

            attackMinRange = Owner.GetVariable("AttackMinRange") as SharedVariable<float>;
            attackMaxRange = Owner.GetVariable("AttackMaxRange") as SharedVariable<float>;
        }

        public override void OnStart()
        {
            base.OnStart();

            playerPos = player.transform.position;
            distance = Vector3.Distance(playerPos, transform.position);
            
            
            
        }

        public override TaskStatus OnUpdate()
        {
            Vector3 dir = (transform.position - playerPos).normalized;
            if (distance >= attackMinRange.Value &&
                distance <= attackMaxRange.Value)
            {
                return TaskStatus.Success; 
            }
            
            if (distance < attackMinRange.Value)
            {
                navMesh.SetDestination(transform.position + dir);
            }

            if (distance > attackMaxRange.Value)
            {
                navMesh.SetDestination(transform.position - dir);
            }
            return TaskStatus.Success;
        }
    }
}