using UnityEngine;

public class MainSceenLoadVer2 : MonoBehaviour
{
    [SerializeField] private CirculDrag circulDrag;
    [SerializeField] private string[] stageSceneNames;

    //현재는 로드가 안됨 준완님이 코드 추가하시면 자동 연결 예정 대신 stageSo에 있는 이름이 어드레서블 네임이니 그거를 인스펙터에 정확하게 입력해야한다. 하면서 씬이름도 어드레서블 이름이랑 같게 하자
    public void LoadSelectedStage()
    {

        int index = circulDrag.CurrentIdex;

        if(index >= 0 && index < stageSceneNames.Length)
        {
            string stageName = stageSceneNames[index];

            SceneLoadManager.Instance.ActiveScene(stageName);
        }
    }
}
