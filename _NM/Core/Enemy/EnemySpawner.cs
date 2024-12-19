using System;
using System.Collections.Generic;
using _NM.Core.Enemy;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using Random = UnityEngine.Random;



public enum ESpawnType
{
    Single,
    Multiple
}

public enum ESpawnEnemyType
{
    None,
    EggSlime,
    Takat,
    Sirutoad,
    BreadBear,
    Eggslime_NonGuard,
    Eggslime_OnlyGuard,
    Takat_NonGuard,
    Takat_OnlyGuard,
}

public class EnemySpawner : MonoBehaviour
{
    [LabelText("스폰 몬스터"),SerializeField]
    private ESpawnEnemyType monsterType;
    
    [HideInInspector] private GameObject commonEnemy;
    [HideInInspector] private GameObject giantEnemy;
    [HideInInspector] private int enemyCount = 0;
    [HideInInspector] private int giantCount = 0;
    [HideInInspector] private float respawnTick;
    [HideInInspector] private IObjectPool<GameObject> enemyPool;
    [HideInInspector] private IObjectPool<GameObject> giantPool;
    [HideInInspector] private bool collectionChecks = true;
    [HideInInspector] private Vector3 respawnPosition;
    [HideInInspector] private EnemySpawnInfoContainer spawnInfo;

    [HideInInspector] private static int monsterAllCount = 0;
    [HideInInspector] private static int giantAllCount = 0;
    
    [LabelText("재정의"),SerializeField] private bool overrided;
    [LabelText("리스폰 여부"),ShowIf(nameof(overrided)),SerializeField] private bool respawn;
    [Range(0f,99f)]
    [LabelText("리스폰 시간"),ShowIf(nameof(overrided)),SerializeField] private float respawnTime;
    [LabelText("스폰 최소 마리 수"),ShowIf(nameof(overrided)),SerializeField] private int spawnMinCount;
    [LabelText("스폰 최대 마리 수"),ShowIf(nameof(overrided)),SerializeField] private int spawnMaxCount;
    [LabelText("스폰 한번에 생성되는 수"),ShowIf(nameof(overrided)),SerializeField] private int spawnQuantity = 1;
    [LabelText("거대 몬스터 스폰 최소 마리 수"),ShowIf(nameof(overrided)),SerializeField] private int spawnGiantMinCount;
    [LabelText("거대 몬스터 스폰 최대 마리 수"),ShowIf(nameof(overrided)),SerializeField] private int spawnGiantMaxCount;
    [Range(0f,99f)]
    [LabelText("거대 몬스터 리스폰 시간"),ShowIf(nameof(overrided)),SerializeField] private float giantSpawnTime = 0;
    [LabelText("로밍 구역"),ShowIf(nameof(overrided)),SerializeField] private Transform roamingArea;
    [LabelText("로밍 구역 범위"),ShowIf(nameof(overrided)),SerializeField] private float areaRange;
    
    [LabelText("오류 표시"),SerializeField] private bool errorNoticed = false;
    
    [SerializeField] public float AreaRange => areaRange;
    [ShowIf(nameof(overrided)),LabelText("거대 몬스터 스폰 여부"), SerializeField] private bool giantSpawnable;
    [LabelText("거대 몬스터 처치 시 변화"),ShowIf(nameof(overrided)),SerializeField] private bool variance;

    [LabelText("몬스터 스폰 주기 증감"), SerializeField] private float varianceTime;
    [LabelText("몬스터 스폰 수 증감"), SerializeField] private int varianceCount;
    private void OnValidate()
    {
        spawnMinCount = Mathf.Clamp(spawnMinCount, 1, spawnMaxCount);
        spawnMaxCount = Mathf.Clamp(spawnMaxCount, spawnMinCount, 99);
        spawnQuantity = Mathf.Clamp(spawnQuantity, 1, spawnMaxCount);
        spawnGiantMinCount = Mathf.Clamp(spawnGiantMinCount, 1, spawnGiantMaxCount);
        spawnGiantMaxCount = Mathf.Clamp(spawnGiantMaxCount, spawnGiantMinCount, 99);
        giantSpawnTime = Mathf.Clamp(giantSpawnTime, 0f, 99f);
        respawnTime = Mathf.Clamp(respawnTime, 0f, 99f);
        areaRange = Mathf.Clamp(areaRange, 1f, 99f);

        varianceTime = Mathf.Clamp(varianceTime, -respawnTime, 99f);
        varianceCount = Mathf.Clamp(varianceCount, -(spawnMaxCount - spawnMinCount), 99);
    }

    public IObjectPool<GameObject> EnemyPool
    {
        get
        {
            if (enemyPool == null)
            {
                enemyPool = new ObjectPool<GameObject>(CreatePooledCommonEnemy, OnTakeFromPool, OnReturnedToPool,
                    OnDestroyPoolObject, collectionChecks, spawnMinCount, spawnMaxCount);
            }

            return enemyPool;
        }
    }
    
    public IObjectPool<GameObject> GiantPool
    {
        get
        {
            if (giantPool == null)
            {
                giantPool = new ObjectPool<GameObject>(CreatePooledGiantEnemy, OnTakeFromPool, OnReturnedToPool,
                    OnDestroyPoolObject, collectionChecks, spawnGiantMinCount, spawnGiantMaxCount);
            }

            return giantPool;
        }
    }
    

    GameObject CreatePooledCommonEnemy()
    {
        GameObject go = Instantiate(commonEnemy,transform.position,quaternion.identity);
        go.name = commonEnemy.name + monsterAllCount++;
        if (go.TryGetComponent(out EnemyBase enemyBase))
        {
            enemyBase.Pool = enemyPool;
            enemyBase.BeginPosition = transform.position + (Random.insideUnitSphere * areaRange);
            enemyBase.BeginPosition.y = transform.position.y;
            enemyBase.SpawnType = ESpawnType.Multiple;
            enemyBase.SetAreaRange(areaRange);
            enemyBase.SetArea(roamingArea);
        }

        return go;
    }
    
    GameObject CreatePooledGiantEnemy()
    {
        GameObject go = Instantiate(giantEnemy,transform.position,quaternion.identity);
        go.name = giantEnemy.name + giantAllCount++;
        if (go.TryGetComponent(out EnemyBase enemyBase))
        {
            enemyBase.Pool = GiantPool;
            enemyBase.BeginPosition = transform.position + (Random.insideUnitSphere * areaRange);
            enemyBase.BeginPosition.y = transform.position.y;
            enemyBase.SpawnType = ESpawnType.Single;
            enemyBase.SetAreaRange(areaRange);
            enemyBase.SetArea(roamingArea);
        }

        return go;
    }

    void OnReturnedToPool(GameObject enemy)
    {
        enemy.SetActive(false);
        if (enemy.TryGetComponent(out EnemyBase enemyBase))
        {
            switch (enemyBase.SpawnType)
            {
                case ESpawnType.Multiple: 
                    enemyCount--;
                    break;
                case ESpawnType.Single:
                    SetSpawnVariance();
                    giantCount--;
                    break;
            }
        }
    }

    void OnTakeFromPool(GameObject enemy)
    {
        enemy.SetActive(true);
        if (enemy.TryGetComponent(out EnemyBase enemyBase))
        {
            switch (enemyBase.SpawnType)
            {
                case ESpawnType.Multiple: 
                    enemyCount++;
                    break;
                case ESpawnType.Single:
                    giantCount++;
                    break;
            }
        }
    }

    void OnDestroyPoolObject(GameObject enemy)
    {
        Destroy(enemy);
    }

    private void Awake()
    {
        spawnInfo = EnemySpawnInfo.Instance.enemySpawnInfoContainers[monsterType];
        #if UNITY_EDITOR
        giantSpawnTime = 0;
        #endif
        Initialize();
        giantAllCount = 0;
        monsterAllCount = 0;
    }

    private void Initialize()
    {
        if (monsterType != ESpawnEnemyType.None)
        {
            commonEnemy = spawnInfo.CommonEnemyPrefab;
            giantEnemy = spawnInfo.GiantEnemyPrefab;
            if (respawnTime == 0f)
            {
                respawnTime = spawnInfo.CommonMonsterSpawnCycle;
            }

            if (spawnMinCount == 0)
            {
                spawnMinCount = spawnInfo.SpawnMin;
            }

            if (spawnMaxCount == 0)
            {
                spawnMaxCount = spawnInfo.SpawnMax;
            }
            
            spawnGiantMinCount = 1;
            spawnGiantMaxCount = 1;

            if (areaRange == 0f)
            {
                areaRange = spawnInfo.AreaRange;
            }
            
        }
        
    }


    private void Update()
    {

        if (roamingArea != null)
        {
            if (giantSpawnTime == 0 && giantSpawnable)
            {
                respawnPosition = transform.position + (Random.insideUnitSphere * areaRange);
                respawnPosition.y = transform.position.y;
            
                GameObject go = GiantPool.Get();
            
                go.TryGetComponent(out NavMeshAgent navMeshAgent);
                navMeshAgent.Warp(respawnPosition);

                giantSpawnTime += spawnInfo.BigMonsterSpawnCycle;

            }
        
            if (respawn && enemyCount < spawnMaxCount)
            {
                respawnTick += Time.unscaledDeltaTime;
            
                if (respawnTick > respawnTime)
                {
                    respawnPosition = transform.position + (Random.insideUnitSphere * areaRange);
                    respawnPosition.y = transform.position.y;

                    for (int i = 1; i <= spawnQuantity; i++)
                    {
                        GameObject go = EnemyPool.Get();

                        go.TryGetComponent(out NavMeshAgent navMeshAgent);
                        navMeshAgent.Warp(respawnPosition);
                    }

                    respawnTick = 0;
                }
            }
        }
        else
        {
            if (!errorNoticed)
            {
                Debug.LogError("스포너의 배회 지역을 설정하지 않았습니다. 오류 오브젝트 : " + gameObject.name);
                errorNoticed = true;
            }
        }

    }

    private void SetSpawnVariance()
    {
        respawnTime += varianceTime;
        spawnMaxCount += varianceCount;
    }
}
