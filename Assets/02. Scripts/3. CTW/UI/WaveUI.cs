using TMPro;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private WaveUIAnim UIAnim;
    [SerializeField] private StageConfigSO stageSO;

    [SerializeField] private TextMeshProUGUI clearText;

    private void Awake()
    {
        if (UIAnim == null)
        {
            UIAnim = GetComponent<WaveUIAnim>();
        }
        if (clearText != null)
        {
            clearText.gameObject.SetActive(false);
        }
        int endWave = stageSO.roundDatas.Count;
        UIAnim.Init(endWave);
    }

    private void OnEnable()
    {
        if (WaveDirectorSystem.Instance != null)
        {
            WaveDirectorSystem.Instance.OnWaveClear += ShowWaveText;
            WaveDirectorSystem.Instance.OnRoundClear += ShowClearText;
        }
    }

    private void OnDisable()
    {
        if (WaveDirectorSystem.Instance != null)
        {
            WaveDirectorSystem.Instance.OnWaveClear -= ShowWaveText;
            WaveDirectorSystem.Instance.OnRoundClear -= ShowClearText;
        }
    }

    private void ShowWaveText(int wave)
    {
        Debug.Log("쇼 웨이브 패널");
        UIAnim.PlayWavePanel(wave);
    }

    private void ShowClearText()
    {
        Debug.Log("스테이지 클리어");
        clearText.gameObject.SetActive(true);
    }
}
