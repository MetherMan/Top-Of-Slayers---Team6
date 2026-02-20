using System;
using UnityEngine;

public class EquipmentManager : Singleton<EquipmentManager>
{
    public InventoryItem weapon;
    public InventoryItem shoes;
    public InventoryItem gloves;
    public InventoryItem armor;
    public InventoryItem emblem;

    public event Action OnEquipmentChanged;

    public void Equip(InventoryItem data)
    {
        if (!(data.item is EquipmentSO equip)) return;

        switch (equip.equipSlot)
        {
            case EquipSlot.Weapon:
                weapon = data;
                break;
            case EquipSlot.Shoes:
                shoes = data;
                break;
            case EquipSlot.Gloves:
                gloves = data;
                break;
            case EquipSlot.Armor:
                armor = data;
                break;
            case EquipSlot.Emblem:
                emblem = data;
                break;
        }

        OnEquipmentChanged?.Invoke();
    }

    public void Unequip(EquipSlot slot)
    {
        switch (slot)
        {
            case EquipSlot.Weapon:
                weapon = null;
                break;

            case EquipSlot.Shoes:
                shoes = null;
                break;

            case EquipSlot.Gloves:
                gloves = null;
                break;

            case EquipSlot.Armor:
                armor = null;
                break;

            case EquipSlot.Emblem:
                emblem = null;
                break;
        }

        OnEquipmentChanged?.Invoke();
    }

    public bool IsEquipped(InventoryItem data)
    {
        return weapon == data ||
            armor == data ||
            shoes == data ||
            gloves == data ||
            emblem == data;
    }

}
