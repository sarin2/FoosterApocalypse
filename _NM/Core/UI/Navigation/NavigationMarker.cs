using SDUnityExtension.Scripts.Extension;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.UI.Navigation
{
    public class NavigationMarker : MonoBehaviour
    {
        public Vector3 TargetPosition => targetPosition;
        public ENavigationType NavigationType => navigationType;
        
        private Vector3 targetPosition;
        private ENavigationType navigationType;
        
        [Header("Components")]
        [SerializeField] private Image[] markerImages;
        [SerializeField] private RectTransform arrowTransform;

        [Header("Values")] 
        [LabelText("기본 스케일 값"), SerializeField] private float defaultScale = 1f;
        [LabelText("화면 밖에 나갔을 때 스케일 값"), SerializeField] private float outOfScreenScale = 0.5f;
        
        public void Initialize(Vector3 position, ENavigationType type)
        {
            targetPosition = position;
            SetType(type);
        }

        public void SetArrowAngle(float angle)
        {
            arrowTransform.eulerAngles = new Vector3(0, 0, angle);
        }
        
        public void ShowArrow(bool show)
        {
            arrowTransform.gameObject.SetActive(show);
            transform.localScale = show ? Vector3.one * outOfScreenScale : Vector3.one * defaultScale;
        }

        public void SetType(ENavigationType type)
        {
            markerImages.ForEach(image => image.gameObject.SetActive(false));
            navigationType = type;
            markerImages[(int)navigationType].gameObject.SetActive(true);
        }
    }
}
