using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.Playables;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.Scarecrow
{
    public class PlayParryingEffect : EnemyActionBase
    {
        
        public SharedGameObject ParryingStartObject;
        public SharedGameObject ParryingObject;
        public SharedGameObject ParryingFailObject;
        public SharedGameObject ParryingSuccessObject;
        
        private ParticleSystem parryingStartParticle;
        private ParticleSystem parryingParticle;
        private ParticleSystem parryingFailParticle;
        private ParticleSystem parryingSuccessParticle;

        public SharedFloat ParryingWaitTime;
        public SharedFloat ParryingDurationTime;
        public Wait ParryingWait;
        public Wait ParryingFailWait;
        
        private float currentTime;
        private float duration;

        public override void OnAwake()
        {
            enemyBase = GetComponent<EnemyBase>();
            playableDirector = GetComponent<PlayableDirector>();

            parryingStartParticle = ParryingStartObject?.Value?.GetComponent<ParticleSystem>();
            parryingParticle = ParryingObject?.Value?.GetComponent<ParticleSystem>();
            parryingFailParticle = ParryingFailObject?.Value?.GetComponent<ParticleSystem>();
            parryingSuccessParticle = ParryingSuccessObject?.Value?.GetComponent<ParticleSystem>();
            
            if (ParryingWait != null)
            {
                ParryingWait.waitTime = ParryingWaitTime?.Value;
            }

            if (ParryingFailWait != null)
            {
                ParryingFailWait.waitTime = ParryingWaitTime?.Value;
            }

            duration = ParryingDurationTime?.Value ?? 1f;

            if (parryingParticle)
            {
                ParticleSystem.MainModule particleModule = parryingParticle.main;
                particleModule.duration = duration;
                particleModule.startLifetime = duration;
            }

        }

        public override void OnStart()
        {
            ResetParticle();
            currentTime = 0f;
            enemyBase.SetAttackReady(true);
            parryingParticle.Play(true);
            parryingStartParticle.Play(true);
        }

        public override TaskStatus OnUpdate()
        {
            currentTime += Time.deltaTime;

            if (currentTime >= duration)
            {
                ResetParticle();
                enemyBase.SetAttackReady(false);
                parryingFailParticle?.Play(true);
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }
        
        public override void OnConditionalAbort()
        {
            ResetParticle();
            enemyBase.SetAttackReady(false);
            parryingSuccessParticle.Play(true);
        }

        private void ResetParticle()
        {
            parryingSuccessParticle.Simulate(0f,true);
            parryingFailParticle.Simulate(0f, true);
            parryingParticle.Simulate(0f, true);
            parryingStartParticle.Simulate(0f, true);
            
            parryingSuccessParticle.Stop(true);
            parryingFailParticle.Stop(true);
            parryingParticle.Stop(true);
            parryingStartParticle.Stop(true);
        }
    }
}