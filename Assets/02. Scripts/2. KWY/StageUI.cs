using TMPro;
using UnityEngine;

public class StageUI : MonoBehaviour
{
    int currentStage;
    int clearStage;
    [SerializeField] TextMeshProUGUI stageText;

    private void Awake()
    {
        Init();
    }

    void Init()
    {
        //currentStage = 파이어베이스데이터;
        stageText.text = $"{currentStage}Stage";
    }

    public void ClearStageUI()
    {
        //clearStage = 스테이지매니저.현재클리어스테이지 메서드();
        currentStage = clearStage + 1;
        stageText.text = $"{currentStage}Stage";
    }
}
