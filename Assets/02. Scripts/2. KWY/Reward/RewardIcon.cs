using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RewardIcon : MonoBehaviour
{
    [SerializeField] Image backGround;
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemText;
    [SerializeField] TextMeshProUGUI amountText;

    [SerializeField] Sprite goldIcon;

    [SerializeField] Sprite legendColor;
    [SerializeField] Sprite epicColor;
    [SerializeField] Sprite normalColor;


    public void Set(RewardData reward)
    {
        if(reward.gold > 0)
        {
            Debug.Log("재화");
            itemImage.sprite = goldIcon;
            backGround.sprite = normalColor;
            itemText.text = "Gold";
            amountText.text = $"+{reward.gold}";
            return;
        }
        ItemSO item = reward.item;

        itemImage.sprite = item.sprite;
        itemText.text = item.itemName;
        amountText.text = $"+{reward.amount}";

        switch (item.grade)
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
}
