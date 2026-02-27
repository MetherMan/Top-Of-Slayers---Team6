using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour
{
    [SerializeField] Image backGround;
    [SerializeField] Image itemSprite;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemCount;
    [SerializeField] TextMeshProUGUI itemLevel;
    [SerializeField] Image equipMark;

    [SerializeField] Sprite legendColor;
    [SerializeField] Sprite epicColor;
    [SerializeField] Sprite normalColor;

    [SerializeField] InventorySelection inventorySelection;

    InventoryItem currentItem;

    public void SetItem(InventoryItem data)
    {
        currentItem = data;

        itemSprite.sprite = data.item.sprite;
        itemSprite.enabled = true;

        itemName.text = data.item.itemName;

        if(data.item is EquipmentSO)
        {
            itemCount.text = "";
        }
        else
        {
            itemCount.text = data.count.ToString();

        }

        if (data.item is EquipmentSO && data.enhancementLevel > 0)
        {
            itemLevel.text = $"+{data.enhancementLevel}";
        }
        else
        {
            itemLevel.text = "";
        }

        ApplyGradeColor(data.item.grade);

        bool isEquipped = EquipmentManager.Instance.IsEquipped(data);
        equipMark.gameObject.SetActive(isEquipped);
    }


    public void ApplyGradeColor(Grade grade)
    {
        switch (grade)
        {
            case Grade.Legend:
                backGround.sprite = legendColor;
                break;
            case Grade.Epic:
                backGround.sprite = epicColor;
                break;
            case Grade.Normal:
                backGround.sprite = normalColor;
                break;
        }
    }
    public void ClearItem()
    {
        currentItem = null;

        itemSprite.enabled = false;
        itemName.text = "";
        itemCount.text = "";
        itemLevel.text = "";
        backGround.sprite = normalColor;
        equipMark.gameObject.SetActive (false);
    }

    public void OnClickSlot()
    {
        if (currentItem == null) return;

        inventorySelection.NotifyItemClicked(currentItem);
    }

    public void SetSelection(InventorySelection selection)
    {
        inventorySelection = selection;
    }
}
