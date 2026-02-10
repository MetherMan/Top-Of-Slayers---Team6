using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Weapon")]
public class WeaponSO : EquipmentSO
{
    public int attack;

    public override string GetStatText()
    {
        return $"공격력 +{attack}";
    }
}
