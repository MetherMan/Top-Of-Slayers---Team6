using UnityEngine;

[CreateAssetMenu(menuName = "Data/DisassemblyRecipe")]
public class ItemDisassemblySO : ScriptableObject
{
    public ItemSO targetItem;

    public ItemSO[] resultItems;
    public int[] resultCounts;
}
