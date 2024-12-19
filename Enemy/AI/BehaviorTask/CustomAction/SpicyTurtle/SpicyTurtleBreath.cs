using System;
using System.Threading;
using _NM.Core.Camera;
using BehaviorDesigner.Runtime.Tasks;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.AI.BehaviorTask.CustomAction.SpicyTurtle
{
    public class SpicyTurtleBreath : SpicyTurtleSkillAction
    {
        public ShakeInfo BigShake;
        public TimeSpan breathDuration;
        public string BreathStayAnim;
        public string BreathEndAnim;
        public CancellationTokenSource breathCts;

        public override void OnAwake()
        {
            base.OnAwake();

            animationEvent["BigShake"] += _ =>
            {
                cameraController.Shake(BigShake).Forget();
                
            };
            animationEvent["BreathStart"] += _ =>
            {
                Breath().Forget();
                cameraController.Shake(ShakeInfo).Forget();
            };
            
            if (ActionTimelineAsset)
            {
                foreach (var track in ActionTimelineAsset.GetOutputTracks())
                {
                    if(track is not AnimationTrack) continue;
                    if (!track.isEmpty)
                    {
                        foreach (var clip in track.GetClips())
                        {
                            if(clip.displayName.Equals(BreathStayAnim))
                            {
                                breathDuration = TimeSpan.FromSeconds((float)clip.duration);
                                ShakeInfo.duration = (float)clip.duration;
                                break;
                            }

                        }
                    }
                }
                
                
            }
        }

        public override void OnStart()
        {
            base.OnStart();
            animator.SetTrigger("Breath");
        }

        protected override void OnAnimationStart(AnimationEvent animEvent)
        {
            navMesh.ResetPath();
            navMesh.velocity = Vector3.zero;
        }

        public override TaskStatus OnUpdate()
        {
            if (actionEnd)
            {
                return TaskStatus.Success;
            }
            
            FacingToPlayer();

            return TaskStatus.Running;
        }

        protected override void OnAnimationEnd(AnimationEvent animEvent)
        {
            if (NodeData.ExecutionStatus == TaskStatus.Running)
            {
                actionEnd = true;
            }
        }

        private async UniTask Breath()
        {
            breathCts?.Cancel();
            breathCts?.Dispose();
            breathCts = new();
            await UniTask.Delay(breathDuration, cancellationToken: breathCts.Token);
            animator.SetTrigger(BreathEndAnim);
        }
    }
}