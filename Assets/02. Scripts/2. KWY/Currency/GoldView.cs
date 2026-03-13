using System.Collections;
using TMPro;
using UnityEngine;

public class GoldView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI goldText;

    private int currentDisplayGold;

    private void Start()
    {
        currentDisplayGold = CurrencyManager.Instance.GetGold();
        goldText.text = currentDisplayGold.ToString();

        CurrencyManager.Instance.OnGoldChanged += Refresh;
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