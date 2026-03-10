using UnityEngine;
using UnityEngine.SceneManagement;

public class AppRestartHandler : MonoBehaviour
{
    void OnApplicationPause(bool pause)
    {
        if (!pause)
        {
            if (SceneManager.GetActiveScene().name != "1.Lobby")
            {
                SceneManager.LoadScene("1.Lobby");
            }
        }
    }
}