using System;
using System.Collections.Generic;
using SDUnityExtension.Scripts.Extension;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _NM.Core.Cook
{
    [Serializable]
    public struct RequireItemInfo
    {
        public long MaterialID;
        public int RequireCount;
        public Sprite MaterialImage;
    }
    
    [CreateAssetMenu(fileName = "CookingRequireData", menuName = "Scriptable Object/CookingRequireData")]
    public class CookMaterialData : ScriptableObject
    {
        [field: LabelText("필요한 재료 ID 목록"),SerializeField] public List<RequireItemInfo> MaterialList { get; private set; }
        [field: LabelText("결과 아이템 ID"), SerializeField] public long CookedItemID { get; private set; }
    }
}