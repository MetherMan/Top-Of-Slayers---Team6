using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private List<ItemSO> shopItems = new();

    public List<ItemSO> GetShopItems()
    {
        return shopItems;
    }

}
