using UnityEngine;
using UnityEngine.SceneManagement;

public class MainSceenLoadVer2 : MonoBehaviour
{
    [SerializeField] private CirculDrag circulDrag;
    [SerializeField] private string[] stageSceneNames;

    public void LoadSelectedStage()
    {
        int index = circulDrag.CurrentIdex;

        if(index >= 0 && index < stageSceneNames.Length)
        {
            LoadingSceneController.LoadScene(stageSceneNames[index]);
        }
    }
}
