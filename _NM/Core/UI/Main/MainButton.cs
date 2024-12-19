using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;

namespace _NM.Core.UI.Main
{
    public class MainButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private RectTransform rectTransform;

        private void Awake()
        {
            if (rectTransform == null)
                rectTransform = transform as RectTransform;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            rectTransform.DOAnchorPosX(25, 0.25f).SetEase(Ease.OutQuad);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            rectTransform.DOAnchorPosX(0, 0.25f).SetEase(Ease.OutQuad);
        }
    }
}
