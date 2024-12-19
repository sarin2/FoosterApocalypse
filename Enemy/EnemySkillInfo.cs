using Sirenix.OdinInspector;
using UnityEngine;

namespace _NM.Core.Enemy
{
    public enum ESkillType
    {
        Auto,
        Cast
    }
    [CreateAssetMenu(fileName = "EnemyInfo", menuName = "Scriptable Object/EnemySkill")]
    public class EnemySkillInfo : ScriptableObject
    {
        [field: LabelText("스킬 이름"), SerializeField] public string SkillName { get; private set; }
        [field: LabelText("스킬 구분"), SerializeField] public ESkillType SkillType { get; private set; }
        [field: LabelText("스킬 쿨타임"), SerializeField] public float SkillCooldown { get; private set; }
        [field: LabelText("스킬 사용 후딜레이"),SerializeField] public float SkillAfterDelay { get; private set; }
        [field: LabelText("스킬 시전 대기 시간"),SerializeField ] public float SkillWaitTime { get; private set; }
        [field: LabelText("스킬 시전 시간"), SerializeField] public float SkillCastTime { get; private set; }
        [field: LabelText("스킬 데미지"), SerializeField] public int SkillDamage { get; private set; }
    }
}