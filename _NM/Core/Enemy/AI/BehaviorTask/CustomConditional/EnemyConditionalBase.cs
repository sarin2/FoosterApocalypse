using _NM.Core.Enemy.AI.EnemyAI;
using BehaviorDesigner.Runtime;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime.Tasks;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional
{
    public class EnemyConditionalBase : Conditional
    {
        protected EnemyBase enemyBase;
        protected NavMeshAgent navMesh;
        protected Animator animator;
        protected Character.Character player;
        private BehaviorTree owner;
        
        public override void OnAwake()
        {
            base.OnAwake();
            OnInitialize();
        }

        public virtual void OnInitialize()
        {
            enemyBase = GetComponent<EnemyBase>();
            navMesh = enemyBase.NavMeshAgent;
            animator = enemyBase.Animator;
            player = Character.Character.Local;
            owner = GetComponent<BehaviorTree>();
        }

        public BehaviorTree GetBT()
        {
            return owner;
        }

    }
}