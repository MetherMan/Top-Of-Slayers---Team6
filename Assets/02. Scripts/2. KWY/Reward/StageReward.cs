using System.Collections.Generic;
using UnityEngine;

public class StageReward : MonoBehaviour
{
    [System.Serializable]
    private class StageRewardCanvasProfile
    {
        public StageConfigSO stage;
        public GameObject rewardCanvasPrefab;
    }

    [Header("풀클리어 보상")]
    [SerializeField] public DropTable dropTable;
    [SerializeField] public int firstCleargold = 100;
    [SerializeField] public int repeatGold = 20;

    [Header("직접 연결")]
    [SerializeField] private RewardSystem rewardSystem;
    [SerializeField] private RewardUI rewardUI;

    [Header("런타임 스테이지 보상")]
    [SerializeField] private List<StageRewardCanvasProfile> stageRewardCanvases = new List<StageRewardCanvasProfile>();
    [SerializeField] private int partialWaveGoldPerWave = 10;
    [SerializeField] private bool pauseGameOnResult = true;

    private GameObject runtimeRewardCanvasInstance;
    private RewardSystem runtimeRewardSystem;
    private RewardUI runtimeRewardUI;
    private StageReward runtimeRewardSource;
    private StageReward sceneRewardSource;
    private RewardSystem sceneRewardSystem;
    private RewardUI sceneRewardUI;

    private void OnEnable()
    {
        if (!ShouldListenStageFlowEvents())
        {
            return;
        }

        ClearResolvedTargets();
        ResetAllRewardPresentations();
        ResetRewardPresentation();
        StageFlowManager.OnStageClear += StageClear;
        StageFlowManager.OnStageFinished += HandleStageFinished;
    }

    private void OnDisable()
    {
        if (!ShouldListenStageFlowEvents())
        {
            return;
        }

        StageFlowManager.OnStageClear -= StageClear;
        StageFlowManager.OnStageFinished -= HandleStageFinished;
        ResetAllRewardPresentations();
        DisposeRuntimeRewardCanvas();
        ClearResolvedTargets();
    }

    private void HandleStageFinished(StageResultData result)
    {
        if (result.IsClear)
        {
            return;
        }

        if (!TryResolveRuntimeTargets(out StageReward rewardSource, out RewardSystem targetRewardSystem, out RewardUI targetRewardUI))
        {
            return;
        }

        List<RewardData> rewards = rewardSource.BuildPartialRewards(result.LastClearedWave);

        if (pauseGameOnResult && targetRewardUI != null)
        {
            Time.timeScale = 0f;
        }

        PresentRewards(rewards, targetRewardSystem, targetRewardUI);
    }

    public void StageClear(bool wasAlreadyCleared)
    {
        if (!TryResolveRuntimeTargets(out StageReward rewardSource, out RewardSystem targetRewardSystem, out RewardUI targetRewardUI))
        {
            return;
        }

        List<RewardData> rewards = rewardSource.BuildFullClearRewards(wasAlreadyCleared);

        if (pauseGameOnResult && targetRewardUI != null)
        {
            Time.timeScale = 0f;
        }

        PresentRewards(rewards, targetRewardSystem, targetRewardUI);
    }

    public void TestFirstClear()
    {
        StageClear(false);
    }

    public void TestRepeatClear()
    {
        StageClear(true);
    }

    public void OnClickClaim()
    {
        ResetAllRewardPresentations();
        Time.timeScale = 1f;
        LoadingSceneController.LoadScene("1.LobbyModify");
    }

    private List<RewardData> BuildFullClearRewards(bool wasAlreadyCleared)
    {
        List<RewardData> rewards = new List<RewardData>();

        if (!wasAlreadyCleared)
        {
            if (dropTable != null)
            {
                rewards.AddRange(DropSystem.Calculate(dropTable));
            }

            if (firstCleargold > 0)
            {
                rewards.Add(new RewardData { gold = firstCleargold });
            }

            return rewards;
        }

        if (repeatGold > 0)
        {
            rewards.Add(new RewardData { gold = repeatGold });
        }

        return rewards;
    }

    private List<RewardData> BuildPartialRewards(int lastClearedWave)
    {
        List<RewardData> rewards = new List<RewardData>();
        int rewardedWaveCount = Mathf.Max(1, lastClearedWave);
        int gold = Mathf.Max(0, partialWaveGoldPerWave) * rewardedWaveCount;

        if (gold > 0)
        {
            rewards.Add(new RewardData { gold = gold });
        }

        return rewards;
    }

    private void PresentRewards(List<RewardData> rewards, RewardSystem targetRewardSystem, RewardUI targetRewardUI)
    {
        List<RewardData> resolvedRewards = rewards ?? new List<RewardData>();

        if (targetRewardSystem != null && resolvedRewards.Count > 0)
        {
            targetRewardSystem.GiveRewards(resolvedRewards);
        }

        if (targetRewardUI != null)
        {
            ActivateRewardCanvas(targetRewardUI);
            targetRewardUI.ShowReward(resolvedRewards);
        }
    }

    private bool TryResolveRuntimeTargets(out StageReward rewardSource, out RewardSystem targetRewardSystem, out RewardUI targetRewardUI)
    {
        rewardSource = this;
        targetRewardSystem = rewardSystem;
        targetRewardUI = rewardUI;

        StageConfigSO currentStage = StageManager.Instance != null ? StageManager.Instance.selectDB : null;
        if (currentStage == null)
        {
            return targetRewardUI != null || targetRewardSystem != null;
        }

        if (TryResolveSceneTargets(out StageReward resolvedSceneRewardSource, out RewardSystem resolvedSceneRewardSystem, out RewardUI resolvedSceneRewardUI))
        {
            rewardSource = resolvedSceneRewardSource != null ? resolvedSceneRewardSource : rewardSource;
            targetRewardSystem = resolvedSceneRewardSystem != null ? resolvedSceneRewardSystem : targetRewardSystem;
            targetRewardUI = resolvedSceneRewardUI != null ? resolvedSceneRewardUI : targetRewardUI;
            return targetRewardUI != null || targetRewardSystem != null;
        }

        if (runtimeRewardCanvasInstance == null)
        {
            GameObject rewardCanvasPrefab = GetRewardCanvasPrefab(currentStage);
            if (rewardCanvasPrefab == null)
            {
                return targetRewardUI != null || targetRewardSystem != null;
            }

            runtimeRewardCanvasInstance = Instantiate(rewardCanvasPrefab);
            runtimeRewardCanvasInstance.name = rewardCanvasPrefab.name;

            if (runtimeRewardCanvasInstance.transform is RectTransform canvasTransform)
            {
                canvasTransform.localScale = Vector3.one;
                canvasTransform.anchoredPosition3D = Vector3.zero;
            }
        }

        if (runtimeRewardSource == null)
        {
            runtimeRewardSource = runtimeRewardCanvasInstance.GetComponentInChildren<StageReward>(true);
        }

        if (runtimeRewardSystem == null)
        {
            runtimeRewardSystem = runtimeRewardCanvasInstance.GetComponentInChildren<RewardSystem>(true);
        }

        if (runtimeRewardUI == null)
        {
            runtimeRewardUI = runtimeRewardCanvasInstance.GetComponentInChildren<RewardUI>(true);
        }

        if (runtimeRewardSource != null)
        {
            rewardSource = runtimeRewardSource;
        }

        if (runtimeRewardSystem != null)
        {
            targetRewardSystem = runtimeRewardSystem;
        }

        if (runtimeRewardUI != null)
        {
            targetRewardUI = runtimeRewardUI;
        }

        return targetRewardUI != null || targetRewardSystem != null;
    }

    private bool TryResolveSceneTargets(out StageReward rewardSource, out RewardSystem targetRewardSystem, out RewardUI targetRewardUI)
    {
        rewardSource = sceneRewardSource;
        targetRewardSystem = sceneRewardSystem;
        targetRewardUI = sceneRewardUI;

        if (sceneRewardSource == null || sceneRewardSystem == null || sceneRewardUI == null)
        {
            sceneRewardSource = null;
            sceneRewardSystem = null;
            sceneRewardUI = null;

            StageReward[] candidates = FindObjectsByType<StageReward>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < candidates.Length; i++)
            {
                StageReward candidate = candidates[i];
                if (candidate == null || candidate == this) continue;
                if (candidate.gameObject.scene != gameObject.scene) continue;

                Canvas candidateCanvas = candidate.GetComponentInParent<Canvas>(true);
                if (candidateCanvas == null) continue;

                RewardSystem candidateRewardSystem = candidate.rewardSystem;
                RewardUI candidateRewardUI = candidate.rewardUI;

                if (candidateRewardSystem == null)
                {
                    candidateRewardSystem = candidateCanvas.GetComponentInChildren<RewardSystem>(true);
                }

                if (candidateRewardUI == null)
                {
                    candidateRewardUI = candidateCanvas.GetComponentInChildren<RewardUI>(true);
                }

                if (candidateRewardSystem == null && candidateRewardUI == null) continue;

                sceneRewardSource = candidate;
                sceneRewardSystem = candidateRewardSystem;
                sceneRewardUI = candidateRewardUI;
                break;
            }
        }

        rewardSource = sceneRewardSource;
        targetRewardSystem = sceneRewardSystem;
        targetRewardUI = sceneRewardUI;
        return rewardSource != null || targetRewardSystem != null || targetRewardUI != null;
    }

    private void ActivateRewardCanvas(RewardUI targetRewardUI)
    {
        Canvas rewardCanvas = targetRewardUI.GetComponentInParent<Canvas>(true);
        GameObject rewardRoot = rewardCanvas != null ? rewardCanvas.gameObject : targetRewardUI.transform.root.gameObject;

        if (rewardRoot == null)
        {
            return;
        }

        if (!rewardRoot.activeSelf)
        {
            rewardRoot.SetActive(true);
        }

        if (rewardRoot.transform is RectTransform canvasTransform)
        {
            canvasTransform.localScale = Vector3.one;
            canvasTransform.anchoredPosition3D = Vector3.zero;
        }
    }

    private GameObject GetRewardCanvasPrefab(StageConfigSO currentStage)
    {
        for (int i = 0; i < stageRewardCanvases.Count; i++)
        {
            StageRewardCanvasProfile profile = stageRewardCanvases[i];
            if (profile == null) continue;
            if (profile.stage != currentStage) continue;
            return profile.rewardCanvasPrefab;
        }

        return null;
    }

    private void ResetRewardPresentation()
    {
        if (TryResolveSceneTargets(out _, out _, out RewardUI targetRewardUI))
        {
            HideRewardCanvas(targetRewardUI);
        }

        if (runtimeRewardCanvasInstance != null)
        {
            HideRewardCanvas(runtimeRewardUI);
            runtimeRewardCanvasInstance.SetActive(false);
        }
    }

    private void ClearResolvedTargets()
    {
        sceneRewardSource = null;
        sceneRewardSystem = null;
        sceneRewardUI = null;
        runtimeRewardSource = null;
        runtimeRewardSystem = null;
        runtimeRewardUI = null;
    }

    private void DisposeRuntimeRewardCanvas()
    {
        if (runtimeRewardCanvasInstance == null)
        {
            return;
        }

        Destroy(runtimeRewardCanvasInstance);
        runtimeRewardCanvasInstance = null;
    }

    private bool ShouldListenStageFlowEvents()
    {
        return GetComponentInParent<Canvas>(true) == null;
    }

    private void ResetAllRewardPresentations()
    {
        RewardUI[] rewardUIs = FindObjectsByType<RewardUI>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        for (int i = 0; i < rewardUIs.Length; i++)
        {
            HideRewardCanvas(rewardUIs[i]);
        }
    }

    private static void HideRewardCanvas(RewardUI targetRewardUI)
    {
        if (targetRewardUI == null)
        {
            return;
        }

        targetRewardUI.ResetPresentation();

        Canvas rewardCanvas = targetRewardUI.GetComponentInParent<Canvas>(true);
        GameObject rewardRoot = rewardCanvas != null ? rewardCanvas.gameObject : targetRewardUI.transform.root.gameObject;
        if (rewardRoot != null && rewardRoot.activeSelf)
        {
            rewardRoot.SetActive(false);
        }
    }
}
