using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using TMPro;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;

public class LoadingSceneController : MonoBehaviour
{
    static string targetKey;
    static bool isStageLoading;

    [SerializeField] Slider slider;
    [SerializeField] TextMeshProUGUI loadingText;

    void Start()
    {
        StartCoroutine(LoadAddressableSceneProcess());
    }
    
    public static void LoadStage(string key) //스테이지 이동
    {
        targetKey = key;
        isStageLoading = true;
        SceneManager.LoadScene("LoadingScene");
    }

    public static void LoadScene(string key) //로비, 로그인
    {
        targetKey = key;
        isStageLoading = false;
        SceneManager.LoadScene("LoadingScene");
    }

    //스테이지 이동용 LoadScene 메서드 코드작성

    IEnumerator LoadAddressableSceneProcess()
    {
        AsyncOperationHandle<SceneInstance> op = isStageLoading
            ? AddressableManager.Instance.RequestStageScene(targetKey) //StageSO 이름
            : AddressableManager.Instance.RequestScene(targetKey); // SceneRemote 이름

        while (op.IsValid() && !op.IsDone)
        {
            float progress = op.PercentComplete;
            UpdateUI(progress);
            yield return null;
        }

        if (op.Status == AsyncOperationStatus.Succeeded)
        {
            UpdateUI(1f);
            yield return new WaitForSeconds(0.5f);
        }
        else
        {
            loadingText.text = "loading failed";
        }
    }

    void UpdateUI(float progress)
    {
        slider.value = progress;
        loadingText.text = $"{Mathf.RoundToInt(progress * 100)}%";
    }
}
