using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<ItemSO> shopItems = new List<ItemSO>();

    //판매 아이템 리스트
    public List<ItemSO> GetShopItems()
    {
        return shopItems;
    }
    //아이템 구매
    public bool TryBuy(ItemSO item)
    {
        if (item == null) return false;

        if (!CurrencyManager.Instance.HasEnough(item.price))
        {
            //골드 부족 판네 띄우기
            return false;
        }

        CurrencyManager.Instance.Spend(item.price);
        InventoryManager.Instance.AddItem(item, 1);

        return true;
    }

}
