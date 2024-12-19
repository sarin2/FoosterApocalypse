using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.Effect
{
    public enum EffectType
    {
        SkillEffect,
        HitEffect
    }
    public interface IEnemyEffect
    {
        public void EffectPlay();
        public void EffectReset();
        public void EffectInitialize();
        public void EffectStop();
        public void EffectComplete();

    }


    public class EnemyEffectBase : MonoBehaviour, IEnemyEffect
    {
        [SerializeField] protected GameObject effectObject;
        [SerializeField] protected ParticleSystem effectParticle;
        [SerializeField] protected AudioSource effectAudio;
        [SerializeField] protected double effectDuration = 1f;
        [SerializeField] protected double totalDelay = 0f;
        [SerializeField] private double currentTime;
        [SerializeField] private double currentDelay;
        [field:SerializeField] public EffectType EffectType { get; private set; }
        public bool IsPlayable => currentDelay >= totalDelay &&
                                  effectParticle.isStopped;

        public bool IsPaused { get; private set; }
        public bool IsEffectEnd => currentTime >= effectDuration;

        private void OnValidate()
        {
            EffectInitialize();
        }

        private void OnEnable()
        {
            EffectReset();
            currentTime = 0f;
            currentDelay = 0f;
        }

        private void Update()
        {
            if (IsPlayable)
            {
                EffectPlay();
            }
            float dt = Time.deltaTime;
            if (gameObject.activeSelf)
            {
                if (currentDelay < totalDelay)
                {
                    currentDelay += dt;
                }
                currentTime += dt;

                if (IsEffectEnd)
                {
                    EffectComplete();
                }
            }
        }

        private void OnDisable()
        {
            EffectStop();
            effectParticle.Pause(true);
        }

        public void EffectPlay()
        {
            effectParticle.Play(true);
            IsPaused = false;
        }

        public void EffectReset()
        {
            effectParticle.Stop(true);
            effectParticle.Clear(true);
        }

        public void EffectInitialize()
        {
            effectObject = gameObject;
            effectParticle = effectObject.GetComponent<ParticleSystem>();
        }

        public void EffectStop()
        {
            effectParticle.Stop(true);
            currentTime += effectDuration;
            EffectComplete();
        }

        public void EffectPause()
        {
            effectParticle.Pause();
            IsPaused = true;
        }

        public void EffectComplete()
        {
            effectObject.SetActive(false);
        }

        public void SetEffectDuration(double delay,double duration)
        {
            effectDuration = duration + delay;
            totalDelay = delay;
        }
}
}