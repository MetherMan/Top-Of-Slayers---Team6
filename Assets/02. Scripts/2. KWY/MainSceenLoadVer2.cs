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

    //현재는 로드가 안됨 준완님이 코드 추가하시면 자동 연결 예정
    //대신 stageSo에 있는 이름이 어드레서블 네임이니 그거를 인스펙터에 정확하게 입력해야한다.
    //하면서 씬이름도 어드레서블 이름이랑 같게 하자
    public void LoadSelectedStage()
    {

        int index = circulDrag.CurrentIdex;

        //if(index < 0 || index >= stageSceneNames.Length)
        //{
        //    return;
        //}
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
        if (stage == null)
        {
            return;
        }

        stage.clearResult = ClearResult.None;
        StageManager.Instance.GetData(stage, StageDatabase.Instance);
        LoadingSceneController.LoadScene(stage.sceneReference.editorAsset.name);


        ////테스트용
        //SceneManager.LoadScene(stageName);

        //실제사용
        //SceneLoadManager.Instance.ActiveScene(stageName);
        //StageDatabase.Instance.GetStageData(stageName);
    }
}
