using System;
using System.Collections.Generic;
using System.Linq;
using _NM.Core.Camera;
using _NM.Core.Common.Combat;
using _NM.Core.Data;
using _NM.Core.Enemy.AI.EnemyAI;
using _NM.Core.Object;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Pool;
using Random = UnityEngine.Random;
using _NM.Core.Item;
using _NM.Core.Manager;
using _NM.Core.Quest;
using _NM.Core.UI.Inventory;
using _NM.Core.Utils;
using BehaviorDesigner.Runtime;
using Sirenix.OdinInspector;
using UnityEngine.Playables;
using AnimationEvent = _NM.Core.Animation.AnimationEvent;

namespace _NM.Core.Enemy
{
    
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Health))]
    public class EnemyBase : ObjectBase
    {
        [field:SerializeField] public GameObject RootObject {get; private set;}
        [field:Header("애니메이션")]
        [field:SerializeField] public Animator Animator { get; private set; }
        [field:SerializeField] public AnimationEvent AnimationEvent { get; private set; }
        [field:SerializeField] public bool EnableIK { get; protected set; }
        [field:Header("애니메이션")]
        [field:SerializeField] public PlayableDirector Director { get; private set; }

        [field:Header("몬스터 ID")]
        [SerializeField] protected int enemyID;
        [field:SerializeField] public NavMeshAgent NavMeshAgent { get; private set; }
        [SerializeField] private EnemyInfoContainer enemyData;
        public IObjectPool<GameObject> Pool;
        [field: SerializeField] public bool SuperArmored { get; private set; }
        [SerializeField] protected float attackDelay;
        [SerializeField] protected List<ItemAcquireInfo> items;
        public bool IsEnemySpotted { get; protected set; }
        public bool IsDamaged { get; private set; }
        [field:SerializeField] public bool IsAttackReady { get; private set; }
        [SerializeField] protected BehaviorTree behaviorTree; 
        [SerializeField] private EffectManager enemyEffectManager;
        [SerializeField] private float areaRange;
        [SerializeField] private float AreaRange => areaRange;
        public ESpawnType SpawnType;
        [field:SerializeField] public Transform CurrentArea { get; protected set; }
        public Vector3 BeginPosition;
        [SerializeField] private HitInfo currentHitInfo;
        [SerializeField] protected Transform noticePosition;
        
        [field: Header("가드")]
        [field: SerializeField] public bool IsGuard { get; protected set; }
        [field: SerializeField] public float GuardGauge { get; protected set; }
        [field: SerializeField] public float GuardTime { get; private set; }
        [SerializeField] protected float guardRecoveryTime = 2f;
        [SerializeField] protected float guardRecoveryTotal = 0f; 
        [field:SerializeField] public bool BlockPlayer { get; protected set; }
        public EnemyInfoContainer DataInfo => enemyData;
        [field: Header("표정")]
        [field: SerializeField] public SerializableDictionary<string, Vector2> FacialOffset { get; private set; }
        [SerializeField] private SkinnedMeshRenderer eyeMesh;
        [field: Header("어택 트리거")]
        [SerializeField] protected SerializableDictionary<string, GameObject> AttackTriggers;
        [SerializeField] private Transform skObjectTransform;

        [field: Header("넉백")]
        [field:LabelText("에고 넉백 거리")] public float EgoKnockbackDist;
        [field:LabelText("에고 넉백 시간")] public float EgoKnockbackTime;

        [field: Header("디졸브")]
        [field: LabelText("디졸브 적용할 메시"),SerializeField] protected List<SkinnedMeshRenderer> dissolveMeshes;
        [field: LabelText("디졸브 시간"),SerializeField] protected float dissolveTime;
        [SerializeField] protected Material[] dissolveMeshMaterials;

        [field: LabelText("아이템 드랍 매니저"),SerializeField] private ItemTableManager itemTableManager;
        
        protected override void OnValidate()
        {
            base.OnValidate();
            if (!Animator)
            {
                if (skObjectTransform)
                {
                    Animator = skObjectTransform.GetComponent<Animator>();
                    AnimationEvent = skObjectTransform.GetComponent<AnimationEvent>();
                }
                else
                {
                    Animator = GetComponentInParent<Animator>();
                    AnimationEvent = GetComponentInParent<AnimationEvent>();
                }
                
            }
            if (!NavMeshAgent) NavMeshAgent = GetComponentInParent<NavMeshAgent>();
            if (TryGetComponent<Collider>(out var col))
            {
                col.isTrigger = true;
            }
            else
            {
                gameObject.AddComponent<CapsuleCollider>().isTrigger = true;
            }

            if (!Director)
            {
                Director = GetComponent<PlayableDirector>();
            }
        }

        public override void OnHit(HitInfo hitInfo)
        {
            base.OnHit(hitInfo);
            currentHitInfo = hitInfo;
            
            if (!health.IsAlive && QuestManager.I != null)
            {
                QuestManager.I.OnEnemySlain(DataInfo.ID, 1);
            }
            
        }

        protected virtual void Awake()
        {
            itemTableManager = FindObjectOfType<ItemTableManager>();
        }

        protected virtual void Start()
        {
            dissolveMeshMaterials = new Material[dissolveMeshes.Count];
            for (int i = 0; i < dissolveMeshes.Count; i++)
            {
                dissolveMeshMaterials[i] = dissolveMeshes[i].material;
            }
        }
        

        protected virtual void OnEnable()
        {
            BlockPlayer = true;
            GuardGauge = enemyData.Guard;
            foreach (var material in dissolveMeshMaterials)
            {
                material.SetFloat("_DissolveThreshold", -0.5f);
            }

            if (Animator)
            {
                Animator.Rebind();
            }
            
        }

        public void MoveTo(Vector3 position)
        {
            NavMeshAgent.SetDestination(position);
        }

        public void CombatReset()
        {
            IsEnemySpotted = false;
        }

        public void StopMove()
        {
            NavMeshAgent.isStopped = true;
        }
        
        private void OnAnimatorMove()
        {
            Vector3 position = Animator.rootPosition;
            position.y = NavMeshAgent.nextPosition.y;
            transform.position = position;
            NavMeshAgent.nextPosition = transform.position;
        }

        protected virtual void InitState()
        {
            
        }

        protected void GetStatFromInfo()
        {
            if (EnemyData.Instance.Initialized)
            {
                health.MaxHp = enemyData.MaxHp;
                health.CurrentHp = enemyData.MaxHp;
            }
            else
            {
                EnemyData.Instance.onDataInitialized += () =>
                {
                    health.MaxHp = enemyData.MaxHp;
                    health.CurrentHp = enemyData.MaxHp;
                    
                };
            }

        }
        
        protected void InitStat()
        {
            GetStatFromInfo();
        }

        protected void InitComponent()
        {
            if (!NavMeshAgent) NavMeshAgent = GetComponent<NavMeshAgent>();
            if (TryGetComponent<Collider>(out var col))
            {
                col.isTrigger = true;
            }
            else
            {
                gameObject.AddComponent<CapsuleCollider>().isTrigger = true;
            }
            
        }

        
        Vector3 AngleToDir(float angle)
        {
            float radian = angle * Mathf.Deg2Rad;
            return new Vector3(Mathf.Sin(radian), 0f, Mathf.Cos(radian));
        }
        
        /// <summary>
        /// BeginPosition 변수를 기준으로 랜덤한 위치로 이동하는 메소드입니다.
        /// 움직이는 거리는 Random.insideUnitSphere * areaRange 입니다.
        /// </summary>
        public Vector3 MoveRandomPosition()
        {
            Vector3 randomDirection = Random.insideUnitSphere * areaRange;
            randomDirection += BeginPosition;
 
            NavMeshHit hit;
            if (NavMesh.SamplePosition(randomDirection, out hit, 20f, NavMesh.AllAreas))
            {
                return hit.position;
            }
            else
            {
                return transform.position;
            }
        }
        
        
        
        

        public void SetSuperArmor(bool value)
        {
            SuperArmored = value;
        }

        public float GetAttackDelay()
        {
            return attackDelay;
        }


        public async UniTaskVoid RemoveEnemy(float startTime, float disappearTime)
        {
            await UniTask.Delay(TimeSpan.FromSeconds(startTime));
            bool result = await Disappear(disappearTime);
            CombatReset();
            NavMeshAgent.enabled = false;

            if (result && Pool != null)
            {
                if (RootObject)
                {
                    
                    Pool.Release(RootObject);
                }
                else
                {
                    Pool.Release(gameObject);
                }
                
            }
            else
            {
                Destroy(gameObject);
            }
        }
        public async UniTask<bool> Disappear(float disappearTime)
        {
            
            float total = 0f;
            while (disappearTime > total)
            {
                foreach (var material in dissolveMeshMaterials)
                {
                    material.SetFloat("_DissolveThreshold", total / disappearTime);
                }
                total += Time.fixedDeltaTime;
                await UniTask.WaitForFixedUpdate();
            }

            return true;
        }

        protected async UniTaskVoid DropItem(float time)
        {
            float total = 0f;
            while (total < time)
            {
                total += Time.deltaTime;
                await UniTask.Yield(destroyCancellationToken);
            }
            itemTableManager.Drop(enemyID);
            
        }
        
        public override void OnDamage(int damage, bool sendEvent = true)
        {

            if (sendEvent && behaviorTree && !IsGuard)
            {
                behaviorTree.SendEvent("OnDamage");
            }
            
            if (IsGuard)
            {
                if (currentHitInfo is { Type: HitType.BigKnife, Data: ComboData data })
                {
                    GuardGauge -= data.GuardDamage;
                }
                return;
            }
            base.OnDamage(damage);
        }


        public void SetAreaRange(float value)
        {
            areaRange = value;
        }

        public void SetAttackReady(bool state)
        {
            IsAttackReady = state;
        }
        
        private bool IsEqualSpecis(ObjectBase other)
        {
            EnemyBase target = other as EnemyBase;
            if (target == null || target.enemyData.ID != enemyData.ID)
            {
                return false;
            }

            return true;
        }

        public void SetArea(Transform areaTransform)
        {
            
            CurrentArea = areaTransform;
            CurrentArea.TryGetComponent(out AreaController areaController);
            areaController.AddEnemyChild(this);
            if (RootObject)
            {
                RootObject.transform.SetParent(areaTransform);   
            }
            else
            {
                transform.SetParent(areaTransform);
            }
            
        }
        public void SetEnemySpotted(bool state)
        {
            IsEnemySpotted = state;
        }

        public HitInfo GetCurrentHitInfo()
        {
            return currentHitInfo;
        }

        public void NoticeEnemySpotted()
        {
            VfxManager.I.Spawn(Vfx.EnemyNotice,noticePosition,.5f);
        }

        public void SetGuard(bool state)
        {
            IsGuard = state;
        }

        public void SetCurrentInfo(HitInfo hitInfo)
        {
            currentHitInfo = hitInfo;
        }

        public void SetGuardGauge(float amount)
        {
            GuardGauge = amount;
            GuardGauge = Mathf.Clamp(GuardGauge, 0, enemyData.Guard);
        }

        public void AddGuardGauge(float amount)
        {
            GuardGauge += amount;
            GuardGauge = Mathf.Clamp(GuardGauge, 0, enemyData.Guard);
        }

        protected void GetEnemyData()
        {
            enemyData = EnemyData.Instance.GetEnemyData(enemyID);
        }

        public void SetPlayerBlock(bool state)
        {
            BlockPlayer = state;
        }
        

        public void ChangeFacial(string state)
        {
            if (eyeMesh && FacialOffset.TryGetValue(state, out var value))
            {
                eyeMesh.material.SetVector("_Offset",value);
                eyeMesh.material.SetTextureOffset("_BaseMap",value);
            }
        }
        public Transform GetSKObjectTransform()
        {
            return skObjectTransform;
        }

        public void SendBehaviorTreeEvent(string eventName)
        {
            behaviorTree.SendEvent(eventName);
        }

        public void ShakingCamera(ShakeInfo info)
        {
            
        }
        
    }
}
