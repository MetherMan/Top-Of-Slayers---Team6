using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Emblem")]
public class EmblemSO : EquipmentSO
{
    public float attackHealRate;

    public override string GetStatText()
    {
        return $"적중 시 피 회복율 +{attackHealRate}";
    }
}
