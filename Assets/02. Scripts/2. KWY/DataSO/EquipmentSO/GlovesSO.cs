using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Gloves")]
public class GlovesSO : EquipmentSO
{
    public float criticalRate;

    public override float GetCritical(int level)
    {
        return criticalRate * (level * 5);
    }
    public override string GetStatText(int level)
    {
        return $"치명타 확률 +{GetCritical(level)}";
    }
}
