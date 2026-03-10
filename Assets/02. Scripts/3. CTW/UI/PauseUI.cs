using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseUI : MonoBehaviour
{
    [SerializeField] private GameObject pausePanel;

    private void Awake()
    {
        if(pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void ClickPause()
    {
        if (pausePanel != null)
        {
            pausePanel.SetActive(true);
        }
        Time.timeScale = 0f;
    }

    public void ClickResume()
    {
        Time.timeScale = 1f;
        if (pausePanel != null)
        {
            pausePanel.SetActive(false);
        }
    }

    public void ClickRestart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ClickMain()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("1.Lobby");
    }
}
