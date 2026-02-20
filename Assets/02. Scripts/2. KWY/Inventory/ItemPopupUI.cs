using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupUI : MonoBehaviour
{
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemName;

    [SerializeField] GameObject equipButton;
    [SerializeField] GameObject okButton;

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
            okButton.SetActive(true);
        }
        else
        {
            equipButton.SetActive(false);
            okButton.SetActive(true);
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

    public void OnClickCancel()
    {
        gameObject.SetActive(false);
    }
}
