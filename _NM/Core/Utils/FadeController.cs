using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

namespace _NM.Core.Utils
{
    public class FadeController : MonoBehaviour
    {
        public bool IsFading { get; private set; }
        public float FadeAlpha
        {
            get => fade.alpha;
            set => fade.alpha = value;
        }
        
        [SerializeField] private CanvasGroup fade;
        
        private void Awake()
        {
            if (!fade)
            {
                fade = GetComponentInChildren<CanvasGroup>();
            }
        }

        public async UniTask ChangeAlpha(float from, float to)
        {
            IsFading = true;
            fade.DOKill();
            FadeAlpha = from;
            await fade.DOFade(to, 0.5f);
            IsFading = false;
        }
    }
}
