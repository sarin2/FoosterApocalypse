using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.UI.Common
{
    public class SkillProgressUI : ProgressUI
    {
        [SerializeField] private Image skillImage;
        [SerializeField] private Sprite defaultSprite;
        [SerializeField] private Sprite skillReadySprite;

        [SerializeField] private float smallKnifeSkillCost = 0f;
        [SerializeField] private float bigKnifeSkillCost = 0f;
        
        protected override void OnAmountChanged(float prev, float cur)
        {
            base.OnAmountChanged(prev, cur);

            var isReady = cur >= smallKnifeSkillCost && cur >= bigKnifeSkillCost;
            skillImage.sprite = isReady ? skillReadySprite : defaultSprite;
        }
    }
}
