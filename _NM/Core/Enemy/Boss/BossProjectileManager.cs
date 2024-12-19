using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using _NM.Core.Character;
using _NM.Core.Common.Combat;
using _NM.Core.Enemy.Boss;
using _NM.Core.Object;
using Cysharp.Threading.Tasks;
using UnityEngine;

public class BossProjectileManager : MonoBehaviour
{
    [SerializeField] private ObjectPool projectilePool;
    [SerializeField] private ObjectPool projectileExplosionPool;
    [SerializeField] private ObjectPool projectileEruptionPool;
    [SerializeField] private ObjectPool projectileNoticePool;
    private HashSet<int> pooledObject;
    [SerializeField] private ObjectBase owner;
    [SerializeField] private int fireCount;
    [SerializeField] private int currentFire;
    [SerializeField] private Transform FireTransform;
    [SerializeField] private Transform centerTransform;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Dictionary<int, EnemyProjectile> projectilesCache;
    [SerializeField] public bool PlayerHit { get; private set; }

    [SerializeField] private float testFireInterval;
    [SerializeField] private float testFireDuration;
    [SerializeField] private AnimationCurve testfireCurve;


    private void OnValidate()
    {
        if (!projectilePool)
        {
            projectilePool = GetComponent<ObjectPool>();
        }
    }

    void Start()
    {
        transform.parent = null;
        pooledObject = new();
        projectilesCache = new();
        PlayerHit = false;
        playerTransform = Character.Local?.transform;
    }

    private void OnEnable()
    {
        currentFire = 0;
    }

    private void SetPlayerHit(HitInfo info)
    {
        PlayerHit = true;
    }

    void ExplosionProjectile(GameObject projectile)
    {
        GameObject explosionObject = projectileExplosionPool.Get();
        if (!pooledObject.Contains(explosionObject.GetInstanceID()))
        {
            pooledObject.Add(explosionObject.GetInstanceID());
            explosionObject.GetComponent<PuttableEffect>().onPutObject += PutExplosionObject;
            Attack explosionAttack = explosionObject.GetComponent<Attack>();
            explosionAttack.SetOwner(owner);
            explosionAttack.onAttack += SetPlayerHit;

        }
        explosionObject.transform.position = projectile.transform.position;
        
        if (owner.Health.CurrentHpPercentage <= 50f)
        {
            GameObject eruptionObject = projectileEruptionPool.Get();
            if (!pooledObject.Contains(eruptionObject.GetInstanceID()))
            {
                pooledObject.Add(eruptionObject.GetInstanceID());
                eruptionObject.GetComponent<PuttableEffect>().onPutObject += PutEruptionObject;
                eruptionObject.GetComponent<Attack>().SetOwner(owner);
            }
            eruptionObject.transform.position = projectile.transform.position;
        }
    }
    

    void PutProjectile(GameObject projectile)
    {
        projectilePool.Put(projectile);
    }

    void PutExplosionObject(GameObject explosionObject)
    {
        projectileExplosionPool.Put(explosionObject);
    }

    void PutNoticeObject(GameObject noticeObject)
    {
        projectileNoticePool.Put(noticeObject);
    }

    void PutEruptionObject(GameObject eruptionObject)
    {
        projectileEruptionPool.Put(eruptionObject);
    }

    void ResetProjectile(Transform projectile)
    {
        projectile.position = FireTransform.position;
    }

    public void Fire(int projectilesPerFire,float radius = 10f, float angle = 50f, bool setTargetCenter = false)
    {
        for (int i = 0; i < projectilesPerFire; i++)
        {
            GameObject projectileObject = projectilePool.Get();
            if (!pooledObject.Contains( projectileObject.GetInstanceID()))
            {
                pooledObject.Add( projectileObject.GetInstanceID());
                EnemyProjectile projectile = projectileObject.GetComponent<EnemyProjectile>();
                projectilesCache.Add(projectileObject.GetInstanceID(), projectile);
                projectile.onProjectileEnter += ExplosionProjectile;
                projectile.onProjectileEnabled += ResetProjectile;
                projectile.onProjectileDisabled += PutProjectile;
            }

            if (setTargetCenter)
            {
                projectilesCache[projectileObject.GetInstanceID()].LaunchRadius = 30f;
                projectilesCache[projectileObject.GetInstanceID()].SetTarget(centerTransform);
                projectilesCache[projectileObject.GetInstanceID()].LaunchAngle = angle;
            }
            else
            {
                projectilesCache[projectileObject.GetInstanceID()].SetTarget(playerTransform);
                projectilesCache[projectileObject.GetInstanceID()].LaunchRadius = radius;
                projectilesCache[projectileObject.GetInstanceID()].LaunchAngle = angle;
            }
            
            projectileObject.SetActive(false);
            projectileObject.SetActive(true);

            

        }
    }

    public async UniTask FireRepeat(float duration, float interval, CancellationToken token, AnimationCurve fireCurve, float fireRadius = 10f)
    {
        float currentTime = 0f;
        float currentInterval = 0f;
        int launchCount = 0;
        PlayerHit = false;
        while (duration > currentTime)
        {
            
            float dt = Time.fixedDeltaTime;
            currentTime += dt;
            currentInterval += dt;
            if (currentInterval > interval)
            {
                if (launchCount % 6 == 0)
                {
                    Fire((int)fireCurve.Evaluate(currentTime/duration),radius:4);
                }
                else
                {
                    Fire((int)fireCurve.Evaluate(currentTime/duration),radius:fireRadius,setTargetCenter:true);
                }

                launchCount++;
                currentInterval = 0f;
            }

            await UniTask.WaitForFixedUpdate(token);
        }
        
    }

    public void FireTest()
    {
        CancellationTokenSource cts = new();
        FireRepeat(testFireDuration, testFireInterval, cts.Token, testfireCurve).Forget();
    }
}
