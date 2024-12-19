using System.Threading;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.SpicyTurtle
{
    public class SpicyTurtleVolcano : SpicyTurtleSkillAction
    {
        private readonly int volcanoEnd = Animator.StringToHash("VolcanoEnd");
        public BossProjectileManager projectileManager;
        public AnimationCurve fireCurve;
        public float duration;
        public float fireInterval;
        public float radius;

        public float currentTime;
        private bool volcanoStarted;
        private CancellationTokenSource volcanoCts;
        
        public override void OnAwake()
        {
            base.OnAwake();
            
            projectileManager = UnityEngine.Object.FindObjectOfType<BossProjectileManager>();
            duration = GetAnimationDuration("Volcano_Stay");
            animationEvent["Volcano Fire"] += anim =>
            {
                projectileManager.FireRepeat(duration, fireInterval,volcanoCts.Token, fireCurve, radius).Forget();
                volcanoStarted = true;
            };
            ShakeInfo.duration = duration;

        }

        public override void OnStart()
        {
            legsAnimator.UseGluing = false;
            base.OnStart();
            volcanoCts = new CancellationTokenSource();
            currentTime = 0f;
            actionEnd = false;
            volcanoStarted = false;
        }

        public override TaskStatus OnUpdate()
        {
            if (actionEnd)
            {
                return TaskStatus.Success;
            }

            if (volcanoStarted)
            {
                currentTime += Time.deltaTime;
            }
            
            if (currentTime > duration)
            {
                if (!projectileManager.PlayerHit)
                {
                    Owner.SendEvent("Groggy");
                }
                else
                {
                    animator.SetTrigger(volcanoEnd);
                }
                
            }

            return TaskStatus.Running;
        }

        public override void OnConditionalAbort()
        {
            legsAnimator.UseGluing = true;
            base.OnConditionalAbort();
            volcanoStarted = false;
            currentTime = 0f;
            animator.ResetTrigger(volcanoEnd);
            volcanoCts?.Cancel();
            volcanoCts?.Dispose();
        }

        public override void OnEnd()
        {
            legsAnimator.UseGluing = true;
            base.OnEnd();
            volcanoStarted = false;
            currentTime = 0f;
            animator.ResetTrigger(volcanoEnd);
            volcanoCts?.Cancel();
            volcanoCts?.Dispose();
        }

        protected override void OnAnimationStart(AnimationEvent animEvent)
        {
            
        }

        protected override void OnAnimationEnd(AnimationEvent animEvent)
        {
            actionEnd = true;
        }
    }
}