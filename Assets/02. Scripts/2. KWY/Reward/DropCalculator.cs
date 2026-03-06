using UnityEngine;

public class DropCalculator
{
    public static bool Roll(float chance)
    {
        return Random.value <= chance;
    }
}
