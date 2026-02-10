using UnityEngine;

[CreateAssetMenu(menuName = "Data/Equipment/Shoes")]
public class ShoesSO : EquipmentSO
{
    public int speed;

    public override string GetStatText()
    {
        return $"이동속도 +{speed}";
    }
}
