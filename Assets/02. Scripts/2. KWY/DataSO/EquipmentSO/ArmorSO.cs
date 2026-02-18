using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Armor")]
public class ArmorSO : EquipmentSO
{
    public int hp;

    public override string GetStatText()
    {
        return $"체력 +{hp}";
    }
}
