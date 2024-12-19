using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _NM.Core.Enemy.Boss
{
    public class EnemyProjectile : MonoBehaviour
    {

        [SerializeField] private Transform targetObjectTF;
        [field:LabelText("발사 각도"),Range(20.0f, 75.0f)] public float LaunchAngle;
        [field:LabelText("발사 범위"),Range(10.0f, 30.0f)] public float LaunchRadius;
        [SerializeField] private ParticleSystem effect;
    
        [SerializeField] private Rigidbody rigid;
        private Vector3 initialPosition;
        private Quaternion initialRotation;
        private bool isFirstFire;
        [Header("발사 UI")]
        [SerializeField] private GameObject DestNotice;
        [SerializeField] private Renderer noticeRenderer;
        private MaterialPropertyBlock noticeMpb;
        [SerializeField] private string matPropertyKey = "_inner_radius";
        [SerializeField] private float ratio;
        [SerializeField] private float currentUITime;
        [SerializeField] private float flightTime;
        [field:SerializeField] public Vector3 NoticeOffset { get; private set; }
        private Vector3 targetXZPos;
        private Vector3 projectileXZPos;
        [SerializeField] private bool disappear;
        [SerializeField] private float disappearTime;
        [SerializeField] private float currentDisappearTime;
        public event Action<GameObject> onProjectileEnter;
        public event Action<GameObject> onProjectileDisabled;
        public event Action<Transform> onProjectileEnabled;

        private void OnValidate()
        {
            if (!effect)
            {
                effect = GetComponentInChildren<ParticleSystem>();
            }

            if (!rigid)
            {
                rigid = GetComponent<Rigidbody>();
            }
            if (!noticeRenderer && DestNotice)
            {
                noticeRenderer = DestNotice.GetComponent<Renderer>();
            }
            
        }

        public void SetTarget(Transform target)
        {
            targetObjectTF = target;
        }

        private void Awake()
        {
            isFirstFire = true;
            if (!targetObjectTF)
            {
                targetObjectTF = Character.Character.Local.transform;
            }
            disappear = false;
            noticeMpb = new MaterialPropertyBlock();
            noticeRenderer.GetPropertyBlock(noticeMpb);
            ratio = noticeRenderer.sharedMaterial.GetFloat(matPropertyKey);
        }

        private void Start()
        {
            initialPosition = transform.position;
            initialRotation = transform.rotation;
            onProjectileEnabled?.Invoke(transform);
            ResetToInitialState();
            currentUITime = 0f;
            Launch();
            isFirstFire = false;
        }

        private void OnEnable()
        {
            initialPosition = transform.position;
            currentUITime = 0f;
            disappear = false;
            if (!isFirstFire)
            {
                ResetToInitialState();
                onProjectileEnabled?.Invoke(transform);
                Launch();
            }

        }

        private void OnDisable()
        {
            onProjectileEnabled?.Invoke(transform);
        }

        void ResetToInitialState()
        {
            rigid.velocity = Vector3.zero;
            transform.rotation = initialRotation;
        }

        void Update ()
        {
            transform.rotation = Quaternion.LookRotation(rigid.velocity) * initialRotation;
            currentUITime += Time.deltaTime;
            if (disappear && currentDisappearTime > disappearTime)
            {
                onProjectileDisabled?.Invoke(gameObject);
                currentDisappearTime = 0f;
            }
            else if (disappear && currentDisappearTime <= disappearTime)
            {
                currentDisappearTime += Time.deltaTime;
            }
            CalculateUI();
        }


        public void Launch()
        { 
            
            projectileXZPos = transform.position;
            targetXZPos = new Vector3(targetObjectTF.position.x + Random.Range(-LaunchRadius,LaunchRadius), transform.position.y, targetObjectTF.position.z + Random.Range(-LaunchRadius,LaunchRadius));
            DestNotice.SetActive(true);
            DestNotice.transform.position = new Vector3(targetXZPos.x, targetObjectTF.position.y, targetXZPos.z) + NoticeOffset;
            transform.LookAt(targetXZPos);
            float range = Vector3.Distance(projectileXZPos, targetXZPos);
            float gravity = Physics.gravity.y;
            float tanAlpha = Mathf.Tan(LaunchAngle * Mathf.Deg2Rad);
            float height = targetObjectTF.position.y - transform.position.y;
        
            float velocityZ = Mathf.Sqrt(gravity * range * range / (2.0f * (height - range * tanAlpha)) );
            float velocityY = tanAlpha * velocityZ;
        
            Vector3 localVelocity = new Vector3(0f, velocityY, velocityZ);
            Vector3 globalVelocity = transform.TransformDirection(localVelocity);
        
            rigid.velocity = globalVelocity;
            
            float initialVelocityY = globalVelocity.y * Mathf.Sin(LaunchAngle * Mathf.Deg2Rad);
            flightTime = (initialVelocityY + Mathf.Sqrt(initialVelocityY * initialVelocityY - 2 * gravity * (initialPosition.y - targetObjectTF.position.y))) / -gravity;
        }

        private void CalculateUI()
        {
            float currentRatio = currentUITime / flightTime * ratio;
            noticeMpb.SetFloat(matPropertyKey,currentRatio);
            noticeRenderer.SetPropertyBlock(noticeMpb);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!disappear)
            {
                disappear = true;
                currentDisappearTime = 0f;
                onProjectileEnter?.Invoke(gameObject);
                DestNotice.SetActive(false);
            }

        }
    }
}