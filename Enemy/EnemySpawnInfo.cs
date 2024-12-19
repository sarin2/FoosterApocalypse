using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace _NM.Core.Enemy
{
    [CreateAssetMenu(fileName = "EnemySpawnInfo", menuName = "Scriptable Object/EnemySpawn")]
    public class EnemySpawnInfo : ScriptableObject
    {
        private static EnemySpawnInfo instance;
        public static EnemySpawnInfo Instance
        {
            get
            {
                if (instance == null)
                {
                    EnemySpawnInfo asset = Resources.Load<EnemySpawnInfo>("Data/Enemy/Spawner/EnemySpawnInfo");
                    if (asset == null)
                    {
                        throw new System.Exception("EnemySpawnInfo 인스턴스를 찾을 수 없습니다.");
                    }

                    instance = asset;
                }

                return instance;
            }
        }
        [field: Title("정보")] [field: LabelText("몬스터 정보 컨테이너"), SerializeField]
        public SerializedDictionary<ESpawnEnemyType, EnemySpawnInfoContainer> enemySpawnInfoContainers
        {
            get;
            private set;
        }
        
        
    }

    [Serializable]
    public struct EnemySpawnInfoContainer
    {
        [field: LabelText("스폰 종류"),SerializeField] public ESpawnType EnemySpawnType { get; private set; }
        [field: LabelText("거대 스폰 가능 여부"),SerializeField] public bool GiantEnemySpawnable { get; private set; }
        [field: ShowIf(nameof(GiantEnemySpawnable)), LabelText("거대 처치시 증감 여부"),SerializeField] public bool Variance { get; private set; }
        [field: LabelText("일반 몬스터 프리팹"),SerializeField] public GameObject CommonEnemyPrefab { get; private set; }
        [field: LabelText("거대 몬스터 프리팹"),SerializeField] public GameObject GiantEnemyPrefab { get; private set; }
        [field: LabelText("최소 생성 수"),SerializeField] public int SpawnMin { get; private set; }
        [field: LabelText("최대 생성 수"),SerializeField] public int SpawnMax { get; private set; }
        [field: LabelText("거대 몬스터 생성 주기 (일 단위)"),SerializeField] public int BigMonsterSpawnCycle { get; private set; }
        [field: LabelText("일반 몬스터 생성 주기"),SerializeField] public float CommonMonsterSpawnCycle { get; private set; }
        [field: LabelText("이동 공간 크기"),SerializeField] public float AreaRange { get; private set; }
        [field: LabelText("최대 생성 감소 수"),SerializeField] public int MDecreaseCount { get; private set; }
        [field: LabelText("최대 생성 증가 수"),SerializeField] public int MIncreaseCount { get; private set; }
        [field: LabelText("생성 주기 증가 시간"),SerializeField] public float CIncreaseTime { get; private set; }
        [field: LabelText("생성 주기 감소 시간"),SerializeField] public float CDecreaseTime { get; private set; }
        [field: LabelText("플레이어 감지 거리"),SerializeField] public float playerDetectDistance { get; private set; }
        

    }
}