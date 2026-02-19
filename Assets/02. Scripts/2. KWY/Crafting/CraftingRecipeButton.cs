using UnityEngine;

public class CraftingRecipeButton : MonoBehaviour
{
    [SerializeField] CraftingItemSlot itemSlot;
    [SerializeField] CraftingUI craftingUI;


    public CraftingSO recipe;

    //제작 레시피 데이터 저장, 슬롯ui에 결과 아이템 표시
    public void SetRecipe(CraftingSO data)
    {
        recipe = data;

        itemSlot.SetItem(recipe.ResultItem);
    }
    //레시피 선택시 제작 상세 ui에 전달
    public void Click()
    {
        craftingUI.ShowRecipe(recipe);
    }
}
