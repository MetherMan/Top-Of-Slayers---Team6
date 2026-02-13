using UnityEngine;
using UnityEngine.UI;

public class DisassemblyUI : MonoBehaviour
{
    [SerializeField] Image selectedItemImage;
    [SerializeField] Image disassemblyItemImage;

    [SerializeField] Sprite defaultselectedItemImage;
    [SerializeField] Sprite defaultdisassemblyItemImage;

    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject failPaenl;

    [SerializeField] ItemDisassemblySystem itemDisassemblySystem;

    ItemSO selectedItem;

    public void OnClickSelectedItem()
    {
        inventoryPanel.SetActive(true);

        InventorySelection.Instance.EnableSelectMode(OnItemSelected);
    }

    public void OnItemSelected(ItemSO item)
    {
        if (!(item is EquipmentSO))
        {
            failPaenl.SetActive(true);
            return;
        }

        selectedItem = item;
        selectedItemImage.sprite = item.sprite;

        var recipe = itemDisassemblySystem.GetRecipe(item);

        if (recipe != null) 
        {
            disassemblyItemImage.sprite = recipe.resultItems[0].sprite;
        }

        inventoryPanel.SetActive(false);
    }

    public void OnClickDisassembly()
    {

        if (selectedItem == null) return;

        itemDisassemblySystem.Disassembly(selectedItem);

        selectedItem = null;
        selectedItemImage.sprite = defaultselectedItemImage;
        disassemblyItemImage.sprite = defaultdisassemblyItemImage;
    }
}
