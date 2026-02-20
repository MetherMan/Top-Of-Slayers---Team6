using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Shoes")]
public class ShoesSO : EquipmentSO
{
    public int speed;

    public override int GetSpeed(int level)
    {
        return speed + level * 3;
    }

    public override string GetStatText(int level)
    {
        return $"이동속도 +{GetSpeed(level)}";
    }
}
