using System;
using System.Collections.Generic;
using System.Linq;
using _NM.Core.Enemy.Effect;
using _NM.Core.Utils;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;

namespace _NM.Core.Enemy
{
    
    public class EnemyEffectManager : MonoBehaviour
    {
        [SerializeField] private List<ParticleSystem> particles = new();
        [SerializeField] private SerializableDictionary<ParticleSystem, EnemyEffectBase> effects = new();
        [SerializeField] private List<EnemyEffectBase> currentEffects;
        [SerializeField] private PlayableDirector playableDirector;

        /*
        public void PlayEffect(string effectName,double delay,double duration)
        {
            effects[effectName].gameObject.SetActive(true);
            effects[effectName].SetEffectDuration(delay,duration);
            currentEffects.Add(effects[effectName]);
        }
        
        public void PlayEffect(string effectName)
        {
            effects[effectName].gameObject.SetActive(true);
            currentEffects.Add(effects[effectName]);
        }
*/
        public void PlayEffect(List<NoticeEffectClipInfo> clipInfoList)
        {
            foreach (var clipInfo in clipInfoList)
            {
                if (clipInfo.effect)
                {
                    effects[clipInfo.effect].gameObject.SetActive(true);
                    effects[clipInfo.effect].SetEffectDuration(clipInfo.delay,clipInfo.duration);
                    currentEffects.Add(effects[clipInfo.effect]);
                }

            }
        }

        private void OnValidate()
        {
            if (!playableDirector)
            {
                playableDirector = GetComponent<PlayableDirector>();
            }
        }


        public void StopEffect(EffectType effectType)
        {
            var skillEffects = effects.Select(e => e.Value)
                .Where(effect => effect!= null && effect.EffectType == effectType)
                .ToList();

            for (int i = 0; i < skillEffects.Count; i++)
            {
                skillEffects[i].EffectStop();
            }
        }
        

        private void Awake()
        {
            currentEffects = new();
            effects.Clear();
        }

        private void Start()
        {
            currentEffects.Clear();
            for (int i = 0; i < particles.Count; i++)
            {
                effects.TryAdd(particles[i], particles[i].GetComponent<EnemyEffectBase>());
            }
        }

        private void Update()
        {
            currentEffects.RemoveAll(effect => effect.IsEffectEnd);
            var playable = currentEffects.FindAll(effect => effect.IsPlayable).ToArray();

            foreach (var effect in playable)
            {
                effect.EffectPlay();
            }

            if (playableDirector)
            {
                if (playableDirector.state == PlayState.Playing)
                {
                    foreach (var effect in playable)
                    {
                        if (effect.IsPaused)
                        {
                            effect.EffectPlay();
                        }
                    }
                }
                else
                {
                    foreach (var effect in playable)
                    {
                        effect.EffectPause();
                    }
                }
            }



            
        }
    }
}