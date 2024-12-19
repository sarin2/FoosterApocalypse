using UnityEngine;

namespace _NM.Core.Item
{
    [CreateAssetMenu(fileName = "ItemAcquireCurve", menuName = "Scriptable Object/ItemAcquireCurve")]
    public class ItemAcquireCurve : ScriptableObject
    {
        public AnimationCurve AcquireCurve;
    }
}