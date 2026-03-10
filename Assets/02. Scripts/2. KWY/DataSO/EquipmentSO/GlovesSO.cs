using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Gloves")]
public class GlovesSO : EquipmentSO
{
    public float criticalRate;

    public override float GetCritical(int level)
    {
        return criticalRate * (1 + level * 5);
    }
    public override string GetStatText(int level)
    {
        return $"치명타률 +{GetCritical(level):F1}%";
    }
}
