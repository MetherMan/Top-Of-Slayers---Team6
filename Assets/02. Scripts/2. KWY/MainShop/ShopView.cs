using UnityEngine;

public class ShopView : MonoBehaviour
{
    [SerializeField] ShopManager shopManager;
    [SerializeField] Transform slotRoot;

    ShopSlotUI[] slots;

    private void OnEnable()
    {
        slots = slotRoot.GetComponentsInChildren<ShopSlotUI>();
        RefreshUI();
    }

    void RefreshUI()
    {
        var shopItems = shopManager.GetShopItems();

        for (int i = 0; i < slots.Length; i++)
        {
            if (i < shopItems.Count)
            {
                slots[i].SetItem(shopItems[i]);
            }
            else
            {
                slots[i].ClearItem();
            }
        }
    }
}
