using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GachaResultIcon : MonoBehaviour
{
    [SerializeField] Image background;
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemText;

    [Header("Grade Backgrounds")]
    [SerializeField] Sprite legendColor;
    [SerializeField] Sprite epicColor;
    [SerializeField] Sprite normalColor;

    public void Set(ItemSO item)
    {
        itemImage.sprite = item.sprite;
        itemText.text = item.itemName;

        switch (item.grade)
        {
            case Grade.Legend:
                background.sprite = legendColor;
                break;

            case Grade.Epic:
                background.sprite = epicColor;
                break;

            case Grade.Normal:
                background.sprite = normalColor;
                break;
        }
    }
}