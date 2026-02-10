using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Armor")]
public class ArmorSO : EquipmentSO
{
    public int defense;

    public override string GetStatText()
    {
        return $"방어력 +{defense}";
    }
}
