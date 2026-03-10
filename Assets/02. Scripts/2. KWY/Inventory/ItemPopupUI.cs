using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupUI : MonoBehaviour
{
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemName;
    [SerializeField] TextMeshProUGUI failText;
    [SerializeField] TextMeshProUGUI costText;

    [SerializeField] GameObject cost;
    [SerializeField] GameObject failPanel;
    [SerializeField] GameObject equipButton;
    [SerializeField] GameObject okButton;
    [SerializeField] GameObject sellButton;


    InventoryItem currentItem;

    public void Show(InventoryItem data)
    {
        currentItem = data;

        gameObject.SetActive(true);

        itemImage.sprite = data.item.sprite;
        itemName.text = data.item.itemName;

        if(data.item is EquipmentSO)
        {
            equipButton.SetActive(true);
            sellButton.SetActive(true);
            okButton.SetActive(false);
            cost.SetActive(true);
            costText.text = (data.item.price/2).ToString();
        }
        else
        {
            equipButton.SetActive(false);
            sellButton.SetActive(false);
            okButton.SetActive(true);
            cost.SetActive(false);
        }
    }

    public void OnClickEquip()
    {
        if (currentItem.item is EquipmentSO)
        {
            EquipmentManager.Instance.Equip(currentItem);
        }

        gameObject.SetActive(false);
    }

    public void OnClickOk()
    {
        gameObject.SetActive(false);
    }

    public void OnClikcSell()
    {
        if (currentItem == null) return;

        if (EquipmentManager.Instance.IsEquipped(currentItem))
        {
            failText.text = "장착 중인 장비는\r\n 판매할 수 없습니다.";
            failPanel.SetActive(true);
            return;
        }

        if (currentItem.item is EquipmentSO)
        {
            InventoryManager.Instance.SellItem(currentItem);
        }
        gameObject.SetActive(false);
    }

}
