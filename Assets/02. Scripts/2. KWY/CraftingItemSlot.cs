using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingItemSlot : MonoBehaviour
{
    [SerializeField] Image backGround;
    [SerializeField] Image itemSprite;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemType;
    [SerializeField] TextMeshProUGUI itemDetails;

    [SerializeField] Sprite legendColor;
    [SerializeField] Sprite epicColor;
    [SerializeField] Sprite normalColor;

    public void SetItem(ItemSO item)
    {
        itemSprite.sprite = item.sprite;
        itemName.text = item.itemName;

        ApplyGradeColor(item.grade);

        if(item is EquipmentSO equip)
        {
            itemType.text = equip.equipSlot.ToString();
            itemDetails.text = equip.GetStatText();
        }
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
        itemName.text = "";
        backGround.sprite = normalColor;
    }
}
