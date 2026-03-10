using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Ring")]
public class RingSO : EquipmentSO
{
    public int attackHealRate;

    public override int GetHeal(int level)
    {
        return attackHealRate + level * 1;
    }

    public override string GetStatText(int level)
    {
        return $"적중시 Hp회복 +{GetHeal(level)}";
    }
}
