using System;
using _NM.Core.Manager;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _NM.Core.Enemy.Skill
{
    [Serializable]
    public class Skill
    {
        public string Name;
        public int MinDamage;
        public int MaxDamage;
    }
    [CreateAssetMenu(fileName = "EnemySkillTable", menuName = "Scriptable Object/EnemySkillTable")]
    public class SkillTable : ScriptableObject
    {
        [field:SerializeField] private static SerializableDictionary<int, Skill> SkillDict = new();

        public void ApplyParseData()
        {
            foreach (var skillData in DataManager.GetSheetData(GoogleSheetsConstantData.ESheetPage.MonsterSkillSettings))
            {
                Skill skill = new Skill
                {
                    Name = !string.IsNullOrEmpty(skillData["Skill_Name"]) ? skillData["Skill_Name"] : string.Empty,
                    MinDamage = int.TryParse(skillData["skill_dmg_min"], out int minValue) ? minValue : 0,
                    MaxDamage = int.TryParse(skillData["skill_dmg_max"], out int maxValue) ? maxValue : 0
                };
                int skillID = int.TryParse(skillData["Monster_skill_ID"], out int idValue) ? idValue : 0;

                if (!SkillDict.TryAdd(skillID, skill))
                {
                    SkillDict[skillID].Name = skill.Name;
                    SkillDict[skillID].MaxDamage = skill.MaxDamage;
                    SkillDict[skillID].MinDamage = skill.MinDamage;
                }
                
                
            }
        }

        public static void Initialize()
        {
            if (SkillDict.Count == 0)
            {
                var skillTable = Resources.Load<SkillTable>("Data/Enemy/Skill/EnemySkillTable");
                skillTable.ApplyParseData();
            }
        }

        public static int GetDamage(int skillID)
        {
            Initialize();
            
            if (SkillDict.TryGetValue(skillID, out var skill))
            {
                return Random.Range(skill.MinDamage, skill.MaxDamage+1);
            }

            return 0;
        }

        public static string GetSkillName(int skillID)
        {
            Initialize();
            
            if (SkillDict.TryGetValue(skillID, out var skill))
            {
                return skill.Name;
            }

            return string.Empty;
        }
        
        
    }
}