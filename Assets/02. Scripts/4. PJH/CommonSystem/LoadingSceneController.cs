using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;

public class LoadingSceneController : MonoBehaviour
{
    static string nextScene;

    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI loadingText;

    void Start()
    {
        StartCoroutine(LoadSceneProcess());
    }

    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        SceneManager.LoadScene("LoadingScene");
    }

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0f;
        while (op.isDone == false)
        {
            slider.value = op.progress;
            if (op.progress < 0.9f) loadingText.text = $"{Mathf.RoundToInt(op.progress * 100)}%";

            if (op.progress >= 0.9f)
            {
                timer += Time.unscaledDeltaTime;
                slider.value = Mathf.Lerp(0.9f, 1f, timer);
                loadingText.text = $"{Mathf.RoundToInt(slider.value * 100)}%";
                if (slider.value >= 1f)
                {
                    op.allowSceneActivation = true;
                    yield break;
                }
            }
            yield return null;
        }
        yield break;
    }
}
