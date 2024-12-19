using System;
using _NM.Core.Utils;
using Sirenix.OdinInspector;
using UnityEngine;

namespace _NM.Core.Item
{
    [Serializable]
    public class ItemIcon
    {
        public Sprite sprite_64;
        public Sprite sprite_256;
        public Sprite sprite_512;
    }

    [CreateAssetMenu(fileName = "Item Icon Table", menuName = "Scriptable Object/Item Icon Table")]
    public class ItemIconTable : ScriptableObject
    {
        public static ItemIconTable Local;

        private void OnValidate()
        {
            Local = this;
        }

        public enum SpriteSize
        {
            Small,
            Middle,
            Big
        }
        [SerializeField] private SerializableDictionary<string,ItemIcon> itemIcon;

        public Sprite GetItemSprite(string iconName, SpriteSize size)
        {
            if (!itemIcon.TryGetValue(iconName, out ItemIcon icon))
            {
                return null;
            }

            return size switch
            {
                SpriteSize.Small => icon.sprite_64,
                SpriteSize.Middle => icon.sprite_256,
                SpriteSize.Big => icon.sprite_512,
                _ => null
            };
        }
        
#if UNITY_EDITOR
        [Button]
        public void ReloadAll()
        {
            Local = this;
            itemIcon ??= new();
            itemIcon.Clear();
            RegisterIcon(SpriteSize.Small);
            RegisterIcon(SpriteSize.Middle);
            RegisterIcon(SpriteSize.Big);
            
        }

        private void RegisterIcon(SpriteSize size)
        {
            string path = $"Image/Icon/";
            string sizePath = string.Empty;
            switch (size)
            {
                case SpriteSize.Small :
                    sizePath = "64";
                    break;
                case SpriteSize.Middle :
                    sizePath = "256";
                    break;
                case SpriteSize.Big :
                    sizePath = "512";
                    break;
            }
            
            var icons = Resources.LoadAll<Sprite>(path + sizePath);

            if (icons != null && !string.IsNullOrEmpty(sizePath))
            {
                foreach (var iconImage in icons)
                {
                    string iconName = iconImage.name.Split("_"+sizePath)[0];

                    if (itemIcon.TryGetValue(iconName, out ItemIcon icon))
                    {
                        SetIconSprite(icon, size, iconImage);

                    }
                    else
                    {
                        ItemIcon newIcon = new ItemIcon();
                        SetIconSprite(newIcon, size, iconImage);
                        itemIcon.TryAdd(iconName, newIcon);
                    }
                }
            }

        }
        
        private void SetIconSprite(ItemIcon icon, SpriteSize size, Sprite iconImage)
        {
            switch (size)
            {
                case SpriteSize.Small:
                    icon.sprite_64 = iconImage;
                    break;
                case SpriteSize.Middle:
                    icon.sprite_256 = iconImage;
                    break;
                case SpriteSize.Big:
                    icon.sprite_512 = iconImage;
                    break;
            }
        }
#endif
    }
}