using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Rendering;

namespace _NM.Core.Item
{
    [Serializable]
    public class ItemAcquireInfo
    {
        [field: LabelText("드랍하는 몬스터 ID"),SerializeField] public long MonsterID { get; private set; }
        [field: LabelText("아이템 ID"),SerializeField] public long ItemID { get; private set; }

        [field: LabelText("아이템 획득 확률"),SerializeField] public float ItemChance { get; private set; }

        [field: LabelText("아이템 그룹"),SerializeField] public int ItemGroup { get; private set; }

        [field:LabelText("최소 드랍 개수"),SerializeField] public int MinCount { get; private set; }
        [field:LabelText("최대 드랍 개수"),SerializeField] public int MaxCount { get; private set; }

        public void Apply(Dictionary<string, string> data)
        {
            MonsterID = long.TryParse(data["MonsterEntry"], out long monsterEntry) ? monsterEntry : 0;
            ItemID = long.TryParse(data["ItemID"], out long itemID) ? itemID : 0;
            ItemChance = float.TryParse(data["Chance"], out float itemChance) ? itemChance : 0;
            ItemGroup = int.TryParse(data["GroupID"], out int itemGroup) ? itemGroup : 0;
            MinCount = int.TryParse(data["MinCount"], out int minCount) ? minCount : 0;
            MaxCount = int.TryParse(data["MaxCount"], out int maxCount) ? maxCount : 0;
        }
    }
    [Serializable]
    public enum EItemType
    {
        Goods,
        Food,
        Other
    }
    
    [CreateAssetMenu(fileName = "Item Table", menuName = "Scriptable Object/Item Table")]
    public class ItemTable : ScriptableObject
    {
        private static ItemTable instance;
        public static ItemTable Instance
        {
            get
            {
                if (instance == null)
                {
                    ItemTable asset = Resources.Load<ItemTable>("Data/Item/Item Table");
                    if (asset == null)
                    {
                        throw new System.Exception("Item Info 인스턴스를 찾을 수 없습니다.");
                    }

                    instance = asset;
                }

                return instance;
            }
        }
        public SerializedDictionary<long, ItemInfoContainer> Items;
    }

    [Serializable]
    public struct ItemInfoContainer
    {
        [field: LabelText("아이템 ID"),SerializeField] public long ItemID { get; private set; }
        [field: LabelText("아이템 종류"),SerializeField] public EItemType ItemType { get; private set; }
        [field: LabelText("아이템 이름"),SerializeField] public string ItemName { get; private set; }
        [field: LabelText("아이템 설명"), SerializeField,TextArea] public string ItemDescription { get; private set; }
        [field: LabelText("아이템 중첩 가능 개수"), SerializeField] public int ItemMaxCount { get; private set; }
        [field: LabelText("아이콘명 (영어)"), SerializeField] public string ItemIconName { get; private set; }
        
        [field: LabelText("아이템 사용 시 회복량"),SerializeField] public int ItemHealAmount { get; private set; }
        [field: LabelText("아이템 제출 시 추가 체력"),SerializeField] public int RewardHealAmount { get; private set; }
        [field: LabelText("아이템 제출 시 추가 데미지"),SerializeField] public int RewardDamageAmount { get; private set; }
        [field: LabelText("아이템 제출 시 추가 스태미나 회복량"),SerializeField] public int RewardStaminaAmount { get; private set; }
        [field: LabelText("아이템 사용 딜레이 (초)"), SerializeField] public float ItemDelay { get; private set; }

        public void ApplyFromSheetData(Dictionary<string,string> data)
        {
            ItemID = long.Parse(data["item_ID"]);
            ItemType = (EItemType)Enum.Parse(typeof(EItemType), data["item_class"]);
            ItemName = data["item_name"];
            ItemDescription = data["item_Text"];
            ItemMaxCount = int.Parse(data["item_stack"]);
            ItemIconName = data["Item_Icon"];
            ItemHealAmount = int.TryParse(data["item_heal_amount"], out var heal) ? heal : 0;
            RewardHealAmount = int.TryParse(data["Reward_Health_Stat"], out var health) ? health : 0;
            RewardDamageAmount = int.TryParse(data["Reward_Damage_Stat"], out var damage) ? damage : 0;
            RewardStaminaAmount = int.TryParse(data["Reward_Stamina_Stat"], out var stamina) ? stamina : 0;
            ItemDelay = float.TryParse(data["item_wait_time"], out var delay) ? delay : 0f;
        }
    }
}