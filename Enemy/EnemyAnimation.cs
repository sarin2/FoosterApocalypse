using System;
using _NM.Core.Utils;
using UnityEngine;

namespace _NM.Core.Enemy
{
    [Serializable]
    public struct EnemyAnimation
    {
        [SerializeField] private SerializableDictionary<string,AnimationKey> animationKeys;
        [SerializeField] private Animator animator;
        public Animator EnemyAnimator => animator;

        public void Initialize(Animator animator)
        {
            animationKeys = new();
            this.animator = animator;
        }
    
    
    }
}
