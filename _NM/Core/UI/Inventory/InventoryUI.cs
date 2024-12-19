using System.Collections.Generic;
using System.Linq;
using System.Text;
using _NM.Core.Character;
using _NM.Core.Input;
using _NM.Core.Item;
using _NM.Core.Manager;
using _NM.Core.Object;
using _NM.Core.UI.UICanvas;
using _NM.Core.Utils;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace _NM.Core.UI.Inventory
{
    public class InventoryUI : CanvasPage
    {
        [Title("인벤토리 정보")]
        [SerializeField] private int slotCount = 12;
        
        [SerializeField] private GameObject slotPrefab;
        [SerializeField] private GameObject contentObject;
        
        [SerializeField] private SerializableDictionary<EItemType,Button> categoryButtons;
        [SerializeField] private EItemType selectedType;
        [SerializeField] private Button currentButton;
        
        [SerializeField] private Button confirmButton;
        
        private List<InventorySlot> slots;
        private InventorySlot selectedSlot;
        private InventorySlot markedSlot;
        
        [Title("선택된 아이템 정보")]
        [field: SerializeField] public TextMeshProUGUI SelectedItemName { get; private set; }
        [field: SerializeField] public TextMeshProUGUI SelectedItemDescription { get; private set; }
        [field: SerializeField] public Image SelectedItemImage { get; private set; }
        [field: SerializeField] public Image SelectedItemBackground { get; private set; }
        [field: SerializeField] public long SelectedItemID { get; private set; }
        [field: SerializeField] public Image RegisterQuickSlotImage { get; private set; }
        [field: SerializeField] public TextMeshProUGUI StatHealthText { get; private set; }
        [field: SerializeField] public TextMeshProUGUI StatDamageText { get; private set; }
        [field: SerializeField] public TextMeshProUGUI StatStaminaText { get; private set; }
        
        private StringBuilder healthStringBuilder;
        private StringBuilder staminaStringBuilder;
        private StringBuilder damageStringBuilder;
        
        [SerializeField] private Image markedImage;
        
        private void Awake()
        {
            InitSlots();
            ClearSlot((int)selectedType);
            currentButton.Select();
            confirmButton.onClick.AddListener(() => Close().Forget());
            
            SetSelectedItemID(0);
        }
        
        private void InitSlots()
        {
            selectedType = EItemType.Goods;
            currentButton = categoryButtons[selectedType];
            slots = new();
            for (var i = 0; i < slotCount; i++)
            {
                var slot = ObjectPool.Spawn(slotPrefab, contentObject.transform).GetComponent<InventorySlot>();
                if (slot != null)
                {
                    var slotIndex = i + 1;
                    slot.name = slotIndex.ToString();
                    slot.SetSlotIndex(slotIndex);
                    slots.Add(slot);
                }
            }
            
            healthStringBuilder = new StringBuilder();
            staminaStringBuilder = new StringBuilder();
            damageStringBuilder = new StringBuilder();
            
            healthStringBuilder.Clear();
            staminaStringBuilder.Clear();
            damageStringBuilder.Clear();
        }
        
        protected override UniTask OnPreOpen()
        {
            GameManager.TimeScale = 0f;
            currentButton.Select();
            currentButton.onClick?.Invoke();
            OnInventoryOpen();
            SetSelectedItemID(0);
            return base.OnPreOpen();
        }

        protected override UniTask OnPostClose()
        {
            GameManager.TimeScale = 1f;
            return base.OnPostClose();
        }

        private void OnInventoryOpen()
        {
            ClearSelectInfo();
            UpdateMarkedSlot();
            RefreshPlayerStats();
        }

        private void UpdateMarkedSlot()
        {
            if (markedSlot)
            {
                markedSlot.SetMark(false);
            }
                
            var markedItem = InventoryManager.I.GetItem(QuickSlot.QuickSlot.I.GetRegisteredItemID());
            if (markedItem != null)
            {
                var foundSlot = slots.Find(slot => slot.GetCurrentItemID() == markedItem.ItemInfoData.ItemID);
                if (foundSlot)
                {
                    markedSlot = foundSlot;
                    markedSlot.SetMark(true);
                }
            }
        }

        private void Update()
        {
            if (InputProvider.I == null) return;
            var inputData = InputProvider.I.LatestInput;
            if (!inputData.RegisterQuickSlot) return;
            
            var item = InventoryManager.I.GetItem(SelectedItemID);
            if (item is not { ItemInfoData: { ItemType: EItemType.Food, ItemHealAmount: > 0 } }) return;
            
            QuickSlot.QuickSlot.I.RegisterItem(item);
            RegisterQuickSlotImage.enabled = false;
            if (markedSlot)
            {
                markedSlot.SetMark(false);
            }
            markedSlot = selectedSlot;
            markedSlot.SetMark(true);
            markedImage.enabled = true;
        }

        public void SetSlotInfo(int index, Item.Implementaion.Item item)
        {
            if (item != null)
            {
                slots[index].SetItem(
                    ItemIconTable.Local.GetItemSprite(item.ItemInfoData.ItemIconName, ItemIconTable.SpriteSize.Middle),
                    ItemIconTable.Local.GetItemSprite(item.ItemInfoData.ItemIconName, ItemIconTable.SpriteSize.Big));
                slots[index].SetItemAmount(item.Amount);
                slots[index].SetItemName(item.ItemInfoData.ItemName, item.ItemInfoData.ItemDescription);
                slots[index].SetItemID(item.ItemInfoData.ItemID);
            }
            else
            {
                RemoveItem(index);
            }
        }

        public void ClearSlot(int itemType)
        {
            for (int i = 0; i < slots.Count; i++)
            {
                slots[i].RemoveItem();
            }

            selectedType = (EItemType)itemType;
        
            currentButton.targetGraphic.TryGetComponent(out Image prevImage);
            prevImage.sprite = currentButton.spriteState.disabledSprite;
        
            currentButton = categoryButtons[selectedType];
            currentButton.targetGraphic.TryGetComponent(out Image currentImage);
            currentImage.sprite = currentButton.spriteState.pressedSprite;
            
            InventoryManager.I.SortInventory((EItemType)itemType);
            UpdateMarkedSlot();
        }

        public void SetSelectedItemID(long itemID)
        {
            SelectedItemID = itemID;
            var item = InventoryManager.I.GetItem(SelectedItemID);
            var registeredItemID = QuickSlot.QuickSlot.I.GetRegisteredItemID();
            
            selectedSlot = slots.FirstOrDefault(e => e.HasItem && e.GetCurrentItemID() == SelectedItemID);
            if (registeredItemID > 0 && registeredItemID == SelectedItemID)
            {
                RegisterQuickSlotImage.enabled = false;
                markedImage.enabled = true;
            }
            else
            {
                RegisterQuickSlotImage.enabled = SelectedItemID > 0 && selectedType == EItemType.Food &&
                                                 item is { ItemInfoData: { ItemHealAmount: > 0 } };
                markedImage.enabled = false;
            }
        }

        public void RemoveItem(int index)
        {
            slots[index].RemoveItem();
        }

        public void ClearSelectInfo()
        {
            SelectedItemImage.color = new Color(1, 1, 1, 0);
            SelectedItemBackground.enabled = false;
            SelectedItemName.text = string.Empty;
            SelectedItemDescription.text = string.Empty;
        }

        public void RefreshPlayerStats()
        {
            CharacterStat stat = Core.Character.Character.Local.Stat;
            healthStringBuilder.Clear();
            staminaStringBuilder.Clear();
            damageStringBuilder.Clear();
            
            healthStringBuilder.Append(stat.Health.ToString());
            damageStringBuilder.Append(stat.MultiplyDamage * 100);
            damageStringBuilder.Append("%");
            staminaStringBuilder.Append(stat.MultiplyStamina * 100);
            staminaStringBuilder.Append("%");
            
            StatHealthText.text = healthStringBuilder.ToString();
            StatDamageText.text = damageStringBuilder.ToString();
            StatStaminaText.text = staminaStringBuilder.ToString();
        }
    }
}
