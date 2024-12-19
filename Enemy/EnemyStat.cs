using System;
using System.Collections.Generic;
using _NM.Core.Manager;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _NM.Core.Enemy
{
    public enum EFaction
    {
        Neutral,
        Enemy
    }

    public enum EQuality
    {
        Normal,
        Elite,
        Boss
    }
    
    [CreateAssetMenu(fileName = "EnemyDatas", menuName = "Scriptable Object/EnemyDatas")]
    public class EnemyData : ScriptableObject
    {
        public static EnemyData Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = Resources.Load<EnemyData>("Data/Enemy/Info/EnemyData");
                }
                return instance;
            }
        }
        private static EnemyData instance = null;

        public event Action onDataInitialized;
        public bool Initialized = false;

        public EnemyInfoContainer GetEnemyData(int monsterID)
        {
            return EnemyDatas?[monsterID];
        }

        [field: SerializeField] public SerializableDictionary<long, EnemyInfoContainer> EnemyDatas { get; private set; }

        public void Invoke()
        {
            onDataInitialized?.Invoke();
        }
    }
    
    [Serializable]
    public class EnemyInfoContainer
    {
        
        [field: Title("정보")]
        [field: LabelText("몬스터 아이디"), SerializeField]
        public int ID { get; private set; }

        [field: LabelText("이름"), SerializeField]
        public string EnemyName { get; private set; }

        [field: LabelText("인게임 이름"), SerializeField]
        public string NameKr { get; private set; }

        [field: LabelText("등급"), SerializeField]
        public EQuality Quality { get; private set; }

        [field: LabelText("공격 타입"), SerializeField]
        public EFaction Faction { get; private set; }

        [field: Space(10)]
        [field: Title("배회")]
        [field: LabelText("배회 속도"), SerializeField]
        public float PatrolSpeed { get; private set; }

        [field: LabelText("배회 거리"), SerializeField]
        public float PatrolRadius { get; private set; }

        [field: Space(10)]
        [field: Title("추적")]
        [field: LabelText("추적 속도"), SerializeField]
        public float ChaseSpeed { get; private set; }

        [field: LabelText("추적 거리"), SerializeField]
        public float ChaseRadius { get; private set; }


        [field: Space(10)]
        [field: Title("공격")]
        [field: LabelText("최소 공격력"), SerializeField]
        public float MinDmg { get; private set; }

        [field: LabelText("최대 공격력"), SerializeField]
        public float MaxDmg { get; private set; }

        [field: LabelText("스킬 공격력"), SerializeField]
        public float SkillDmg { get; private set; }

        [field: Space(10)]
        [field: Title("치명타")]
        [field: LabelText("치명타 확률"), SerializeField]
        public int CritChance { get; private set; }

        [field: LabelText("치명타 데미지"), SerializeField]
        public float CritDmg { get; private set; }

        [field: Space(10)]
        [field: Title("방어")]
        [field: LabelText("방어력"), SerializeField]
        public float Armor { get; private set; }

        [field: LabelText("최대 생명력"), SerializeField]
        public int MaxHp { get; private set; }

        [field: LabelText("가드 수치"), SerializeField]
        public float Guard { get; private set; }

        [field: LabelText("가드 초당 회복"), SerializeField]
        public float GuardRecovery { get; private set; }
        
        [field: LabelText("가드 브레이크 데미지"),SerializeField]
        public int GuardBreakDamage { get; private set; }

        [field: Space(10)]
        [field: Title("시야")]
        [field: LabelText("주변 시야"), Range(0f, 50f), SerializeField]
        public float NearRadius { get; private set; }

        [field: LabelText("시야각"), SerializeField]
        public float FovAngle { get; private set; }

        [field: LabelText("시야 거리"), Range(0f, 100f), SerializeField]
        public float FovRadius { get; private set; }

        [field: Space(10)]
        [field: Title("애니메이션")]
        [field: LabelText("애니메이션 키(영문명으로 입력)"), SerializeField]
        public SerializableDictionary<string, AnimationKey> AnimationKeys { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void Parse()
        {
            DataManager.onInitialized += OnDataInitialized;
        }

        public static void OnDataInitialized()
        {
            DataManager.onInitialized -= OnDataInitialized;
            ParseAsync().Forget();
        }
        
        public static async UniTask ParseAsync()
        {
            var datas = await GoogleSheetsHandler.UseCache((long)GoogleSheetsConstantData.ESheetPage.MonsterSettings,false);
            
            for (int i = 0; i < datas.Count; i++)
            {
                var data = datas[i];

                EnemyInfoContainer enemyData = new EnemyInfoContainer()
                {
                    ID = int.Parse(data["Monster_ID"]),
                    Quality = (EQuality)int.Parse(data["Monster_grade"]),
                    EnemyName = data["Monster_name"],
                    MaxHp = int.Parse(data["Monster_Hp"]),
                    Guard = float.Parse(data["Guard_Amount"]),
                    GuardRecovery = float.Parse(data["Guard_Recovery"]),
                    GuardBreakDamage = int.Parse(data["GuardBreak_Dmg"]),
                    ChaseSpeed = float.Parse(data["Chasing_Speed"]),
                    ChaseRadius = float.Parse(data["Chasing_Area_Max"])
                };

                if (!EnemyData.Instance.EnemyDatas.TryAdd(enemyData.ID, enemyData))
                {
                    EnemyData.Instance.EnemyDatas[enemyData.ID].ID = enemyData.ID;
                    EnemyData.Instance.EnemyDatas[enemyData.ID].Quality = enemyData.Quality;
                    EnemyData.Instance.EnemyDatas[enemyData.ID].EnemyName = enemyData.EnemyName;
                    EnemyData.Instance.EnemyDatas[enemyData.ID].MaxHp = enemyData.MaxHp;
                    EnemyData.Instance.EnemyDatas[enemyData.ID].Guard = enemyData.Guard;
                    EnemyData.Instance.EnemyDatas[enemyData.ID].GuardRecovery = enemyData.GuardRecovery;
                    EnemyData.Instance.EnemyDatas[enemyData.ID].GuardBreakDamage = enemyData.GuardBreakDamage;
                    EnemyData.Instance.EnemyDatas[enemyData.ID].ChaseSpeed = enemyData.ChaseSpeed;
                    EnemyData.Instance.EnemyDatas[enemyData.ID].ChaseRadius = enemyData.ChaseRadius;
                }
                
            }


            EnemyData.Instance.Initialized = true;
            EnemyData.Instance.Invoke();
        }
        
        
    }

    
}