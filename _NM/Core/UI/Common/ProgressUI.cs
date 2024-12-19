using DG.Tweening;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.UI.Common
{
    public class ProgressUI : MonoBehaviour
    {
        [ShowIf(nameof(useBarAnimation)), SerializeField] private Image backgroundImage;
        [SerializeField] private Image foregroundImage;
        
        [SerializeField] private bool useBarAnimation = true;
        [ShowIf(nameof(useBarAnimation)), SerializeField] private float increaseTime = 0.15f;
        [ShowIf(nameof(useBarAnimation)), SerializeField] private float increaseDelay = 0f;
        [ShowIf(nameof(useBarAnimation)), SerializeField] private float decreaseTime = 0.5f;
        [ShowIf(nameof(useBarAnimation)), SerializeField] private float decreaseDelay = 1f;
        [ShowIf(nameof(useBarAnimation)), SerializeField] private Ease fillEase = Ease.Linear;

        public float Amount
        {
            get => foregroundImage.fillAmount;
            set
            {
                var prevAmount = foregroundImage.fillAmount;
                foregroundImage.fillAmount = value;
                OnAmountChanged(prevAmount, foregroundImage.fillAmount);
            }
        }
        
        protected virtual void OnAmountChanged(float prev, float cur)
        {
            if (useBarAnimation)
            {
                var increase = cur > prev;
                var duration = increase ? increaseTime : decreaseTime;
                var delay = increase ? increaseDelay : decreaseDelay;
                backgroundImage.DOKill();
                backgroundImage.DOFillAmount(cur, duration).SetDelay(delay).SetEase(fillEase);
            }
        }
    }
}
