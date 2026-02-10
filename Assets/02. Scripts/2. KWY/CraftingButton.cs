using UnityEngine;

public class CraftingButton : MonoBehaviour
{
    [SerializeField] CraftingItemSlot itemSlot;

    public CraftingSO recipe;

    public void SetRecipe(CraftingSO data)
    {
        recipe = data;

        itemSlot.SetItem(recipe.ResultItem);
    }
}
