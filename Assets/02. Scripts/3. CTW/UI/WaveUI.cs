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
        UIAnim.PlayWavePanel(wave);
    }

    private void ShowClearText()
    {
        UIAnim.PlayClearPanel();
    }
}
