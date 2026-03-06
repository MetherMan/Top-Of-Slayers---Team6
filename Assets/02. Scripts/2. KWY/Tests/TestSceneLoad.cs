using UnityEngine;
using UnityEngine.SceneManagement;

public class TestSceneLoad : MonoBehaviour
{
    public void NewSceneLoad()
    {
        SceneManager.LoadScene("New Scene");
    }
}
