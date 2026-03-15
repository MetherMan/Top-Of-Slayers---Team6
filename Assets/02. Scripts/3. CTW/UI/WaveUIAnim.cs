using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class WaveUIAnim : MonoBehaviour
{
    [SerializeField] private CanvasGroup panelGroup;
    [SerializeField] private RectTransform wavePanel;
    [SerializeField] private RectTransform waveContainer;

    [SerializeField] private GameObject prevSlot;
    [SerializeField] private GameObject currentSlot;
    [SerializeField] private GameObject nextSlot;

    [SerializeField] private TextMeshProUGUI prevText;
    [SerializeField] private TextMeshProUGUI currentText;
    [SerializeField] private TextMeshProUGUI nextText;

    [SerializeField] private TextMeshProUGUI clearText;
    [SerializeField] private TextMeshProUGUI failText;

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
        ResetPanelState();
    }

    private void OnEnable()
    {
        ResetPanelState();
    }

    public void ResetPanelState()
    {
        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
            waveCoroutine = null;
        }

        panelGroup?.DOKill();
        wavePanel?.DOKill();
        waveContainer?.DOKill();

        if (wavePanel != null)
        {
            wavePanel.sizeDelta = new Vector2(normalSize, wavePanel.sizeDelta.y);
            wavePanel.gameObject.SetActive(false);
        }

        if (waveContainer != null)
        {
            waveContainer.anchoredPosition = originPos;
        }

        if (panelGroup != null)
        {
            panelGroup.alpha = 0f;
        }

        if (prevSlot != null) prevSlot.SetActive(false);
        if (currentSlot != null) currentSlot.SetActive(false);
        if (nextSlot != null) nextSlot.SetActive(false);
        if (clearText != null) clearText.gameObject.SetActive(false);
        if (failText != null) failText.gameObject.SetActive(false);
    }
    public void PlayWavePanel(int currentWave)
    {
        if (wavePanel != null && !wavePanel.gameObject.activeSelf)
        {
            wavePanel.gameObject.SetActive(true);
        }

        panelGroup.DOKill();
        panelGroup.alpha = 1.0f;

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
            yield break;
        }
        prevSlot.SetActive(true);
        currentSlot.SetActive(true);
        nextSlot.SetActive(true);

        prevText.text = (wave - 2).ToString();
        currentText.text = (wave - 1).ToString();
        nextText.text = (wave).ToString();

        if(wave == 2) prevSlot.SetActive(false);


        waveContainer.anchoredPosition = originPos;

        yield return wavePanel.DOSizeDelta(new Vector2(expandSize, wavePanel.sizeDelta.y), expandTime)
            .SetEase(Ease.OutBack).SetUpdate(true).WaitForCompletion();

        yield return new WaitForSecondsRealtime(0.2f);

        Vector2 nextPos = originPos + Vector2.left * moveDistance;
        //yield return StartCoroutine(MoveContainer(nextPos));
        yield return waveContainer.DOAnchorPos(nextPos, moveTime)
            .SetEase(Ease.OutCubic).SetUpdate(true).WaitForCompletion();


        prevText.text = (wave - 1).ToString();
        prevSlot.SetActive(false);
        currentText.text = (wave).ToString();
        nextText.text = (wave + 1).ToString();
        waveContainer.anchoredPosition = originPos;
        prevSlot.SetActive(true);

        if(wave >= endWave) nextSlot.SetActive(false);
        else nextSlot.SetActive(true);


        yield return new WaitForSecondsRealtime(stayTime);

        SetNormalWave(wave);

        yield return wavePanel.DOSizeDelta(new Vector2(normalSize, wavePanel.sizeDelta.y), expandTime)
            .SetEase(Ease.OutBack).SetUpdate(true).WaitForCompletion();
    }

    public void SetNormalWave(int wave)
    {
        prevSlot.SetActive(false);
        nextSlot.SetActive(false);

        currentSlot.SetActive(true);
        currentText.text = wave.ToString();

        waveContainer.anchoredPosition = originPos;

        panelGroup.DOKill();

        panelGroup.DOFade(0, 0.5f).SetDelay(1f).SetUpdate(true);
    }

    public void PlayClearPanel()
    {
        if (wavePanel != null && !wavePanel.gameObject.activeSelf)
        {
            wavePanel.gameObject.SetActive(true);
        }

        panelGroup.DOKill();
        panelGroup.alpha = 1.0f;

        if (waveCoroutine != null)
        {
            StopCoroutine(waveCoroutine);
        }

        waveCoroutine = StartCoroutine(ResultCoroutine(true));
    }

    private IEnumerator ResultCoroutine(bool isClear)
    {
        prevSlot.SetActive(false);
        currentSlot.SetActive(false);
        nextSlot.SetActive(false);

        yield return wavePanel.DOSizeDelta(new Vector2(expandSize, wavePanel.sizeDelta.y), expandTime)
            .SetEase(Ease.OutBack).SetUpdate(true).WaitForCompletion();

        clearText.gameObject.SetActive(isClear);
        failText.gameObject.SetActive(!isClear);
    }
}
