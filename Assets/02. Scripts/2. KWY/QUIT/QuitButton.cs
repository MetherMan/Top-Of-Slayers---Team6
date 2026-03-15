using UnityEngine;
using UnityEngine.SceneManagement;

public class QuitButton : MonoBehaviour
{
    public void OnClickQuit()
    {
        QuitApplicationUtility.QuitApp();
    }
}
