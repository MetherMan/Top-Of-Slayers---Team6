using UnityEngine;

public class CraftingRecipeUI : MonoBehaviour
{
    [SerializeField] CraftingSO[] recipes;
    [SerializeField] CraftingRecipeButton[] slots;

    private void Start()
    {
        for(int i= 0; i< recipes.Length; i++)
        {
            if (i < recipes.Length)
            {
                slots[i].SetRecipe(recipes[i]);
            }
        }
    }
}
