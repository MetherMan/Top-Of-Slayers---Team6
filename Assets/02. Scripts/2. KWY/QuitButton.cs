using UnityEngine;

public class QuitButton : MonoBehaviour
{
    public void OnClickQuit()
    {
        QuitApplicationUtility.QuitApp();
    }
}
