using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItemSlot : MonoBehaviour
{
    [SerializeField] Image backGroud;
    [SerializeField] Image itemSprite;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemCount;

    [SerializeField] Sprite legendColor;
    [SerializeField] Sprite epicColor;
    [SerializeField] Sprite normalColor;

    public void SetItem(ItemSO item, int count)
    {
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
                backGroud.sprite = legendColor;
                break;
            case Grade.Epic:
                backGroud.sprite = epicColor;
                break;
            case Grade.Normal:
                backGroud.sprite = normalColor;
                break;
        }
    }
    public void ClearItem()
    {
        itemSprite.enabled = false;
        itemName.text = "";
        itemCount.text = "";
        backGroud.sprite = normalColor;
    }
}
