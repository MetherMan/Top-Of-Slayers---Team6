using TMPro;
using UnityEngine;

public class WaveUI : MonoBehaviour
{
    [SerializeField] private WaveUIAnim UIAnim;
    [SerializeField] private StageConfigSO stageSO;

    [SerializeField] private TextMeshProUGUI clearText;

    private WaveDirectorSystem boundWaveDirector;
    private bool hasWaveStarted;

    private void Awake()
    {
        if (UIAnim == null)
        {
            UIAnim = GetComponent<WaveUIAnim>();
        }

        if (stageSO == null && StageManager.Instance != null)
        {
            stageSO = StageManager.Instance.selectDB;
        }

        if (clearText != null)
        {
            clearText.gameObject.SetActive(false);
        }

        int endWave = stageSO != null && stageSO.roundDatas != null
            ? stageSO.roundDatas.Count
            : 0;
        UIAnim.Init(endWave);
    }

    private void OnEnable()
    {
        hasWaveStarted = false;
        UIAnim?.ResetPanelState();
        StageFlowManager.OnStageFinished += HandleStageFinished;
        TryBindWaveDirector();
    }

    private void OnDisable()
    {
        StageFlowManager.OnStageFinished -= HandleStageFinished;
        UnbindWaveDirector();
    }

    private void Update()
    {
        TryBindWaveDirector();
    }

    private void TryBindWaveDirector()
    {
        WaveDirectorSystem currentWaveDirector = WaveDirectorSystem.Instance;
        if (currentWaveDirector == boundWaveDirector)
        {
            return;
        }

        UnbindWaveDirector();
        if (currentWaveDirector == null)
        {
            return;
        }

        currentWaveDirector.OnWaveClear += ShowWaveText;
        boundWaveDirector = currentWaveDirector;
    }

    private void UnbindWaveDirector()
    {
        if (boundWaveDirector == null)
        {
            return;
        }

        boundWaveDirector.OnWaveClear -= ShowWaveText;
        boundWaveDirector = null;
    }

    private void ShowWaveText(int wave)
    {
        hasWaveStarted = true;
        UIAnim.PlayWavePanel(wave);
    }

    private void HandleStageFinished(StageResultData result)
    {
        if (!result.IsClear || !hasWaveStarted)
        {
            return;
        }

        UIAnim.PlayClearPanel();
    }
}
