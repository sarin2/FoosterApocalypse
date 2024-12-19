using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction
{
    [TaskCategory("Enemy Action/Takat")]
    public class TakatAttack : AttackAction
    {

        public override void OnAwake()
        {
            base.OnAwake();
            
        }

        public override void OnStart()
        {
            enemyBase.SetSuperArmor(false);
            enemyBase.SetGuard(false);
            animator.SetTrigger("Attack");
            
            playableDirector.Stop();
            playableDirector.Play(ActionTimelineAsset);

            LookAtPlayerWithOutY();
            navMesh.ResetPath();
            
        }

        public override TaskStatus OnUpdate()
        {
            if (playableDirector.state == PlayState.Playing)
            {
                return TaskStatus.Running;
            }
            return TaskStatus.Success;
        }
    }
    
    [TaskCategory("Enemy Action/Takat")]
    public class TakatGuardAttack : EnemyActionBase
    {
        public Wait attackAfter;
        public float attackCastTime;
        public float currentTme;
        
        private SharedVariable guardAttackVariable;
        public override void OnAwake()
        {
            base.OnAwake();
            
            guardAttackVariable = GetBT().GetVariable("GuardAttack");
        }

        public override void OnStart()
        {
            Debug.Log("GuardAttack");
            guardAttackVariable.SetValue(false);
            enemyBase.SetGuard(false);
            enemyBase.SetSuperArmor(true);
            
            animator.SetTrigger("GuardAttack");
        }

        public override TaskStatus OnUpdate()
        {
            if (animator.GetAnimatorTransitionInfo(0).IsName("TailSweep"))
            {
                return TaskStatus.Running;
            }

            enemyBase.SetSuperArmor(false);
            return TaskStatus.Success;
        }
    }
}