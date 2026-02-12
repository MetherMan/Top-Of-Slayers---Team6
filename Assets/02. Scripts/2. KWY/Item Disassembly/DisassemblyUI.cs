using UnityEngine;
using UnityEngine.UI;

public class DisassemblyUI : MonoBehaviour
{
    [SerializeField] Image selectedItemImage;
    [SerializeField] GameObject inventoryPanel;

    ItemSO selectedItem;

    public void OnClickSelectedItem()
    {
        inventoryPanel.SetActive(true);

        InventorySelection.Instance.EnableSelectMode(OnItemSelected);
    }

    public void OnItemSelected(ItemSO item)
    {
        selectedItem = item;
        selectedItemImage.sprite = item.sprite;

        inventoryPanel.SetActive(false);
    }
}
