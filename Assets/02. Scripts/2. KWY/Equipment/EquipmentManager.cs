using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class EquipmentManager : Singleton<EquipmentManager>
{
    public InventoryItem weapon;
    public InventoryItem shoes;
    public InventoryItem gloves;
    public InventoryItem armor;
    public InventoryItem emblem;

    public event Action OnEquipmentChanged;

    //firebase
    protected override void Awake()
    {
        base.Awake();
    }

    private async void Start()
    {
        try
        {
            await WaitUntilDataLoaded();
            Init();
        } 
        catch (System.Exception ex)
        {
            Debug.LogWarning($"EquipmentManager Start중 Error: {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void Init()
    {
        Debug.Log("EquipmentManager: 데이터 로드 완료. Init 실행");
        FirebaseManager.Instance.PushEquipment(ref weapon, ref shoes, ref gloves, ref armor, ref emblem);
    }

    private async UniTask WaitUntilDataLoaded()
    {
        while (!FirebaseManager.Instance.IsDataLoaded)
        {
            Debug.LogFormat("IsDataLoaded : {0}", FirebaseManager.Instance.IsDataLoaded);
            await UniTask.Delay(500);
        }
    }

    public void Equip(InventoryItem data)
    {
        if (!(data.item is EquipmentSO equip)) return;

        switch (equip.equipSlot)
        {
            case EquipSlot.Weapon:
                weapon = data;
                break;
            case EquipSlot.Shoes:
                shoes = data;
                break;
            case EquipSlot.Gloves:
                gloves = data;
                break;
            case EquipSlot.Armor:
                armor = data;
                break;
            case EquipSlot.Ring:
                emblem = data;
                break;
        }

        FirebaseManager.Instance.SaveEquipment(
            weapon, shoes, gloves, armor, emblem, FirebaseManager.Instance.UID
            );

        OnEquipmentChanged?.Invoke();
    }

    public void Unequip(EquipSlot slot)
    {
        switch (slot)
        {
            case EquipSlot.Weapon:
                weapon = null;
                break;

            case EquipSlot.Shoes:
                shoes = null;
                break;

            case EquipSlot.Gloves:
                gloves = null;
                break;

            case EquipSlot.Armor:
                armor = null;
                break;

            case EquipSlot.Ring:
                emblem = null;
                break;
        }

        FirebaseManager.Instance.SaveEquipment(
            weapon, shoes, gloves, armor, emblem, FirebaseManager.Instance.UID
            );

        OnEquipmentChanged?.Invoke();
    }

    public bool IsEquipped(InventoryItem data)
    {
        return weapon == data ||
            armor == data ||
            shoes == data ||
            gloves == data ||
            emblem == data;
    }
}