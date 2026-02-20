using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Emblem")]
public class EmblemSO : EquipmentSO
{
    public int attackHealRate;

    public override int GetHeal(int level)
    {
        return attackHealRate + level * 1;
    }

    public override string GetStatText(int level)
    {
        return $"적중 시 피 회복 +{GetHeal(level)}";
    }
}
