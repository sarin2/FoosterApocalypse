using System;
using _NM.Core.UI.Inventory;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour
{
    [field: LabelText("아이템 ID"), SerializeField] private long itemID;
    [field: LabelText("아이템 이미지"), SerializeField] private Image itemImage;
    [field: LabelText("아이템 이미지(512px)"), SerializeField] private Sprite itemImage_512;
    [field: LabelText("아이템 개수"), SerializeField] private TextMeshProUGUI amountText;
    [field: LabelText("아이템 이름"), SerializeField] private string nameText;
    [field: LabelText("아이템 버튼"), SerializeField] private Button itemButton;
    [field: LabelText("아이템 설명"), SerializeField] private string itemDescription;

    public int Index { get; private set; }
    public bool HasItem => itemImage.sprite != null;

    [SerializeField] public InventoryUI inventoryUI;
    [SerializeField] private GameObject iconObject;
    [SerializeField] private Image markImage;

    private void ShowIcon() => iconObject.SetActive(true);

    private void HideIcon() => iconObject.SetActive(false);

    public void SetSlotIndex(int index) => Index = index;

    public void SetItem(Sprite itemSprite256,Sprite itemSprite512)
    {
        if (itemSprite256 != null)
        {
            itemImage.sprite = itemSprite256;
            itemImage_512 = itemSprite512;
            ShowIcon();
        }
        else
        {
            RemoveItem();
        }
    }
    public void RemoveItem()
    {
        itemImage.sprite = null;
        itemImage_512 = null;
        SetItemAmount(0);
        SetItemName(string.Empty,string.Empty);
        SetItemID(0);
        HideIcon();
    }

    public void SetItemAmount(int amount)
    {
        amountText.text = amount.ToString();
    }

    public void SetItemName(string itemName, string description)
    {
        nameText = itemName;
        itemDescription = description;
    }

    public void SetItemID(long itemID)
    {
        this.itemID = itemID;
    }

    public long GetCurrentItemID()
    {
        return itemID;
    }

    private void Awake()
    {
        itemButton.onClick.AddListener(OnClickSlot);
        inventoryUI = FindAnyObjectByType<InventoryUI>();
    }

    public void OnClickSlot()
    {
        inventoryUI.SelectedItemName.text = nameText;
        inventoryUI.SelectedItemDescription.text = itemDescription;
        inventoryUI.SetSelectedItemID(itemID);
        if (itemImage_512 != null)
        {
            inventoryUI.SelectedItemImage.color = new Color(1, 1, 1, 1);
            inventoryUI.SelectedItemImage.sprite = itemImage_512;
            inventoryUI.SelectedItemBackground.enabled = true;
        }
        else
        {
            inventoryUI.ClearSelectInfo();
        }
    }

    public void SetMark(bool state)
    {
        markImage.enabled = state;
    }
    
}
