using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField] Transform slotRoot;
    private InventoryItemSlot[] slotsUI;

    private void OnEnable()
    {
        if(InventoryManager.Instance != null)
        {
            slotsUI = slotRoot.GetComponentsInChildren<InventoryItemSlot>();

            InventoryManager.Instance.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }
    }

    private void OnDisable()
    {
        if(InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= RefreshUI;
        }
    }

    public void RefreshUI()
    {
        slotsUI = slotRoot.GetComponentsInChildren<InventoryItemSlot>();

        var inventoryData = InventoryManager.Instance.inventory;

        for (int i = 0; i < slotsUI.Length; i++)
        {
            if (i < inventoryData.Count)
            {
                slotsUI[i].SetItem(inventoryData[i].item, inventoryData[i].count);
            }
            else
            {
                slotsUI[i].ClearItem();
            }
        }
    }
}
