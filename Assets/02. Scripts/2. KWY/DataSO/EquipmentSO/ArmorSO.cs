using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Armor")]
public class ArmorSO : EquipmentSO
{
    public int hp;

    public override int GetHP(int level)
    {
        return hp + level * 5;
    }

    public override string GetStatText(int level)
    {
        return $"체력 +{GetHP(level)}";
    }
}
