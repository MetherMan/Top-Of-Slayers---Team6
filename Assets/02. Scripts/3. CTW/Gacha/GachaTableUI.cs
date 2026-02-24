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
        if (gachaData == null)
        {
            return;
        }

        tableText.text = "";

        tableText.text += $"Legend : {gachaData.legendChance}%\n\n";
        tableText.text += $"Epic : {gachaData.epicChance}%\n\n";
        tableText.text += $"Normal : {gachaData.normalChance}%\n\n";


    }
}
