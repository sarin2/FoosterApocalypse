using System;
using System.Collections.Generic;
using System.Linq;
using _NM.Core.Data.Savable;
using _NM.Core.Item;
using _NM.Core.Manager;
using _NM.Core.Quest;
using Newtonsoft.Json;
using SDUnityExtension.Scripts.Pattern;
using UnityEngine;
using UnityEngine.Events;

namespace _NM.Core.UI.Inventory
{
    public class InventoryManager : SDSingleton<InventoryManager>, ISavable
    {
        public UnityEvent<Item.Implementaion.Item> onAcquireItem;
        public UnityEvent onItemChanged;
        
        [SerializeField] private int slotCount = 12;
        [SerializeField] private InventoryUI inventoryUI;
        
        private Dictionary<EItemType, List<Item.Implementaion.Item>> itemDictionary;

        protected override void Awake()
        {
            SetInstance(this);
            Initialize();
            inventoryUI = FindAnyObjectByType<InventoryUI>(FindObjectsInactive.Include);
            DataManager.onPostInitialized += OnDataInitialized;
        }

        private void OnDataInitialized()
        {
            DataManager.onPostInitialized -= OnDataInitialized;
            Load();
        }

        private void Initialize()
        {
            itemDictionary = new();

            itemDictionary.TryAdd(EItemType.Goods, new List<Item.Implementaion.Item>(slotCount));
            itemDictionary.TryAdd(EItemType.Food, new List<Item.Implementaion.Item>(slotCount));
            itemDictionary.TryAdd(EItemType.Other, new List<Item.Implementaion.Item>(slotCount));

            for (int i = 0; i < slotCount; i++)
            {
                itemDictionary[EItemType.Goods].Add(null);
                itemDictionary[EItemType.Food].Add(null);
                itemDictionary[EItemType.Other].Add(null);
            }
        }

        public void OnAcquireItem(Item.Implementaion.Item item)
        {
            QuestManager.I.OnItemAcquired(item.ItemInfoData.ItemID, item.Amount);

            EItemType itemType = item.ItemInfoData.ItemType;
            List<Item.Implementaion.Item> itemTypeList = itemDictionary[itemType];
            
            for (int i = 0; i < itemTypeList.Count; i++)
            {
                if (itemTypeList[i] != null && itemTypeList[i].ItemInfoData.ItemID == item.ItemInfoData.ItemID)
                {
                    itemTypeList[i].AddAmountAndGetExcess(item.Amount);
                    inventoryUI.SetSlotInfo(i, itemTypeList[i]);
                    onAcquireItem?.Invoke(item);
                    return;
                }
            }
            
            for (int i = 0; i < itemTypeList.Count; i++)
            {
                if (itemTypeList[i] == null)
                {
                    itemTypeList[i] = item.Clone() as Item.Implementaion.Item;
                    itemTypeList[i].SetSlot(i);
                    itemTypeList[i].SetAmount(item.Amount);
                    inventoryUI.SetSlotInfo(i, itemTypeList[i]);
                    onAcquireItem?.Invoke(item);
                    return;
                }
            }
        }

        public void AddItem(long itemID, int amount)
        {
            Item.Implementaion.Item item = CreateItem(itemID, amount);
            if (item != null)
            {
                OnAcquireItem(item);
                SortInventory(item.ItemInfoData.ItemType);
            }
            else
            {
                Debug.LogError("아이템 추가 실패");
            }
            onItemChanged.Invoke();
        }
    
        public Item.Implementaion.Item GetItem(long itemID)
        {
            foreach (EItemType itemType in Enum.GetValues(typeof(EItemType)))
            {
                if (!itemDictionary.TryGetValue(itemType, out var itemList)) continue;
                var result = itemList.Find(item => item != null && item.ItemInfoData.ItemID.Equals(itemID) && item.Amount != 0);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }

        public Item.Implementaion.Item CreateItem(long itemID, int amount)
        {
            Item.Implementaion.Item result = new Item.Implementaion.Item(itemID, amount);
            return result;
        }
    
        public void DisAmountItem(long itemID, int disAmount)
        {
            Item.Implementaion.Item item = GetItem(itemID);
            DisAmountItem(item, disAmount);
        }
        
        public void DisAmountItem(Item.Implementaion.Item item, int disAmount)
        {
            if (item == null) return;
            item.SetAmount(item.Amount - disAmount);
            onItemChanged.Invoke();
            SortInventory(item.ItemInfoData.ItemType);
        }

        public int GetAllAmount(Item.Implementaion.Item item)
        {
            if (item == null) return 0;
            int itemAmountTotal = 0;
            for (int i = 0; i < itemDictionary[item.ItemInfoData.ItemType].Count; i++)
            {
                if (itemDictionary[item.ItemInfoData.ItemType][i] != null)
                {
                    if (item.ItemInfoData.ItemID == itemDictionary[item.ItemInfoData.ItemType][i].ItemInfoData.ItemID)
                    {
                        itemAmountTotal += itemDictionary[item.ItemInfoData.ItemType][i].Amount;
                    }
                }
            }
            return itemAmountTotal;
        }

        public void SetSlotInfo(EItemType itemType, int index)
        {
            Item.Implementaion.Item found = itemDictionary[itemType][index];

            if (found is { IsEmpty: true })
            {
                itemDictionary[itemType][index] = null;
                inventoryUI.RemoveItem(index);
                return;
            }
            inventoryUI.SetSlotInfo(index, itemDictionary[itemType][index]);
            onItemChanged.Invoke();
        }

        public void SortInventory(EItemType itemType)
        {
            if (!itemDictionary.ContainsKey(itemType))
            {
                return;
            }
        
            int i = 0;
            while (itemDictionary[itemType][i] != null)
            {
                i++;
            }
            int j = i;
        
            while (true)
            {
                while (++j < slotCount && itemDictionary[itemType][j] == null) ;
                if (j == slotCount)
                    break;
            
                SetSlotInfo(itemType,i);
                itemDictionary[itemType][i] = itemDictionary[itemType][j];
                itemDictionary[itemType][j] = null;
                SetSlotInfo(itemType,j);
            
                i++;
            }

            for (int k = 0; k < itemDictionary[itemType].Count; k++)
            {
                if (itemDictionary[itemType][k] != null)
                {
                    SetSlotInfo(itemType,k);
                }
            }
        }

        public void UseItem(long itemID)
        {
            Item.Implementaion.Item item = GetItem(itemID);
            UseItem(item);
        }
    
        public void UseItem(Item.Implementaion.Item item)
        {
            if (item == null) return;
            
            if (item.Amount > 0)
            {
                item.Use();
                var healAmount = Core.Character.Character.Local.Health.MaxHp * item.ItemInfoData.ItemHealAmount * 0.01f;
                var healAmountInt = Mathf.FloorToInt(healAmount);
                Core.Character.Character.Local.Health.CurrentHp += healAmountInt;
                SetSlotInfo(item.ItemInfoData.ItemType,item.Slot);
                SortInventory(item.ItemInfoData.ItemType);
            }
        
            onItemChanged.Invoke();
        }

        public void Load()
        {
            var json = PlayerPrefs.GetString(nameof(InventoryManager));
            Debug.Log($"{nameof(InventoryManager)} Load: {json}");
            if (string.IsNullOrEmpty(json))
            {
                return;
            }

            Initialize();
            var tempItems = JsonConvert.DeserializeObject<List<string>>(json);
            foreach (var itemInfoString in tempItems)
            {
                var split = itemInfoString.Split(',');
                AddItem(long.Parse(split[0]), int.Parse(split[1]));
            }
        }

        public void Save()
        {
            var items = itemDictionary.Values
                .SelectMany(list => list)
                .Where(item => item != null)
                .Select(item => $"{item.ItemInfoData.ItemID},{item.Amount}").ToList();
            PlayerPrefs.SetString(nameof(InventoryManager), JsonConvert.SerializeObject(items));
        }
    }
}
