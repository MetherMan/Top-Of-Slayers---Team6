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
    public virtual string GetStatText(int level)
    {
        return "";
    }
    public virtual int GetAttack(int level)
    {
        return 0;
    }
    public virtual int GetHP(int level)
    {
        return 0;
    }
    public virtual int GetHeal(int level) 
    {
        return 0;
    }
    public virtual float GetCritical(int level) 
    {
        return 0;
    }
    public virtual int GetSpeed(int level)
    {
        return 0;
    }


}
