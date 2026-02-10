using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Emblem")]
public class EmblemSO : EquipmentSO
{
    public int attack;

    public override string GetStatText()
    {
        return $"공격력 +{attack}";
    }
}
