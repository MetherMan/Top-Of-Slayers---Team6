using UnityEngine;

public enum EquipSlot
{
    Weapon,
    Armor,
    Emblem,
    Gloves,
    Shoes

}
public class EquipmentSO : ItemSO
{
    public EquipSlot equipSlot;
    public virtual string GetStatText()
    {
        return "";
    }
}
