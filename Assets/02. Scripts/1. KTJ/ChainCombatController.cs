using System.Collections;
using UnityEngine;

public class ChainCombatController : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private DamageSystem damageSystem;
    [SerializeField] private PlayerMoveController moveController;
    [SerializeField] private TargetingSystem targetingSystem;

    [Header("체인 종료")]
    [SerializeField] private bool endChainWhenNoTargets = true;

    [Header("체인")]
    [SerializeField, Min(0f)] private float damageIncreaseRate = 0.1f;

    [Header("체인 마일스톤")]
    [SerializeField] private bool useChainMilestoneBeat = true;
    [SerializeField, Min(1)] private int firstMilestoneChain = 3;
    [SerializeField, Min(1)] private int secondMilestoneChain = 5;
    [SerializeField, Min(1)] private int thirdMilestoneChain = 7;

    [Header("슬로우")]
    [SerializeField, Range(0f, 1f)] private float slowTimeScale = 0.1f;
    [SerializeField, Min(0f)] private float firstSlowDuration = 1f;
    [SerializeField, Min(0f)] private float chainSlowDuration = 0.8f;
    [SerializeField] private bool lockMovementDuringSlow = true;

    private Transform lastTarget;
    private int currentChain;
    private bool isSlowActive;
    private float pendingTimeScale = -1f;
    private Coroutine slowRoutine;
    private Coroutine resumeRoutine;
    private bool movementLockApplied;
    private Coroutine targetCheckRoutine;
    private float slowEndTimeRealtime;
    private float slowDurationRealtime;
    private int lastTriggeredMilestoneChain;

    public int CurrentChain => currentChain;
    public bool IsSlowActive => isSlowActive;
    public Transform LastTarget => lastTarget;
    public float SlowRemainingTime => !isSlowActive ? 0f : Mathf.Max(0f, slowEndTimeRealtime - Time.unscaledTime);
    public float SlowRemainingNormalized => !isSlowActive || slowDurationRealtime <= 0f
        ? 0f
        : Mathf.Clamp01(SlowRemainingTime / slowDurationRealtime);
    public event System.Action<bool> OnSlowStateChanged;
    public event System.Action<int> OnChainMilestoneReached;

    private void Awake()
    {
        EnsureDamageSystem();
        EnsureMoveController();
        EnsureTargetingSystem();
    }

    private void OnEnable()
    {
        EnsureDamageSystem();
        if (damageSystem != null)
        {
            damageSystem.OnDamageApplied += HandleDamageApplied;
        }
    }

    private void OnDisable()
    {
        if (damageSystem != null)
        {
            damageSystem.OnDamageApplied -= HandleDamageApplied;
        }

        if (targetCheckRoutine != null)
        {
            StopCoroutine(targetCheckRoutine);
            targetCheckRoutine = null;
        }

        CancelSlow();
    }

    public float GetDamageMultiplier(Transform target)
    {
        if (target == null) return 1f;

        var nextChain = target != lastTarget ? currentChain + 1 : currentChain;
        if (nextChain <= 0) nextChain = 1;

        return 1f + damageIncreaseRate * (nextChain - 1);
    }

    public void CancelSlow()
    {
        if (!isSlowActive) return;
        StopSlow();
    }

    private void HandleDamageApplied(DamageSystem.DamageResult result)
    {
        if (result.Target == null) return;

        var previousChain = currentChain;
        var isNewTarget = result.Target != lastTarget;
        if (isNewTarget || currentChain <= 0)
        {
            currentChain = Mathf.Max(1, currentChain + 1);
        }

        if (currentChain != previousChain)
        {
            TryNotifyChainMilestone(currentChain);
        }

        lastTarget = result.Target;

        var duration = currentChain <= 1 ? firstSlowDuration : chainSlowDuration;
        StartSlow(duration);

        if (result.IsDead)
        {
            RequestTargetCheck();
        }
    }

    private void StartSlow(float duration)
    {
        if (duration <= 0f || slowTimeScale <= 0f) return;

        slowDurationRealtime = Mathf.Max(0.0001f, duration);
        slowEndTimeRealtime = Time.unscaledTime + slowDurationRealtime;

        if (slowRoutine != null)
        {
            StopCoroutine(slowRoutine);
        }

        slowRoutine = StartCoroutine(SlowRoutine(duration));
    }

    private IEnumerator SlowRoutine(float duration)
    {
        var wasSlowActive = isSlowActive;
        isSlowActive = true;
        if (!wasSlowActive)
        {
            ApplyMovementLock(true);
            OnSlowStateChanged?.Invoke(true);
        }
        SetTimeScale(slowTimeScale);
        yield return new WaitForSecondsRealtime(duration);
        StopSlow();
    }

    private void StopSlow()
    {
        if (!isSlowActive) return;
        isSlowActive = false;
        ApplyMovementLock(false);
        OnSlowStateChanged?.Invoke(false);
        if (slowRoutine != null)
        {
            StopCoroutine(slowRoutine);
            slowRoutine = null;
        }

        SetTimeScale(1f);
        ResetSlowTimerState();
        ResetChainState();
    }

    private void RequestTargetCheck()
    {
        if (!endChainWhenNoTargets) return;
        if (targetCheckRoutine != null)
        {
            StopCoroutine(targetCheckRoutine);
        }

        targetCheckRoutine = StartCoroutine(CheckTargetsNextFrame());
    }

    private IEnumerator CheckTargetsNextFrame()
    {
        yield return null;
        targetCheckRoutine = null;
        if (!isSlowActive) yield break;
        if (!endChainWhenNoTargets) yield break;
        EnsureTargetingSystem();
        if (targetingSystem == null) yield break;
        if (targetingSystem.GetActiveTargetCount() <= 0)
        {
            StopSlow();
        }
    }

    private void ApplyMovementLock(bool locked)
    {
        if (!lockMovementDuringSlow) return;
        EnsureMoveController();
        if (moveController == null) return;
        if (locked)
        {
            if (movementLockApplied) return;
            movementLockApplied = true;
            moveController.AddMovementLock();
        }
        else
        {
            if (!movementLockApplied) return;
            movementLockApplied = false;
            moveController.RemoveMovementLock();
        }
    }

    private void EnsureMoveController()
    {
        if (moveController != null) return;
        moveController = GetComponent<PlayerMoveController>();
        if (moveController == null) moveController = GetComponentInParent<PlayerMoveController>();
        if (moveController == null) moveController = FindObjectOfType<PlayerMoveController>();
    }

    private void EnsureDamageSystem()
    {
        if (damageSystem != null) return;
        damageSystem = GetComponent<DamageSystem>();
        if (damageSystem == null) damageSystem = GetComponentInParent<DamageSystem>();
        if (damageSystem == null) damageSystem = FindObjectOfType<DamageSystem>();
    }

    private void EnsureTargetingSystem()
    {
        if (targetingSystem != null) return;
        targetingSystem = GetComponent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = GetComponentInParent<TargetingSystem>();
        if (targetingSystem == null) targetingSystem = FindObjectOfType<TargetingSystem>();
    }

    public void BindSceneRefs(
        DamageSystem externalDamageSystem,
        PlayerMoveController externalMoveController,
        TargetingSystem externalTargetingSystem = null)
    {
        if (externalDamageSystem != null && damageSystem != externalDamageSystem)
        {
            if (isActiveAndEnabled && damageSystem != null)
            {
                damageSystem.OnDamageApplied -= HandleDamageApplied;
            }

            damageSystem = externalDamageSystem;
            if (isActiveAndEnabled)
            {
                damageSystem.OnDamageApplied += HandleDamageApplied;
            }
        }

        if (externalMoveController != null)
        {
            moveController = externalMoveController;
        }

        if (externalTargetingSystem != null)
        {
            targetingSystem = externalTargetingSystem;
        }
    }

    private void SetTimeScale(float value)
    {
        if (Mathf.Approximately(Time.timeScale, 0f))
        {
            pendingTimeScale = value;
            if (resumeRoutine == null)
            {
                resumeRoutine = StartCoroutine(WaitForTimeScaleResume());
            }
            return;
        }

        Time.timeScale = value;
        pendingTimeScale = -1f;
    }

    private void ResetChainState()
    {
        currentChain = 0;
        lastTarget = null;
        lastTriggeredMilestoneChain = 0;
    }

    private IEnumerator WaitForTimeScaleResume()
    {
        while (Mathf.Approximately(Time.timeScale, 0f))
        {
            yield return null;
        }

        if (pendingTimeScale >= 0f)
        {
            Time.timeScale = pendingTimeScale;
            pendingTimeScale = -1f;
        }

        resumeRoutine = null;
    }

    private void ResetSlowTimerState()
    {
        slowEndTimeRealtime = 0f;
        slowDurationRealtime = 0f;
    }

    private void TryNotifyChainMilestone(int chain)
    {
        if (!useChainMilestoneBeat) return;
        if (chain <= 0) return;
        if (chain == lastTriggeredMilestoneChain) return;
        if (!IsMilestoneChain(chain)) return;

        lastTriggeredMilestoneChain = chain;
        OnChainMilestoneReached?.Invoke(chain);
    }

    private bool IsMilestoneChain(int chain)
    {
        var first = Mathf.Max(1, firstMilestoneChain);
        var second = Mathf.Max(1, secondMilestoneChain);
        var third = Mathf.Max(1, thirdMilestoneChain);
        return chain == first || chain == second || chain == third;
    }
}
