using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemPopupUI : MonoBehaviour
{
    [SerializeField] Image itemImage;
    [SerializeField] TextMeshProUGUI itemName;

    [SerializeField] GameObject equipButton;
    [SerializeField] GameObject okButton;

    ItemSO currentItem;

    public void Show(ItemSO item)
    {
        currentItem = item;

        gameObject.SetActive(true);

        itemImage.sprite = item.sprite;
        itemName.text = item.itemName;

        if(item is EquipmentSO)
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
        if (currentItem is EquipmentSO equip)
        {
            EquipmentManager.Instance.Equip(equip);
        }

        gameObject.SetActive(false);
    }

    public void OnClickCancel()
    {
        gameObject.SetActive(false);
    }
}
