using System;
using UnityEngine;

public class InventorySelection : MonoBehaviour
{
    [SerializeField] ItemPopupUI itemPopup;
    private Action<InventoryItem> onItemSelected;

    //아이템 선택 대기 상태 ON
    public void EnableSelectMode(Action<InventoryItem> callback)
    {
        onItemSelected = callback;
    }
    //아이템 선택 대기 상태 off
    public void DisableSelectMode()
    {
        onItemSelected = null;
    }
    //슬롯에서 클릭됐다고 알려주는 함수
    public void NotifyItemClicked(InventoryItem item) 
    {
        if (item == null) return;

        // 선택 모드라면 → 콜백
        if (onItemSelected != null)
        {
            onItemSelected.Invoke(item);
            DisableSelectMode();
        }
        else
        {
            if (itemPopup != null)
                itemPopup.Show(item);
        }
    }
}
