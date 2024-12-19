using _NM.Core.Enemy.Effect;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.BreadBear
{
    [TaskCategory("Bread Bear")]
    public class BreadBearParry : EnemyActionBase
    {
        [BehaviorDesigner.Runtime.Tasks.Tooltip("그로기를 유지하는 시간입니다.")] 
        public float groggyWait;

        public Wait groggyWaitNode;
        public SharedGameObject parryingEffect;
        private bool parring;
        
        private ParticleSystem parryingParticles;

        public override void OnAwake()
        {
            base.OnAwake();
            
            groggyWaitNode.waitTime = groggyWait;
            
            animationEvent["ParringEnd"] += _ =>
            {
                if (NodeData.ExecutionStatus == TaskStatus.Running)
                {
                    parring = false;
                }
            };

            if (parryingEffect.Value != null)
            {
                parryingParticles = parryingEffect.Value.GetComponent<ParticleSystem>();
            }
        }

        public override void OnStart()
        {
            base.OnStart();
            parring = true;
            playableDirector.Stop();
            enemyNoticeEffectManager.StopEffects(EffectType.SkillEffect);
            enemyBase.SetAttackReady(false);
            animator.SetTrigger("Parry");

            if (parryingEffect != null)
            {
                parryingEffect.Value.SetActive(true);
                parryingParticles.Play();
            }

        }

        public override TaskStatus OnUpdate()
        {
            if (parring)
            {
                return TaskStatus.Running;
            }
            
            return TaskStatus.Success;
        }


        public override void OnEnd()
        {
            base.OnEnd();
            
            parryingEffect.Value.SetActive(false);
        }
    }
}