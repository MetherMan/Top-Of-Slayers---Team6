using UnityEngine;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] CraftingSO[] recipes;
    [SerializeField] CraftingButton[] slots;

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
