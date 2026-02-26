using UnityEngine;

public class SceneLoadTest : MonoBehaviour
{
    void Start()
    {
        
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadingSceneController.LoadScene("TextScene02");
        }
    }
}
