using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] Transform slotRoot;
    private InventorySlotUI[] slotsUI;

    private void OnEnable()
    {
        if(InventoryManager.Instance != null)
        {
            slotsUI = slotRoot.GetComponentsInChildren<InventorySlotUI>();

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
        slotsUI = slotRoot.GetComponentsInChildren<InventorySlotUI>();

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
