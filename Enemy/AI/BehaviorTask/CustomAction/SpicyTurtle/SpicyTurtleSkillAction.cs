using System.Collections.Generic;
using _NM.Core.Animation;
using _NM.Core.Camera;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.SpicyTurtle
{
    [TaskCategory("Spicy Turtle")]
    public abstract class SpicyTurtleSkillAction : EnemyActionBase
    {
        public ShakeInfo ShakeInfo;
        public string triggerName;
        protected Type.SpicyTurtle owner;
        public float lookatTime;
        protected float currentLookAtTime;
        public bool actionEnd;
        private List<SpicyTurtleSkillAction> attacks;
        
        private SharedVariable<float> attackMinRange;
        private SharedVariable<float> attackMaxRange;

        protected CameraController cameraController;
        public override void OnAwake()
        {
            base.OnAwake();
            actionEnd = true;
            attacks = null;
            attacks = Owner.FindTasks<SpicyTurtleSkillAction>();
            
            attackMaxRange = Owner.GetVariable("AttackMaxRange") as SharedVariable<float>;
            attackMinRange = Owner.GetVariable("AttackMinRange") as SharedVariable<float>;
            owner = enemyBase as Type.SpicyTurtle;
            if (player)
            {
                cameraController = player.Camera.Controller;
            }
            
            animationEvent["On Animation Start"] += OnAnimationStart;
            animationEvent["On Animation End"] += OnAnimationEnd;
            animationEvent["Shake"] += _ =>
            {
                if (!actionEnd)
                {
                    cameraController.Shake(ShakeInfo).Forget();
                }
                
            };
            
        }

        public override void OnStart()
        {
            base.OnStart();
            actionEnd = false;
            if (!string.IsNullOrEmpty(triggerName))
            {
                animator.SetTrigger(triggerName);
            }

            currentLookAtTime = 0f;
        }

        public override void OnEnd()
        {
            base.OnEnd();
            actionEnd = true;
            
            for(int i = 0; i < attacks.Count; i++)
            {
                if (attacks[i] != this)
                {
                    if (attacks[i].weight - WeightToOtherNode <= 1f)
                    {
                        attacks[i].AddWeight(WeightToOtherNode);
                    }
                    attacks[i].AddExecutedOrder();
                }
            }
            SpecificAttackNode?.AddWeight(WeightForSpecificAttackNode);
            weight = 0.1f;
            executedOrder = 1;
            enemyBase.SetAttackReady(false);
        }

        public void FacingToPlayer()
        {
            if (lookatTime > currentLookAtTime)
            {
                currentLookAtTime += Time.deltaTime;
                SetFrontLegWeight(1f);
                LookAtPlayerWithOutY();
                legsAnimator.MainGlueBlend = 1f;
            }
            else
            {
                SetFrontLegWeight(0f);
                legsAnimator.MainGlueBlend = 0f;
            }
        }

        public override TaskStatus OnUpdate()
        {
            return base.OnUpdate();
            
        }

        protected abstract void OnAnimationStart(UnityEngine.AnimationEvent animEvent);

        protected abstract void OnAnimationEnd(UnityEngine.AnimationEvent animEvent);
        
        public bool IsAttackableRange()
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            if ( distance >= attackMinRange.Value && 
                 distance <= attackMaxRange.Value)
            {
                return true;
            }

            return false;
        }

        public override void OnConditionalAbort()
        {
            base.OnConditionalAbort();

            actionEnd = true;
        }
    }
}