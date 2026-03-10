using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class WaveUIAnim : MonoBehaviour
{
    [SerializeField] private RectTransform wavePanel;
    [SerializeField] private RectTransform waveContainer;

    [SerializeField] private GameObject prevSlot;
    [SerializeField] private GameObject currentSlot;
    [SerializeField] private GameObject nextSlot;

    [SerializeField] private TextMeshProUGUI prevText;
    [SerializeField] private TextMeshProUGUI currentText;
    [SerializeField] private TextMeshProUGUI nextText;

    [Header("패널 설정")]
    [SerializeField] private float expandTime;
    [SerializeField] private float stayTime;

    [SerializeField] private float moveDistance;
    [SerializeField] private float moveTime = 0.5f;
    private float normalSize;
    private float expandSize;

    private Vector2 originPos;

    private Coroutine waveCoroutine;
    private int endWave;

    public void Init(int endWave)
    {
        this.endWave = endWave;
    }

    private void Awake()
    {
        if(wavePanel != null)
        {
            normalSize = wavePanel.sizeDelta.x;
            expandSize = normalSize * 3f;
        }
        if(waveContainer != null)
        {
            originPos = waveContainer.anchoredPosition;
        }
        prevSlot.gameObject.SetActive(false);
        nextSlot.gameObject.SetActive(false);
    }
    public void PlayWavePanel(int currentWave)
    {
        if(currentWave == 1)
        {
            SetNormalWave(1);
            return;
        }
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }
        waveCoroutine = StartCoroutine(WaveCoroutine(currentWave));
    }

    private IEnumerator WaveCoroutine(int wave)
    {
        if (wave - 1 >= endWave)
        {
            yield return StartCoroutine(ClearCoroutine());
            yield break;
        }
        Debug.Log($"현재 웨이브{wave}");

        prevSlot.SetActive(true);
        currentSlot.SetActive(true);
        nextSlot.SetActive(true);

        prevText.text = (wave - 2).ToString();
        currentText.text = (wave - 1).ToString();
        nextText.text = (wave).ToString();

        if(wave == 2) prevSlot.SetActive(false);


        waveContainer.anchoredPosition = originPos;

        yield return StartCoroutine(AdjustSize(expandSize));

        yield return new WaitForSecondsRealtime(0.2f);

        Vector2 nextPos = originPos + Vector2.left * moveDistance;
        yield return StartCoroutine(MoveContainer(nextPos));


        prevText.text = (wave - 1).ToString();
        prevSlot.SetActive(false);
        currentText.text = (wave).ToString();
        nextText.text = (wave + 1).ToString();
        waveContainer.anchoredPosition = originPos;
        prevSlot.SetActive(true);

        if(wave >= endWave) nextSlot.SetActive(false);
        else nextSlot.SetActive(true);


        yield return new WaitForSecondsRealtime(stayTime);

        yield return StartCoroutine(AdjustSize(normalSize));

        SetNormalWave(wave);
    }

    private IEnumerator ClearCoroutine()
    {
        prevSlot.SetActive(false);
        currentSlot.SetActive(false);
        nextSlot.SetActive(false);

        yield return StartCoroutine(AdjustSize(expandSize));
    }

    private IEnumerator AdjustSize(float nextSize)
    {
        float startSize = wavePanel.sizeDelta.x;
        float timer = 0f;

        while(timer < expandTime)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / expandTime;
            float size = Mathf.Lerp(startSize, nextSize, t);
            wavePanel.sizeDelta = new Vector2(size, wavePanel.sizeDelta.y);
            yield return null;
        }
        wavePanel.sizeDelta = new Vector2(nextSize, wavePanel.sizeDelta.y);
    }

    private IEnumerator MoveContainer(Vector2 nextPos)
    {
        Vector2 startPos = waveContainer.anchoredPosition;
        float timer = 0f;

        while (timer < moveTime)
        {
            timer += Time.unscaledDeltaTime;
            float t = timer / moveTime;

            waveContainer.anchoredPosition = Vector2.Lerp(startPos, nextPos, t);
            yield return null;
        }
        waveContainer.anchoredPosition = nextPos;
    }

    public void SetNormalWave(int wave)
    {
        wavePanel.sizeDelta = new Vector2(normalSize, wavePanel.sizeDelta.y);

        prevSlot.SetActive(false);
        nextSlot.SetActive(false);

        currentSlot.SetActive(true);
        currentText.text = wave.ToString();

        waveContainer.anchoredPosition = originPos;
    }
}
