using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using _NM.Core.Enemy;
using _NM.Core.Enemy.Effect;
using _NM.Core.Utils;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

[Serializable]
public class NoticeEffectClipInfo 
{
    public string effectName;
    public double duration;
    public double delay;
    public Transform position;
    public ParticleSystem effect;
}


public class EnemyNoticeEffectManager : MonoBehaviour
{
    [SerializeField] private List<TimelineAsset> timelines;
    [SerializeField] private PlayableDirector playableDirector;
    [SerializeField] private EnemyEffectManager effectManager;
    private Dictionary<TimelineAsset,List<NoticeEffectClipInfo>> timelineEffects = new();
    [SerializeField] private List<NoticeEffectClipInfo> currentInfo;

    private void OnValidate()
    {
        if (!playableDirector)
        {
            playableDirector = GetComponent<PlayableDirector>();
        }

        if (!effectManager)
        {
            effectManager = GetComponent<EnemyEffectManager>();
        }
    }

    private void Awake()
    {
        timelineEffects.Clear();
        Initialize();
    }

    private void Initialize()
    {
        foreach (var timeline in timelines)
        {
            var tracks = timeline.GetOutputTracks().Where(track => track is ControlTrack)
                .ToList();
            List<NoticeEffectClipInfo> clipInfoList = new();
            foreach (var track in tracks)
            {
                if (!track.isEmpty && track.muted)
                {
                    var clips = track.GetClips().ToArray();
                    foreach (var clip in clips)
                    {
                        NoticeEffectClipInfo clipInfo = new();
                        if (clip.asset is ControlPlayableAsset playableAsset)
                        {
                            clipInfo.effectName = clip.displayName;
                            clipInfo.duration = clip.duration;
                            clipInfo.delay = clip.start;
                            var clipObject = playableAsset.sourceGameObject.Resolve(playableDirector);
                            if (clipObject != null)
                            {
                                clipInfo.position = clipObject.transform;
                                clipInfo.effect = clipObject.GetComponent<ParticleSystem>();
                            }

                        }
                        currentInfo.Add(clipInfo);
                        clipInfoList.Add(clipInfo);
                    }
                }
                
            }
            
            timelineEffects.TryAdd(timeline,clipInfoList);
        }
    }

    public void PlayEffects(TimelineAsset timeline)
    {
        effectManager.PlayEffect(timelineEffects[timeline]);
    }

    public void StopEffects(EffectType type)
    {
        effectManager.StopEffect(type);
    }
} 
