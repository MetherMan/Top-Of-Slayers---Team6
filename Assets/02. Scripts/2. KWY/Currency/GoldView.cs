using Cysharp.Threading.Tasks;
using System.Collections;
using TMPro;
using UnityEngine;

public class GoldView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    private int currentDisplayGold;

    private async void Start()
    {
        try
        {
            await WithUntilInit();
            Init();
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"GoldView Start중 Error : {ex.Message}\n{ex.StackTrace}");
        }
    }

    private void Init()
    {
        Debug.Log("GoldView: 데이터 로드 완료. Init 실행");
        currentDisplayGold = CurrencyManager.Instance.GetGold();
        goldText.text = currentDisplayGold.ToString();

        Debug.LogFormat("<color=cyan>GoldView currentDisplayGold : {0}</color>", currentDisplayGold);
        Debug.LogFormat("<color=cyan>GoldView goldText : {0}</color>", goldText.text);

        CurrencyManager.Instance.OnGoldChanged += Refresh;
    }

    private async UniTask WithUntilInit()
    {
        while (!CurrencyManager.Instance.isCompleted)
        {
            await UniTask.Delay(1000);
        }
    }

    private void OnDestroy()
    {
        if (CurrencyManager.HasInstance)
            CurrencyManager.Instance.OnGoldChanged -= Refresh;
    }

    public void Refresh()
    {
        int targetGold = CurrencyManager.Instance.GetGold();

        StopAllCoroutines();
        StartCoroutine(Count(targetGold));
    }

    IEnumerator Count(int target)
    {
        int start = currentDisplayGold;

        float duration = 0.5f;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;

            int value = (int)Mathf.Lerp(start, target, time / duration);
            goldText.text = value.ToString();

            yield return null;
        }

        currentDisplayGold = target;
        goldText.text = target.ToString();
    }
}