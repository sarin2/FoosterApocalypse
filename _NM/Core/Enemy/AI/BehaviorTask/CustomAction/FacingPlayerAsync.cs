using System.Threading;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction
{
    [TaskCategory("Enemy Action")]
    public class FacingPlayerAsync : EnemyActionBase
    {
        public float FacingTime;
        public float FacingSpeed;
        private bool complete;
        private Tweener tweener;
        private float prevSpeed;

        public override void OnStart()
        {
            base.OnStart();
            legsAnimator.MainGlueBlend = 1f;
            FacingTime = Rad2Dir() / FacingSpeed;
            tweener = transform.parent.DOLookAt(player.transform.position, FacingTime).SetAutoKill(false);
            SetFrontLegWeight(1f);
        }

        public override TaskStatus OnUpdate()
        {
            if (tweener.IsComplete())
            {
                tweener.Kill();
                return TaskStatus.Success;
            }

            return TaskStatus.Running;
        }

        public override void OnAwake()
        {
            base.OnAwake();
            prevSpeed = FacingSpeed;
        }

        public override void OnEnd()
        {
            base.OnEnd();

            FacingSpeed = prevSpeed;
            legsAnimator.MainGlueBlend = 0f;
        }


        public override void OnConditionalAbort()
        {
            base.OnConditionalAbort();
        }
        
        float Rad2Dir()
        {
            float lookingAngle = transform.eulerAngles.y;
            float radian = lookingAngle * Mathf.Deg2Rad;
            Vector3 lookDir = new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian));
            Vector3 targetDir = (Character.Character.Local.transform.position - transform.position).normalized;
            float targetAngle = Mathf.Acos(Vector3.Dot(lookDir, targetDir));

            return targetAngle;
        }
    }
}