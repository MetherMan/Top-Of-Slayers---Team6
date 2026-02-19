using System;
using UnityEngine;

public class EquipmentManager : Singleton<EquipmentManager>
{
    public EquipmentSO weapon;
    public EquipmentSO shoes;
    public EquipmentSO gloves;
    public EquipmentSO armor;
    public EquipmentSO emblem;

    public event Action OnEquipmentChanged;

    public void Equip(EquipmentSO equipment)
    {
        switch (equipment.equipSlot)
        {
            case EquipSlot.Weapon:
                weapon = equipment;
                break;
            case EquipSlot.Shoes:
                shoes = equipment;
                break;
            case EquipSlot.Gloves:
                gloves = equipment;
                break;
            case EquipSlot.Armor:
                armor = equipment;
                break;
            case EquipSlot.Emblem:
                emblem = equipment;
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

    public bool IsEquipped(EquipmentSO equipment)
    {
        return weapon == equipment ||
            armor == equipment ||
            shoes == equipment ||
            gloves == equipment ||
            emblem == equipment;
    }

}
