using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomConditional
{
    public enum Side
    {
        Right,
        Left,
        None
    }
    [TaskCategory("Enemy Conditional")]
    public class CheckDegreeToPlayer: EnemyConditionalBase
    {
        public float MinAngle;
        public float MaxAngle;
        public bool CheckDir;
        public Side CheckSide;
        
        public override TaskStatus OnUpdate()
        {
            float angle = Rad2Dir();
            if (MaxAngle >= angle && MinAngle <= angle)
            {
                if (CheckDir)
                {
                    Vector3 cross = Vector3.Cross(transform.forward,player.transform.position - transform.position);

                    float result = Vector3.Dot(cross, Vector3.up);


                    switch (CheckSide)
                    {
                        case Side.Left : return result < 0 ? TaskStatus.Success : TaskStatus.Failure; 
                        case Side.Right : return result > 0 ? TaskStatus.Success : TaskStatus.Failure; 
                    }

                    return TaskStatus.Failure;
                }
                return TaskStatus.Success;

            }

            return TaskStatus.Failure;
        }
        
        float Rad2Dir()
        {
            float lookingAngle = transform.eulerAngles.y;
            float radian = lookingAngle * Mathf.Deg2Rad;
            Vector3 lookDir = new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian));
            Vector3 targetDir = (player.transform.position - transform.position).normalized;
            float targetAngle = Mathf.Acos(Vector3.Dot(lookDir, targetDir)) * Mathf.Rad2Deg;

            return targetAngle;
        }
    }
}