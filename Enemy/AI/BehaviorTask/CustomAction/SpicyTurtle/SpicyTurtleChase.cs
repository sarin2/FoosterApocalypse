using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.SpicyTurtle
{
    [TaskCategory("Spicy Turtle")]
    public class SpicyTurtleChase : SpicyTurtleSkillAction
    {
        public SharedGameObject LeftFootSmoke;
        public SharedGameObject RightFootSmoke;

        public ParticleSystem leftParticle;
        public ParticleSystem rightParticle;
        
    public override void OnAwake()
    {
        base.OnAwake();

        if (LeftFootSmoke.Value)
        {
            leftParticle = LeftFootSmoke.Value.GetComponent<ParticleSystem>();
        }

        if (RightFootSmoke.Value)
        {
            rightParticle = RightFootSmoke.Value.GetComponent<ParticleSystem>();
        }

        animationEvent["LeftFoot"] += _ =>
        {
            if (NodeData.ExecutionStatus == TaskStatus.Running)
            {
                LeftFootSmoke.Value.SetActive(true);
                leftParticle.Play();
            }
        };
        
        animationEvent["RightFoot"] += _ =>
        {
            if (NodeData.ExecutionStatus == TaskStatus.Running)
            {
                RightFootSmoke.Value.SetActive(true);
                rightParticle.Play();
            }
        };
        
        animationEvent["LeftFootOff"] += _ =>
        {
            if (NodeData.ExecutionStatus == TaskStatus.Running)
            {
                leftParticle.Stop();
                LeftFootSmoke.Value.SetActive(false);
            }
        };
        
        animationEvent["RightFootOff"] += _ =>
        {
            if (NodeData.ExecutionStatus == TaskStatus.Running)
            {
                rightParticle.Stop();
                RightFootSmoke.Value.SetActive(false);
            }
        };

    }

    public override void OnStart()
    {
        base.OnStart();
        navMesh.ResetPath();
        SetFrontLegWeight(0);
        actionEnd = false;
        legsAnimator.MainGlueBlend = 0f;
    }

    public override void OnEnd()
    {

        if (IsAttackableRange())
        {
            navMesh.ResetPath();
            navMesh.velocity = Vector3.zero;
            legsAnimator.MainGlueBlend = 1.0f;
        }

        StopWalkEffect();
    }

    public override void OnConditionalAbort()
    {
        base.OnConditionalAbort();

        StopWalkEffect();
    }

    public void StopWalkEffect()
    {
        rightParticle.Stop();
        RightFootSmoke.Value.SetActive(false);
        leftParticle.Stop();
        LeftFootSmoke.Value.SetActive(false);
    }
    
    protected override void OnAnimationStart(AnimationEvent animEvent)
    {
        if (NodeData.ExecutionStatus == TaskStatus.Running)
        {
            actionEnd = false;
        }
        
    }
    
    protected override void OnAnimationEnd(AnimationEvent animEvent)
    {
        if (NodeData.ExecutionStatus == TaskStatus.Running)
        {
            actionEnd = true;
        }
    }

    public override TaskStatus OnUpdate()
    {
        navMesh.SetDestination(player.transform.position);
        return actionEnd ? TaskStatus.Success : TaskStatus.Running;
    }
    }
}