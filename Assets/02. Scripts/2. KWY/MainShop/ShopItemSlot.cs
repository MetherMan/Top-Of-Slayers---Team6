using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class ShopItemSlot : MonoBehaviour
{
    [SerializeField] Image backGroud;
    [SerializeField] Image itemSprite;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI itemPrice;


    [SerializeField] Sprite legendColor;
    [SerializeField] Sprite epicColor;
    [SerializeField] Sprite normalColor;

    [SerializeField] ItemDetailsView detailsView;
    
    private ItemSO currentItem;

    public void SetItem(ItemSO item)
    {
        currentItem = item;

        itemSprite.sprite = item.sprite;
        itemSprite.enabled = true;
        itemName.text = item.itemName;
        itemPrice.text = item.price.ToString();

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
        itemPrice.text = "";
        backGroud.sprite = normalColor;
    }

    public void OnClickSlot()
    {
        if(currentItem == null) return;
        detailsView.Show(currentItem);
    }
}
