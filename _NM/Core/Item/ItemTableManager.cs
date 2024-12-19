using System.Collections.Generic;
using _NM.Core.Manager;
using _NM.Core.UI.Inventory;
using _NM.Core.Utils;
using SDUnityExtension.Scripts.Pattern;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _NM.Core.Item
{
    public class ItemTableManager : SDSingleton<ItemTableManager>
    {
        public bool Initialized { get; private set; }
        
        private static readonly Dictionary<long, ItemInfoContainer> Items = new();
        [SerializeField] private SerializableDictionary<long, Dictionary<int,List<ItemAcquireInfo>>> dropTable;
        [SerializeField] private ItemIconTable itemIconTable;

        [SerializeField] private bool usingCache;
        
        private void Awake()
        {
            ItemIconTable.Local = itemIconTable;
            DataManager.onInitialized += ApplyDropTable;
            dropTable = new();
        }

        public static bool ContainItemID(long itemID)
        {
            return Items.ContainsKey(itemID);
        }

        public static ItemInfoContainer? GetItemInfo(long itemID)
        {
            if (Items != null && Items.TryGetValue(itemID, out ItemInfoContainer itemInfo))
            {
                return itemInfo;
            }

            return null;
        }

        private void ApplyDropTable()
        {
            foreach (var setting in DataManager.GetSheetData(GoogleSheetsConstantData.ESheetPage.ItemSettings))
            {
                ItemInfoContainer item = new();
                item.ApplyFromSheetData(setting);
                Items.TryAdd(item.ItemID, item);
            }
            
            foreach (var dropData in DataManager.GetSheetData(GoogleSheetsConstantData.ESheetPage.MonsterDropTable))
            {
                ItemAcquireInfo acquireInfo = new ItemAcquireInfo();
                acquireInfo.Apply(dropData);
                if (!dropTable.ContainsKey(acquireInfo.MonsterID))
                {
                    dropTable.TryAdd(acquireInfo.MonsterID, new Dictionary<int, List<ItemAcquireInfo>>());
                }

                if (!dropTable[acquireInfo.MonsterID].ContainsKey(acquireInfo.ItemGroup))
                {
                    dropTable[acquireInfo.MonsterID].TryAdd(acquireInfo.ItemGroup, new List<ItemAcquireInfo>());
                }
                
                dropTable[acquireInfo.MonsterID][acquireInfo.ItemGroup].Add(acquireInfo);
            }

            DataManager.onInitialized -= ApplyDropTable;
            Initialized = true;
        }

        private void OnDestroy()
        {
            foreach (var table in dropTable)
            {
                table.Value.Clear();
            }
            dropTable.Clear();
        }

        public void Drop(long enemyID)
        {
            if (dropTable.TryGetValue(enemyID, out var dropInfo))
            {
                foreach (var dropList in dropInfo)
                {
                    if (dropList.Key == 0)
                    {
                        foreach (var info in dropList.Value)
                        {
                            if (info.ItemChance is > 0 and < 100 && Random.value * 100 > info.ItemChance)
                            {
                                continue;
                            }
                            int dropCount = Random.Range(info.MinCount, info.MaxCount + 1);
                            InventoryManager.I.AddItem(info.ItemID,dropCount);
                        }
                        
                        
                    }
                    else
                    {
                        var probs = new float[dropList.Value.Count + 1];
                        float total = 0f;
                        int i;
                        for (i = 0; i < dropList.Value.Count; i++)
                        {
                            if (dropList.Key > 0)
                            {
                                probs[i] = dropList.Value[i].ItemChance;
                                total += probs[i];
                            }
                        }
                        probs[i] = 100 - total;
                        if (probs[i] < 0)
                        {
                            probs[i] = 0f;
                        }
                        int num = RandomGenerator.Choose(probs);
                        
                        if (num == dropList.Value.Count)
                        {
                            break;
                        }
                        long itemID = dropList.Value[num].ItemID;
                        int dropCount = Random.Range(dropList.Value[num].MinCount, dropList.Value[num].MinCount + 1);
                        
                        InventoryManager.I.AddItem(itemID,dropCount);
                    }
                }
            }
        }
    }
}
