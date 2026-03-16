using UnityEngine.SceneManagement;
using UnityEngine;
using TMPro;


public class MainSceenLoadVer2 : MonoBehaviour
{
    [SerializeField] private CirculDrag circulDrag;
    [SerializeField] private string[] stageSceneNames;

    [SerializeField] private EnergyManager energyManager;
    [SerializeField] private int stageEnergyCost = 10;

    [SerializeField] private GameObject failPanel;
    [SerializeField] private TextMeshProUGUI failText;

    //태완추가
    [SerializeField] private StageConfigSO[] stageSO;

    public void LoadSelectedStage()
    {

        int index = circulDrag.CurrentIdex;

        if(index < 0 || index >= stageSO.Length)
        {
            return;
        }

        //에너지 차감
        if (!energyManager.UseEnergy(stageEnergyCost))
        {
            failText.text = "에너지가 부족합니다.";
            failPanel.SetActive(true);
            return;
        }

        //string stageName = stageSceneNames[index];
        StageConfigSO stage = stageSO[index];
        if (stage != null)
        {
            StageDatabase.Instance.GetStageData(stage.name);
            StageFlowManager.Instance.stageIn = true;
            Debug.Log(stage.name);
            LoadingSceneController.LoadStage(stage.name);
        }

        //stage.clearResult = ClearResult.None;
        //StageManager.Instance.GetData(stage, StageDatabase.Instance);
    }
}
