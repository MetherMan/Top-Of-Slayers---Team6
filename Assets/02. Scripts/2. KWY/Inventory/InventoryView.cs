using System.Linq;
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
        var manager = InventoryManager.Instance;

        if (manager != null)
        {
            manager.OnInventoryChanged += RefreshUI;
            RefreshUI();
        }
    }

    private void OnDisable()
    {
        var manager = InventoryManager.Instance;

        if (manager != null)
        {
            manager.OnInventoryChanged -= RefreshUI;
        }
    }

    public void RefreshUI()
    {
        slotsUI = slotRoot.GetComponentsInChildren<InventoryItemSlot>();

        var inventoryData = InventoryManager.Instance.inventory;

        var sortList = inventoryData.OrderBy(x => x.item.grade).ToList();

        for (int i = 0; i < slotsUI.Length; i++)
        {
            if (i < sortList.Count)
            {
                slotsUI[i].SetItem(sortList[i]);
            }
            else
            {
                slotsUI[i].ClearItem();
            }
        }
    }
}
