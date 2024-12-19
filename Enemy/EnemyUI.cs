using System;
using System.Threading;
using _NM.Core.Common.Combat;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.Enemy
{
    public class EnemyUI : MonoBehaviour
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private Health ownerHealth;
        [SerializeField] private Transform owner;
        [SerializeField] private UnityEngine.Camera mainCamera;
        [SerializeField] private Image hpBar;
        [SerializeField] private Image prevHPBar;
        [SerializeField] private CanvasGroup fade;
        [SerializeField] private RectTransform iconRect;
        [SerializeField] private CancellationTokenSource hpCts;
        [SerializeField] private float startTime = 1.0f;
        [SerializeField] private bool fixedTransform;


        private void Awake()
        {
            EnemyBroadcastManager.I[gameObject.GetInstanceID(), "SetMainCamera"] += SetMainCamera;
            owner = transform.parent;
            fade = GetComponent<CanvasGroup>();
            fade.alpha = 0;
            hpCts = new CancellationTokenSource();
            if (!mainCamera)
            {
                mainCamera = UnityEngine.Camera.main;
            }
        }

        private void Start()
        {
            if (!ownerHealth)
            {
                ownerHealth = owner.GetComponentInParent<Health>();
            }
            
            LayoutRebuilder.ForceRebuildLayoutImmediate(iconRect);
            
            ownerHealth.onHpChanged += (prev, current) =>
            {
                hpBar.fillAmount = (float)ownerHealth.CurrentHp / ownerHealth.MaxHp;
                if (prev != current)
                {
                    startTime = 1.0f;
                    if (!hpCts.IsCancellationRequested)
                    {
                        hpCts.Cancel();
                        hpCts = new CancellationTokenSource();
                    }
                    ProgressHp(hpCts.Token).Forget();
                }
            };
            rectTransform.gameObject.SetActive(true);
            
        }

        private void Update()
        {
            if (mainCamera && !fixedTransform)
            {
                Vector3 ownerPosInScreen = mainCamera.WorldToScreenPoint(owner.position,mainCamera.stereoActiveEye);
                rectTransform.position = ownerPosInScreen;
            }
            
            float distFromPlayer = Vector3.Distance(owner.position, Character.Character.Local.transform.position);
            
            if (distFromPlayer < 20f)
            {
                fade.alpha = 10 / distFromPlayer;
            }
            else
            {
                fade.alpha = 0;
            }
            
            if (rectTransform.position.z < 0f)
            {
                fade.alpha = 0;
            }

        }

        private async UniTaskVoid ProgressHp(CancellationToken token)
        {
            startTime = 0.5f;
            float totalTime = 0.3f;
            float currentTime = 0f;
            while (prevHPBar.fillAmount > hpBar.fillAmount && !token.IsCancellationRequested)
            {
                if (startTime > 0f)
                {
                    startTime -= Time.deltaTime * 2;
                    await UniTask.Yield();
                    continue;
                }
                
                currentTime += Time.deltaTime;
                float result = Mathf.Lerp(prevHPBar.fillAmount, hpBar.fillAmount, currentTime / totalTime);

                prevHPBar.fillAmount = result;
                await UniTask.Yield();
            }
        }

        private void OnEnable()
        {
            prevHPBar.fillAmount = 1.0f;
        }

        private void SetMainCamera()
        {
            mainCamera = UnityEngine.Camera.main;
        }

        private void OnDestroy()
        {
            if (EnemyBroadcastManager.I)
            {
                EnemyBroadcastManager.I.UnregisterInstance(GetInstanceID());
            }
            
        }
    }
}
