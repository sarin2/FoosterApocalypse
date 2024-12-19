using System;
using UnityEngine;

namespace _NM.Core.Enemy.IK
{
    public class SirutoadTongue : MonoBehaviour
    {
        [SerializeField] private Transform playerPos;
        [SerializeField] private Transform sirutoadToungue;
        [SerializeField] private Animator animator;
        [SerializeField] private int attackIndex;

        private void OnValidate()
        {
            if (!animator)
            {
                animator = GetComponentInChildren<Animator>();
            }
        }

        private void Start()
        {
            attackIndex = animator.GetLayerIndex("Attack Layer");
            playerPos = Character.Character.Local.transform;
        }

        private void OnAnimatorIK(int layerIndex)
        {
            if (layerIndex != attackIndex)
            {
                return;
            }

            float dist = Vector3.Distance(sirutoadToungue.position, playerPos.position);

            if (dist < 3f)
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand,1.0f);
                animator.SetIKPosition(AvatarIKGoal.RightHand,playerPos.position);
            }
            else
            {
                animator.SetIKPositionWeight(AvatarIKGoal.RightHand,0);
            }

        }
    }
}
