using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemDetailsView : MonoBehaviour
{
    [SerializeField] Image backGround;
    [SerializeField] Sprite legendColor;
    [SerializeField] Sprite epicColor;
    [SerializeField] Sprite normalColor;

    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemDesc;
    [SerializeField] TextMeshProUGUI itemPrice;

    ItemSO currentItem;

    public void Show(ItemSO item)
    {
        currentItem = item;

        itemImage.sprite = item.sprite;
        itemName.text = item.itemName;
        itemDesc.text = item.description;
        itemPrice.text = item.price.ToString();

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
    public void OnClick()
    {
        if (currentItem == null) return;

        InventoryManager.Instance.AddItem(currentItem, 1);
    }
}
