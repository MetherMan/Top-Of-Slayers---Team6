using UnityEngine;

public class CraftingRecipeButton : MonoBehaviour
{
    [SerializeField] CraftingItemSlot itemSlot;
    [SerializeField] CraftingUI craftingUI;


    public CraftingSO recipe;

    public void SetRecipe(CraftingSO data)
    {
        recipe = data;

        itemSlot.SetItem(recipe.ResultItem);
    }
    public void Click()
    {
        craftingUI.ShowRecipe(recipe);
    }
}
