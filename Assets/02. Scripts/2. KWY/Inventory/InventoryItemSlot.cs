using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour
{
    [SerializeField] Image backGround;
    [SerializeField] Image itemSprite;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemCount;

    [SerializeField] Sprite legendColor;
    [SerializeField] Sprite epicColor;
    [SerializeField] Sprite normalColor;

    ItemSO currentItem;

    public void SetItem(ItemSO item, int count)
    {
        currentItem = item;

        itemSprite.sprite = item.sprite;
        itemSprite.enabled = true;
        itemName.text = item.itemName;

        itemCount.text = count.ToString();

        ApplyGradeColor(item.grade);
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
        backGround.sprite = normalColor;
    }

    public void OnClickSlot()
    {
        if (currentItem == null) return;

        InventorySelection.Instance?.NotifyItemClicked(currentItem);
    }
}
