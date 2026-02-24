using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DisassemblyUI : MonoBehaviour
{
    [SerializeField] Image selectedItemImage;
    [SerializeField] Image disassemblyItemImage;

    [SerializeField] Sprite defaultSelectedItemImage;
    [SerializeField] Sprite defaultDisassemblyItemImage;

    [SerializeField] TextMeshProUGUI disassemblyItemCount;

    [SerializeField] GameObject inventoryPanel;
    [SerializeField] GameObject failPanel;
    [SerializeField] TextMeshProUGUI failText;

    [SerializeField] ItemDisassemblySystem itemDisassemblySystem;
    [SerializeField] InventorySelection inventorySelection;

    InventoryItem selectedItem;

    private void OnDisable()
    {
        ResetUI();
    }

    //인벤토리 패널 열기, 아이템 선택 모드 활성화
    public void OnClickSelectedItem()
    {
        inventoryPanel.SetActive(true);

        inventorySelection.EnableSelectMode(OnItemSelected);
    }
    //선택된 아이템 ui에 반영
    public void OnItemSelected(InventoryItem data)
    {
        if (!(data.item is EquipmentSO equip))
        {
            failText.text = "재료아이템은\r\n 분해하지 못합니다.";
            failPanel.SetActive(true);
            inventorySelection.EnableSelectMode(OnItemSelected);
            return;
        }
        //아이템 장착시 불가
        if (EquipmentManager.Instance.IsEquipped(data))
        {
            failText.text = "장착 중인 장비는\r\n 분해할 수 없습니다.";
            failPanel.SetActive(true);   
            inventorySelection.EnableSelectMode(OnItemSelected);
            return;
        }

        selectedItem = data;
        selectedItemImage.sprite = data.item.sprite;

        var recipe = itemDisassemblySystem.GetRecipe(data.item);

        if (recipe != null && recipe.resultItems.Length > 0) 
        {
            disassemblyItemImage.sprite = recipe.resultItems[0].sprite;
            disassemblyItemCount.text = recipe.resultCounts[0].ToString();
        }
        else
        {
            disassemblyItemImage.sprite = defaultDisassemblyItemImage;
            disassemblyItemCount.text = "";

        }

        inventoryPanel.SetActive(false);
    }
    //클릭시 분해
    public void OnClickDisassembly()
    {

        if (selectedItem == null) return;

        itemDisassemblySystem.Disassembly(selectedItem);

        ResetUI();

    }

    private void ResetUI()
    {
        selectedItem = null;

        selectedItemImage.sprite = defaultSelectedItemImage;
        disassemblyItemImage.sprite = defaultDisassemblyItemImage;
        disassemblyItemCount.text = "";

    }
}
