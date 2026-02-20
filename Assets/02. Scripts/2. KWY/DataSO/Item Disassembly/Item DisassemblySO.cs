using UnityEngine;

[CreateAssetMenu(menuName = "Data/DisassemblyRecipe")]
public class ItemDisassemblySO : ScriptableObject
{
    public EquipmentSO targetItem;

    public ItemSO[] resultItems;
    public int[] resultCounts;
}
