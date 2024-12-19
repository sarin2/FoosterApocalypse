using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.UI.Cooking
{
    public class RequiredSlot : MonoBehaviour
    {
        [SerializeField] private Image itemBackgroundImage;
        [SerializeField] private Image itemIconImage;
        [SerializeField] private TextMeshProUGUI countText;
        [SerializeField] private Sprite possibleSprite;
        [SerializeField] private Sprite impossibleSprite;

        public Sprite ItemIcon
        {
            get => itemIconImage.sprite;
            set => itemIconImage.sprite = value;
        }
        public string CountText   
        {
            get => countText.text;
            set => countText.text = value;
        }
        public Color TextColor
        {
            get => countText.color;
            set => countText.color = value;
        }
        
        public Sprite BackgroundSprite
        {
            get => itemBackgroundImage.sprite;
            set => itemBackgroundImage.sprite = value;
        }

        public void SetInfo(Sprite sprite, int currentCount, int requireCount)
        {
            ItemIcon = sprite;

            BackgroundSprite = currentCount >= requireCount ? possibleSprite : impossibleSprite;

            CountText = $"{currentCount}/{requireCount}";

        }
    }
}
