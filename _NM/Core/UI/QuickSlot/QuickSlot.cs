using System;
using _NM.Core.Item;
using _NM.Core.Sound;
using _NM.Core.UI.Inventory;
using SDUnityExtension.Scripts.Pattern;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace _NM.Core.UI.QuickSlot
{
    public class QuickSlot : SDSingleton<QuickSlot>
    {
        private bool Usable { get; set; } = true;
        
        [SerializeField] private Item.Implementaion.Item registeredItem;
        [SerializeField] private Image gaugeImage;
        [SerializeField] private float cooldown;
        [SerializeField] private float cooldownTotal;
        [SerializeField] private TextMeshProUGUI itemCountText;
        [SerializeField] private Image itemImage;
        [SerializeField] private PlaySound itemRegisterSound;
        [SerializeField] private PlaySound itemUseSound;
        
        private bool CanUse => Usable && cooldownTotal <= 0;

        private void Start()
        {
            InventoryManager.I.onItemChanged.AddListener(UpdateQuickSlotInfo);
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }
        
        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Usable = scene.name switch
            {
                "Stage1" or "Stage2" or "Stage3" or "BossRoom" => true,
                _ => false
            };
        }

        private void Update()
        {
            cooldownTotal -= Time.deltaTime;
            cooldownTotal = Mathf.Clamp(cooldownTotal, 0, cooldown);
            gaugeImage.fillAmount = cooldownTotal / cooldown;
        }

        private void UpdateQuickSlotInfo()
        {
            var itemCount = InventoryManager.I.GetAllAmount(registeredItem);
            if (itemCount == 0)
            {
                registeredItem = null;
                itemImage.sprite = null;
                itemImage.enabled = false;
                itemCountText.text = String.Empty;
            }
            else
            {
                itemCountText.text = InventoryManager.I.GetAllAmount(registeredItem).ToString();
            }
        }

        public void Use()
        {
            if (!CanUse || InventoryManager.I.GetAllAmount(registeredItem) <= 0) return;
            
            InventoryManager.I.UseItem(registeredItem);
            if (itemUseSound)
                itemUseSound.Play();
            if (registeredItem is { Amount: 0 })
            {
                Item.Implementaion.Item item = InventoryManager.I.GetItem(registeredItem.ItemInfoData.ItemID);
                if (item != null)
                {
                    registeredItem = item;
                    return;
                }
                itemImage.sprite = null;
                itemImage.enabled = false;
                itemCountText.text = String.Empty;
            }
            else
            {
                itemCountText.text = InventoryManager.I.GetAllAmount(registeredItem).ToString();
            }
            cooldownTotal = cooldown;
        }

        public void RegisterItem(Item.Implementaion.Item item)
        {
            if (item == null) return;
            
            registeredItem = item;
            itemImage.enabled = true;
            itemImage.sprite = ItemIconTable.Local.GetItemSprite(registeredItem.ItemInfoData.ItemIconName, ItemIconTable.SpriteSize.Small);
            itemCountText.text = InventoryManager.I.GetAllAmount(registeredItem).ToString();
            if (itemRegisterSound)
                itemRegisterSound.Play();
        }
        
        public void RegisterItem(long itemID)
        {
            Item.Implementaion.Item item = InventoryManager.I.GetItem(itemID);
            RegisterItem(item);
        }

        public long GetRegisteredItemID()
        {
            return registeredItem?.ItemInfoData.ItemID ?? 0;
        }
    }
}
