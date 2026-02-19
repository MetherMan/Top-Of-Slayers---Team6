using TMPro;
using UnityEngine;

public class GachaTableUI : MonoBehaviour
{
    [SerializeField] private GachaDataSO gachaData;
    [SerializeField] private TextMeshProUGUI tableText;

    //패널 켜질때 테이블 보여주기
    private void OnEnable()
    {
        ShowTable();
    }

    public void ShowTable()
    {
        if(gachaData == null)
        {
            return;
        }

        float totalChance = gachaData.TotalChance();

        foreach(var entry in gachaData.items)
        {
            float percent = (entry.chance / totalChance) * 100f;
            //테이블에 아이템 이름과 확률
            tableText.text += $"{entry.item.name}({percent})%\n";
        }
    }
}
