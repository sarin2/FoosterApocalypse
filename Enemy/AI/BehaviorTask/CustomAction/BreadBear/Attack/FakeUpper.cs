using _NM.Core.Enemy.AI.BehaviorTask.CustomAction.BreadBear.Attack;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.BreadBear.BreadBear.Attack
{
    [TaskCategory("Bread Bear")]
    public class FakeUpper : BreadBearAttack
    {
        private TimelineAsset upperTimeline;
        private PlayableDirector director;
        public SignalEmitter AttackStart;
        public override TaskStatus OnUpdate()
        {
            
            return TaskStatus.Running;
        }
    }
}