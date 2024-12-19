using System.Collections.Generic;
using _NM.Core.Enemy.AI.BehaviorTask.CustomAction;
using _NM.Core.Enemy.AI.BehaviorTask.CustomAction.BreadBear.Attack;
using _NM.Core.Manager;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomComposites
{
    [TaskDescription("Random Selector와 비슷하게 랜덤으로 가져오지만, 하위 자식들의 노드에 확률이 설정되어 있을 경우 해당 확률을 반영하여 실행할 수 있도록 지정합니다.")]
    public class RandomChanceExecutor : Composite
    {
        private float[] chanceProbs;
        private int currentChildIndex = 0;
        // The task status of the last child ran.
        private TaskStatus executionStatus = TaskStatus.Inactive;
        public bool UsingChanceSetter;
        private List<EnemyActionBase> childrenBases;
        private List<ChanceSetter> chanceSetters;

        public override void OnAwake()
        {
            chanceProbs = null;
            chanceSetters = new();
            childrenBases = new();
            foreach (var task in children)
            {
                if (UsingChanceSetter)
                {
                    chanceSetters.Add(task as ChanceSetter);
                }
                else
                {
                    childrenBases.Add(task as EnemyActionBase);
                }
                
            }


            if (UsingChanceSetter)
            {
                chanceProbs = new float[chanceSetters.Count];
            }
            else
            {
                chanceProbs = new float[childrenBases.Count];
            }
            
            
        }

        public override void OnStart()
        {
            if (UsingChanceSetter)
            {
                for (int i = 0; i < chanceSetters.Count; i++)
                {
                    chanceProbs[i] = chanceSetters[i].Chance;
                }
            }
            else
            {
                for (int i = 0; i < childrenBases.Count; i++)
                {
                    chanceProbs[i] = childrenBases[i].GetUtility();
                }
            }


        }
        
        public override int CurrentChildIndex()
        {
            int res = RandomGenerator.Choose(chanceProbs);
            return res;
        }
        
        public override bool CanExecute()
        {
            return currentChildIndex < children.Count && executionStatus != TaskStatus.Failure;
        }

        public override void OnChildExecuted(TaskStatus childStatus)
        {
            currentChildIndex = children.Count;
            executionStatus = childStatus;
        }

        public override void OnConditionalAbort(int childIndex) 
        {
            currentChildIndex = children.Count;
            executionStatus = TaskStatus.Inactive;
        }

        public override void OnEnd()
        {
            currentChildIndex = 0;
           executionStatus = TaskStatus.Inactive;
        }
    }
}