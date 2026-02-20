using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Weapon")]
public class WeaponSO : EquipmentSO
{
    public int attack;

    public override int GetAttack(int level)
    {
        return attack + level * 5;
    }

    public override string GetStatText(int level)
    {
        return $"공격력 +{GetAttack(level)}";
    }
}
