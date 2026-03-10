using TMPro;
using UnityEngine;

public class StageUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI stageText;

    private void Start()
    {
        //int clearStage = 스테이지매니저.현재클리어스테이지 메서드();

        //stageText.text = $"Stage{clearStage + 1}";
    }
}
