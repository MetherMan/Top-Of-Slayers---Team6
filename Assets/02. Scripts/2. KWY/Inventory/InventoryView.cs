using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class InventoryView : MonoBehaviour
{
    [SerializeField] Transform slotRoot;
    [SerializeField] GameObject slotPrefab;
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
        }
        EquipmentManager.Instance.OnEquipmentChanged += RefreshUI;
        RefreshUI();

    }

    private void OnDisable()
    {
        var manager = InventoryManager.Instance;

        if (manager != null)
        {
            manager.OnInventoryChanged -= RefreshUI;
        }

        if(EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshUI;
        }
    }

    public void RefreshUI()
    {
        var inventoryData = InventoryManager.Instance.inventory;
        var sortList = inventoryData.OrderBy(x => x.item.grade).ToList();

        int currentSlotCount = slotRoot.childCount;

        if (currentSlotCount < sortList.Count)
        {
            int needCreate = sortList.Count - currentSlotCount;

            for (int i = 0; i < needCreate; i++)
            {
                GameObject obj = Instantiate(slotPrefab, slotRoot);

                var slot = obj.GetComponent<InventoryItemSlot>();
                slot.SetSelection(inventorySelection);
            }
        }
        slotsUI = slotRoot.GetComponentsInChildren<InventoryItemSlot>();

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
