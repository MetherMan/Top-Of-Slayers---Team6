using System.Collections.Generic;
using UnityEngine;

public class ItemDisassemblySystem : MonoBehaviour
{
    [SerializeField] List<ItemDisassemblySO> recipes = new List<ItemDisassemblySO>();

    public ItemDisassemblySO GetRecipe(ItemSO item)
    {
        for(int i = 0; i<recipes.Count; i++)
        {
            if (recipes[i].targetItem == item)
            {
                return recipes[i];
            }
        }
        return null;
    }

    public void Disassembly(ItemSO item)
    {
        if (item == null) return;

        var recipe = GetRecipe(item);

        if (recipe == null) return;

        InventoryManager.Instance.RemoveItem(item, 1);

        for (int i = 0; i < recipe.resultItems.Length; i++)
        {
            InventoryManager.Instance.AddItem(recipe.resultItems[i],
                recipe.resultCounts[i]);
        }
    }
}
