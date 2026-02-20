using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    [SerializeField] private EquipSlot slotType;
    [SerializeField] private Image icon;
    [SerializeField] private Sprite defaultImage;


    private void OnEnable()
    {

            EquipmentManager.Instance.OnEquipmentChanged += Refresh;
            Refresh();
    }

    private void OnDisable()
    {

            EquipmentManager.Instance.OnEquipmentChanged -= Refresh;
    }

    public void Refresh()
    {
        InventoryItem equip = null;

        switch (slotType)
        {
            case EquipSlot.Weapon:
                equip = EquipmentManager.Instance.weapon;
                break;

            case EquipSlot.Shoes:
                equip = EquipmentManager.Instance.shoes;
                break;

            case EquipSlot.Gloves:
                equip = EquipmentManager.Instance.gloves;
                break;

            case EquipSlot.Armor:
                equip = EquipmentManager.Instance.armor;
                break;

            case EquipSlot.Emblem:
                equip = EquipmentManager.Instance.emblem;
                break;
        }

        if (equip != null && equip.item != null)
        {
            icon.sprite = equip.item.sprite;
        }
        else
        {
            icon.sprite = defaultImage;
        }
    }
    public void OnClickUnequip()
    {
        EquipmentManager.Instance.Unequip(slotType);
    }

}
