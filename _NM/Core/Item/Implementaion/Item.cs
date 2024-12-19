using System;
using Newtonsoft.Json;
using UnityEngine;

namespace _NM.Core.Item.Implementaion
{
    [Serializable]
    public class Item : ICloneable
    {
        public ItemInfoContainer ItemInfoData { get; private set; }
        public int Slot { get; protected set; }
        public int Amount { get; protected set; }
        public bool IsEmpty => Amount <= 0;
        
        public Item(ItemInfoContainer data) => ItemInfoData = data;

        public Item()
        {
            SetAmount(1);
        }
        public Item(ItemInfoContainer itemData, int amount)
        {
            ItemInfoData = itemData;
            SetAmount(amount);
        }
        
        public Item(long itemID, int amount)
        {
            var item = ItemTableManager.GetItemInfo(itemID);
            if (item.HasValue)
            {
                ItemInfoData = item.Value;
            }
            SetAmount(amount);
        }
        
        public void SetAmount(int amount)
        {
            Amount = Mathf.Clamp(amount, 0, 99);
        }

        public bool Use(int amount)
        {
            if (Amount - amount < 0)
            {
                return false;
            }
            
            Amount -= amount;
            return true;
        }
        
        public bool Use()
        {
            Amount--;

            return true;
        }
        
        public int AddAmountAndGetExcess(int amount)
        {
            int nextAmount = Amount + amount;
            SetAmount(nextAmount);

            return nextAmount;
        }

        public void SetSlot(int slot)
        {
            Slot = slot;
        }

        public object Clone()
        {
            Item other = (Item) MemberwiseClone();
            other.SetAmount(Amount);
            other.Slot = 0;
            return other;
        }
        
    }

    
}