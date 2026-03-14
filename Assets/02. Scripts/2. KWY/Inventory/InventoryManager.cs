using UnityEngine;
using System.Collections.Generic;
using System;
using UnityEngine.Experimental.GlobalIllumination;

public class InventoryManager : Singleton<InventoryManager>
{
    public List<InventoryItem> inventory = new List<InventoryItem>();
    public event Action OnInventoryChanged;
    protected override void Awake()
    {
        base.Awake();
        Init();
    }
    public void NotifyInventoryChanged()
    {

        OnInventoryChanged?.Invoke();
    }

    //firebase
    private void Init()
    {
        FirebaseManager.Instance.PushItemList(inventory);
    }

    //아이템 추가
    public void AddItem(ItemSO item, int amount = 1)
    {

        if (item == null) return;

        if (item is EquipmentSO)
        {   
            {
                inventory.Add(new InventoryItem
                {
                    item = item,
                    //count = 1,
                    enhancementLevel = 0
                });
            }

        }
        else
        {
            InventoryItem slot = inventory.Find(x => x.item == item);

            if (slot != null)
            {
                slot.count += amount;
            }
            else
            {
                inventory.Add(new InventoryItem
                {
                    item = item,
                    count = amount
                });
            }
        }
        OnInventoryChanged?.Invoke();
    }

    //제작/분해 아이템 제거
    public void RemoveItem(InventoryItem data)
    {
        inventory.Remove(data);
        OnInventoryChanged?.Invoke();
    }

    public bool SellItem(InventoryItem data)
    {
        if(data == null) return false;

        if(!(data.item is EquipmentSO)) return false;

        int sellPrice = data.item.price /2;

        CurrencyManager.Instance.Add(sellPrice);

        RemoveItem(data);

        return true;
    }


    //소비아이템 사용
    public bool RemoveItem(ItemSO item, int amount)
    {
        InventoryItem slot = inventory.Find(x => x.item == item);

        if (slot == null) return false;

        slot.count -= amount;

        if (slot.count <= 0)
            inventory.Remove(slot);

        OnInventoryChanged?.Invoke();
        return true;
    }


    //특정 아이템 보유 수량 조회
    public int GetItemCount(ItemSO item)
    {
        var slot = inventory.Find(x => x.item == item);
        return slot != null ? slot.count : 0;
    }

    public bool HasEnoughItem(ItemSO item, int amount)
    {
        return GetItemCount(item) >= amount;
    }

}
