using UnityEngine;
using System.Collections.Generic;

public class InventoryManager : Singleton<InventoryManager>
{
    [SerializeField] ItemDataSO itemDatabase;
    [SerializeField] Transform slotRoot;

    InventorySlotUI[] slotsUI;
    List<InventorySlot> inventory = new List<InventorySlot>();

    //테스트용 아이템
    [SerializeField] ItemSO testItem1;
    [SerializeField] ItemSO testItem2;
    [SerializeField] ItemSO testItem3;
    [SerializeField] ItemSO testItem4;

    private void Start()
    {
        slotsUI = slotRoot.GetComponentsInChildren<InventorySlotUI>();

        //테스트용 아이템
        AddItem(testItem1, 1000);
        AddItem(testItem2, 1);
        AddItem(testItem3, 1);
        AddItem(testItem4 , 1);

    }

    public void AddItem(ItemSO item, int amount)
    {
        if (item == null) return;


        foreach (var slot in inventory)
        {
            if (slot.item == item)
            {
                slot.count += amount;
                RefreshUI();
                return;
            }
        }
        inventory.Add(new InventorySlot { item = item, count = amount });

        RefreshUI();
    }

    public void RefreshUI()
    {
        for (int i = 0; i < slotsUI.Length; i++)
        {
            if (i < inventory.Count)
            {
                slotsUI[i].SetItem(inventory[i].item, inventory[i].count);
            }
            else
            {
                slotsUI[i].ClearItem();
            }
        }
    }
    
}
