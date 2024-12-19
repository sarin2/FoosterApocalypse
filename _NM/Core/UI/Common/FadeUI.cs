using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.UI.Common
{
    [RequireComponent(typeof(CanvasGroup))]
    public class FadeUI : MonoBehaviour
    {
        public static FadeUI Instance { get; private set; }
        
        public float FadeDuration => fadeDuration;
        public event Action onFadeIn;
        public event Action onFadeOut;
        
        [SerializeField] private CanvasGroup fadeCanvas;
        [SerializeField] private Graphic targetGraphics;
        [SerializeField] private float fadeDuration = 0.5f;
        [SerializeField] private bool useAsSingleton = false;

        private void OnValidate()
        {
            if (fadeCanvas == null) fadeCanvas = GetComponent<CanvasGroup>();
            if (targetGraphics == null) targetGraphics = GetComponentInChildren<Graphic>();
        }

        private void Awake()
        {
            if (useAsSingleton)
            {
                Instance = this;
            }

            FadeIn(0).Forget();
        }

        /// <summary>
        /// 게임 화면이 서서히 나타납니다.
        /// </summary>
        public async UniTask FadeIn(float duration = -1, Color? color = null)
        {
            fadeCanvas.alpha = 1;
            await DoFade(0, duration, color);
            onFadeIn?.Invoke();
        }
    
        /// <summary>
        /// 게임 화면이 서서히 사라집니다.
        /// </summary>
        public async UniTask FadeOut(float duration = -1, Color? color = null)
        {
            fadeCanvas.alpha = 0;
            await DoFade(1, duration, color);
            onFadeOut?.Invoke();
        }

        /// <summary>
        /// 게임 화면이 서서히 사라졌다 다시 나타납니다.
        /// 중간에 딜레이를 줄 수 있습니다.
        /// </summary>
        public async UniTask FadeOutAndIn(float waitDelay, float outDuration = -1, Color? outColor = null, float inDuration = -1, Color? inColor = null)
        {
            await FadeOut(outDuration, outColor);
            await UniTask.Delay(TimeSpan.FromSeconds(waitDelay));
            await FadeIn(inDuration, inColor);
        }

        private async UniTask DoFade(float alpha, float duration = -1, Color? color = null)
        {
            fadeCanvas.interactable = true;
            fadeCanvas.blocksRaycasts = true;
            if (targetGraphics != null)
            {
                targetGraphics.color = color ?? Color.black;
            }
            await fadeCanvas.DOFade(alpha, duration < 0 ? FadeDuration : duration).AsyncWaitForCompletion();
            fadeCanvas.interactable = false;
            fadeCanvas.blocksRaycasts = false;
        }
    }
}
