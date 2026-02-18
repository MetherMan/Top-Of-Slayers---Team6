using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Gloves")]
public class GlovesSO : EquipmentSO
{
    public float criticalRate;

    public override string GetStatText()
    {
        return $"치명타 확률 +{criticalRate}";
    }
}
