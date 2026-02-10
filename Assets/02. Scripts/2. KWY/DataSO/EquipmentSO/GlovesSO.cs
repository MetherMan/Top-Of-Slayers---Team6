using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Gloves")]
public class GlovesSO : EquipmentSO
{
    public float attackSpeed;

    public override string GetStatText()
    {
        return $"공격속도 +{attackSpeed}";
    }
}
