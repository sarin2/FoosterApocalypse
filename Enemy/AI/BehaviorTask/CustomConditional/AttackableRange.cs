using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Unity.VisualScripting;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional
{
    [TaskCategory("Enemy Conditional")]
    public class AttackableRange : EnemyConditionalBase
    {
        [field:SerializeField]private SharedVariable attackMinRangeVariable;
        [field:SerializeField]private SharedVariable attackMaxRangeVariable;

        private float attackMinRange;
        private float attackMaxRange;

        public override void OnAwake()
        {
            player = Character.Character.Local;
            attackMinRangeVariable = Owner.GetVariable("AttackMinRange") as SharedVariable<float>;
            attackMaxRangeVariable = Owner.GetVariable("AttackMaxRange") as SharedVariable<float>;


            if (attackMinRangeVariable == null)
            {
                attackMinRange = 0f;
            }
            else
            {
                attackMinRange = (float)attackMinRangeVariable.GetValue();
            }
            
            if (attackMaxRangeVariable == null)
            {
                attackMaxRange = 0f;
            }
            else
            {
                attackMaxRange = (float)attackMaxRangeVariable.GetValue();
            }
            
        }

        public override TaskStatus OnUpdate()
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if ( distance >= attackMinRange && 
                 distance <= attackMaxRange)
            {
                return TaskStatus.Success;
            }

            return TaskStatus.Failure;
        }
    }
}