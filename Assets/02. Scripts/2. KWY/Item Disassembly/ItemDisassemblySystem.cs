using System.Collections.Generic;
using UnityEngine;

public class ItemDisassemblySystem : MonoBehaviour
{
    [SerializeField] List<ItemDisassemblySO> recipes = new List<ItemDisassemblySO>();
    //아이템의 분해 레시피 찾아서 반환
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
    //분해 실행
    public void Disassembly(InventoryItem data)
    {
        if (data == null) return;

        var recipe = GetRecipe(data.item);

        if (recipe == null) return;

        InventoryManager.Instance.RemoveItem(data);

        for (int i = 0; i < recipe.resultItems.Length; i++)
        {
            InventoryManager.Instance.AddItem(recipe.resultItems[i],
                recipe.resultCounts[i]);
        }
    }
}
