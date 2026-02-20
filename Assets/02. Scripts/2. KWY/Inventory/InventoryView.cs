using UnityEngine;

public class InventoryView : MonoBehaviour
{
    [SerializeField] Transform slotRoot;
    private InventoryItemSlot[] slotsUI;
    [SerializeField] InventorySelection inventorySelection;
    private void Start()
    {
        var slots = GetComponentsInChildren<InventoryItemSlot>();

        foreach (var slot in slots) 
        {
            slot.SetSelection(inventorySelection);
        }
    }
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
        if (InventoryManager.HasInstance)
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
                slotsUI[i].SetItem(inventoryData[i]);
            }
            else
            {
                slotsUI[i].ClearItem();
            }
        }
    }
}
