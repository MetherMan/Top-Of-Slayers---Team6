using UnityEngine;
using System.Collections.Generic;
using System;

public class InventoryManager : Singleton<InventoryManager>
{
    public List<InventoryItem> inventory = new List<InventoryItem>();
    public event Action OnInventoryChanged;
    protected override void Awake()
    {
        base.Awake();
    }
    public void NotifyInventoryChanged()
    {
        OnInventoryChanged?.Invoke();
    }

    //아이템 추가
    public void AddItem(ItemSO item, int amount = 1)
    {

        if (item == null) return;

        if (item is EquipmentSO)
        {
            inventory.Add(new InventoryItem
            {
                item = item,
                count = 1,
                enhancementLevel = 0
            });
        }
        else
        {
            // 재료는 스택
            var slot = inventory.Find(x => x.item == item);

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

    //소비아이템 사용(가챠권 사용때 사용 예정)

    public bool RemoveItem(ItemSO item, int amount)
    {
        var slot = inventory.Find(x => x.item == item);

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
