using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUI : MonoBehaviour
{
    [SerializeField] Image resultImage;
    [SerializeField] TextMeshProUGUI itemDitails;
    [SerializeField] TextMeshProUGUI resultName;

    [SerializeField] Image mat1Image;
    [SerializeField] TextMeshProUGUI mat1Count;

    [SerializeField] Image mat2Image;
    [SerializeField] TextMeshProUGUI mat2Count;

    [SerializeField] UIRootManager uiRootManager;

    CraftingSO currentRecipe;

    public void ShowRecipe(CraftingSO recipe)
    {
        currentRecipe = recipe;

        resultImage.sprite = recipe.ResultItem.sprite;
        resultName.text = recipe.ResultItem.itemName;

        if (recipe.ResultItem is EquipmentSO equip)
        {
            itemDitails.text = equip.GetStatText();

        }
        int have1 = InventoryManager.Instance.GetItemCount(recipe.materials[0]);
        int need1 = recipe.materialCounts[0];

        mat1Image.sprite = recipe.materials[0].sprite;
        mat1Count.text = $"{have1}/{need1}";

        int have2 = InventoryManager.Instance.GetItemCount(recipe.materials[1]);
        int need2 = recipe.materialCounts[1];

        mat2Image.sprite = recipe.materials[1].sprite;
        mat2Count.text = $"{have2}/{need2}";
    }
    public void Craft()
    {
        if (currentRecipe == null) return;

        for (int i = 0; i < currentRecipe.materials.Count; i++)
        {
            if (!InventoryManager.Instance.HasEnoughItem(
                currentRecipe.materials[i],
                currentRecipe.materialCounts[i]))
            {
                uiRootManager.OpenCraftingFailPanel();
                return;
            }
        }

        for (int i = 0; i < currentRecipe.materials.Count; i++)
        {
            InventoryManager.Instance.RemoveItem(
                currentRecipe.materials[i],
                currentRecipe.materialCounts[i]);
            
        }

        // 결과 아이템 추가
        InventoryManager.Instance.AddItem(currentRecipe.ResultItem, 1);

        // UI 갱신
        ShowRecipe(currentRecipe);

        uiRootManager.OpenCraftingSuccesPanel();


    }

}
