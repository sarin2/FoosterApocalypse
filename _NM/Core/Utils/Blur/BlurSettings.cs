using Sirenix.OdinInspector;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace _NM.Core.Utils.Blur
{
    [System.Serializable, VolumeComponentMenu("Blur")]
    public class BlurSettings : VolumeComponent, IPostProcessComponent
    {
        [field: LabelText("블러 강도")]
        public ClampedFloatParameter strength = new ClampedFloatParameter(0.0f, 0.0f, 15.0f);

        public bool IsActive()
        {
            return (strength.value > 0.0f) && active;
        }

        public bool IsTileCompatible()
        {
            return false;
        }
    }
}