using System;
using System.Collections.Generic;
using _NM.Core.Camera;
using _NM.Core.Utils;
using UnityEngine;
using UnityEngine.Timeline;

namespace _NM.Core.Enemy.Data
{
    [Serializable]
    public class BossSkillData
    {
        public ShakeInfo shakeInfo;
        public TimelineAsset timeline;
    }
    
    [CreateAssetMenu(fileName = "Boss Camera ShakeData", menuName = "Scriptable Object/Boss Camera ShakeData")]
    public class BossCameraShakeData : ScriptableObject
    {
        [SerializeField] private SerializableDictionary<string, List<BossSkillData>> bossSkills;


        public List<BossSkillData> GetSkillShakeData(string key)
        {
            return bossSkills.GetValueOrDefault(key);
        }
    }
}