using System;
using System.Collections.Generic;
using System.Linq;
using _NM.Core.Camera;
using _NM.Core.Enemy.Data;
using _NM.Core.Utils;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Timeline;
using AnimationEvent = _NM.Core.Animation.AnimationEvent;

namespace _NM.Core.Enemy.Type
{
    public class SpicyTurtle : EnemyBase
    {
        [SerializeField] private AnimationEvent animationEvent;
        [SerializeField] private MonsterIKController ikController;

        [SerializeField] private CameraController cameraController;
        [SerializeField] private BossCameraShakeData bossSkillData;
        [SerializeField] private BossProjectileManager projectileManager;
        [SerializeField] private Transform headTransform;
        
        [Header("가드 브레이크")]
        [SerializeField] private Renderer guardBreakRenderer;
        private MaterialPropertyBlock guardBreakMpb;
        [SerializeField] private string materialKey = "_Gaurd_Break";
        [SerializeField] private float guardBreakRatio;
        
 
        [field:SerializeField,Header("볼케이노 사용 HP")] public SerializableDictionary<int,bool> VolcanoHPDict { get; private set; }
        [SerializeField] private string volcanoKey = "Volcano";
        [field:SerializeField] public EnemyRoarManager RoarManager { get; private set; }
        protected override void Awake()
        {
            base.Awake();
            EnemyBroadcastManager.I[gameObject.GetInstanceID(), "OnBossDead"] += FadeToEnding;
            animationEvent["LeftWeight"] += animEvent =>
            {
                float weight = animEvent.floatParameter;
                int time = animEvent.intParameter;

                if (EnableIK)
                {
                    ikController.SetWeightWithTime(MonsterIKController.IKPosition.Left,time,weight).Forget();
                }
                
            };

            animationEvent["FireProjectile"] += animEvent =>
            {
                projectileManager.Fire(animEvent.intParameter);
            };

            animationEvent["OnDeath"] += _ =>
            {
                EnemyBroadcastManager.I.SendEvent("OnBossDead",0);
            };
            guardBreakMpb = new();
            if (guardBreakRenderer)
            {
                guardBreakRenderer.GetPropertyBlock(guardBreakMpb);
                guardBreakMpb.SetFloat(materialKey,0f);
                guardBreakRenderer.SetPropertyBlock(guardBreakMpb);
            }
        }

        protected override void OnValidate()
        {
            base.OnValidate();

            if (!RoarManager)
            {
                RoarManager = GetComponent<EnemyRoarManager>();
            }
        }


        protected override void Start()
        {
            cameraController = Character.Character.Local.GetComponent<CameraController>();
            GetEnemyData();
            GetStatFromInfo();

        }

        public override void OnDamage(int damage, bool sendEvent = true)
        {
            base.OnDamage(damage, sendEvent);
            guardBreakRatio = 1f - GuardGauge / DataInfo.Guard;
            guardBreakMpb?.SetFloat(materialKey,guardBreakRatio);
            guardBreakRenderer?.SetPropertyBlock(guardBreakMpb);

            var pair = VolcanoHPDict.FirstOrDefault(pair => pair.Key >= Health.CurrentHpPercentage &&!pair.Value);
            if (pair.Key == 0)
            {
                return;
            }
            behaviorTree.SetVariableValue(volcanoKey,true);
            VolcanoHPDict[pair.Key] = true;
        }

        private void Update()
        {
            if (!IsGuard && GuardGauge < DataInfo.Guard)
            {
                GuardGauge += DataInfo.GuardRecovery * Time.deltaTime;
                if (guardBreakRenderer)
                {
                    guardBreakMpb.SetFloat(materialKey,0f);
                    guardBreakRenderer.SetPropertyBlock(guardBreakMpb);
                }
            }
        }
        
        public void FadeToEnding()
        {
            SceneLoadingController.Instance.ChangeScene(SceneName.BossDeadCutScene, false, true, Color.black);
        }
    }
}