using UnityEngine;

public class PlayerAttackAnimator : MonoBehaviour
{
    private const int BaseLayerIndex = 0;
    private const string BaseLayerName = "Base Layer";
    private const string AttackReadyStateName = "Attack Ready";
    private const string AttackHitStateName = "Attack Hit";

    [Header("참조")]
    [SerializeField] private SlashDashController dashController;
    [SerializeField] private AutoSlashController autoSlash;
    [SerializeField] private Animator animator;
    [SerializeField] private ChainCombatController chainCombat;
    [SerializeField] private PlayerMoveController moveController;

    [Header("파라미터")]
    [SerializeField] private string readyTrigger = "Attack";
    [SerializeField] private string slashTrigger = "Slash";

    [Header("공격 모션")]
    [SerializeField] private string[] slashStateNames = { AttackHitStateName };

    [Header("타격 VFX")]
    [SerializeField] private GameObject hitVfxPrefab;
    [SerializeField] private Transform hitVfxSpawnPoint;
    [SerializeField] private Vector3 hitVfxOffset = new Vector3(0f, 0.6f, 0.8f);
    [SerializeField] private bool hitVfxFollowSpawnPoint;
    [SerializeField, Min(0f)] private float hitVfxAutoDestroyTime = 2f;

    private bool isAutoSlashSubscribed;
    private bool isImpactSubscribed;
    private bool openingSlashPending;
    private bool slashFacingLockApplied;
    private bool slashStateObserved;
    private int slashEnterGraceFrames;
    private int attackReadyFullPathHash;
    private int attackHitHash;
    private int attackHitFullPathHash;
    private int activeSlashShortHash;
    private int activeSlashFullPathHash;
    private int[] slashStateShortHashes = System.Array.Empty<int>();
    private int[] slashStateFullPathHashes = System.Array.Empty<int>();
    private Vector3 slashFacingDirection = Vector3.forward;

    private void Awake()
    {
        attackReadyFullPathHash = Animator.StringToHash(BuildStatePath(AttackReadyStateName));
        attackHitHash = Animator.StringToHash(AttackHitStateName);
        attackHitFullPathHash = Animator.StringToHash(BuildStatePath(AttackHitStateName));
        CacheSlashStateHashes();
        ResolveReferences();
    }

    private void OnEnable()
    {
        CacheSlashStateHashes();
        ResolveReferences();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        openingSlashPending = false;
        ClearActiveSlashState();
        ReleaseSlashFacingLock();
    }

    private void OnValidate()
    {
        CacheSlashStateHashes();
    }

    private void Update()
    {
        UpdateSlashFacingLock();
    }

    private void HandleAttackReady()
    {
        if (animator == null) ResolveReferences();
        if (animator == null) return;
        if (dashController != null && dashController.IsDashing) return;

        ClearActiveSlashState();
        ReleaseSlashFacingLock();
        openingSlashPending = true;
        PlayState(attackReadyFullPathHash);
    }

    private void HandleDashImpact(Transform hitTarget)
    {
        if (animator == null) ResolveReferences();
        if (animator == null) return;

        if (TrySelectSlashForImpact(out var shortHash, out var fullPathHash))
        {
            AcquireSlashFacingLock(hitTarget);
            SetActiveSlash(shortHash, fullPathHash);
            PlayState(fullPathHash);
        }

        PlayHitVfx(hitTarget);
    }

    private void ResolveReferences()
    {
        if (dashController == null) dashController = GetComponent<SlashDashController>();
        if (dashController == null) dashController = GetComponentInParent<SlashDashController>();
        if (dashController == null) dashController = FindObjectOfType<SlashDashController>();

        if (autoSlash == null) autoSlash = GetComponent<AutoSlashController>();
        if (autoSlash == null) autoSlash = GetComponentInParent<AutoSlashController>();
        if (autoSlash == null) autoSlash = FindObjectOfType<AutoSlashController>();

        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>(true);
        if (animator == null) animator = GetComponentInParent<Animator>();

        if (chainCombat == null) chainCombat = GetComponent<ChainCombatController>();
        if (chainCombat == null) chainCombat = GetComponentInParent<ChainCombatController>();
        if (chainCombat == null) chainCombat = FindObjectOfType<ChainCombatController>();

        if (moveController == null) moveController = GetComponent<PlayerMoveController>();
        if (moveController == null) moveController = GetComponentInParent<PlayerMoveController>();
        if (moveController == null) moveController = FindObjectOfType<PlayerMoveController>();
    }

    private void ResetAttackTriggers()
    {
        if (animator == null) return;

        if (!string.IsNullOrEmpty(readyTrigger))
        {
            animator.ResetTrigger(readyTrigger);
        }

        if (!string.IsNullOrEmpty(slashTrigger))
        {
            animator.ResetTrigger(slashTrigger);
        }

        if (slashStateNames == null || slashStateNames.Length == 0) return;

        for (int i = 0; i < slashStateNames.Length; i++)
        {
            var stateName = slashStateNames[i];
            if (string.IsNullOrWhiteSpace(stateName)) continue;
            if (string.Equals(stateName, AttackHitStateName)) continue;

            animator.ResetTrigger(stateName);
        }
    }

    private void PlayState(int fullPathHash)
    {
        if (animator == null) return;
        if (fullPathHash == 0) return;
        if (!animator.HasState(BaseLayerIndex, fullPathHash)) return;

        ResetAttackTriggers();
        animator.Play(fullPathHash, BaseLayerIndex, 0f);
    }

    private bool TrySelectSlashForImpact(out int shortHash, out int fullPathHash)
    {
        shortHash = 0;
        fullPathHash = 0;

        if (animator == null) return false;

        if (openingSlashPending)
        {
            openingSlashPending = false;
            if (!animator.HasState(BaseLayerIndex, attackHitFullPathHash)) return false;

            shortHash = attackHitHash;
            fullPathHash = attackHitFullPathHash;
            return true;
        }

        CacheSlashStateHashes();

        var candidateIndices = new System.Collections.Generic.List<int>();
        for (int i = 0; i < slashStateNames.Length; i++)
        {
            var stateFullPathHash = slashStateFullPathHashes[i];
            if (stateFullPathHash == 0) continue;
            if (!animator.HasState(BaseLayerIndex, stateFullPathHash)) continue;

            candidateIndices.Add(i);
        }

        if (candidateIndices.Count == 0) return false;

        var selectedIndex = candidateIndices[Random.Range(0, candidateIndices.Count)];

        shortHash = slashStateShortHashes[selectedIndex];
        fullPathHash = slashStateFullPathHashes[selectedIndex];
        return true;
    }

    private void SetActiveSlash(int shortHash, int fullPathHash)
    {
        activeSlashShortHash = shortHash;
        activeSlashFullPathHash = fullPathHash;
        slashStateObserved = false;
        slashEnterGraceFrames = 3;
    }

    private void ClearActiveSlashState()
    {
        activeSlashShortHash = 0;
        activeSlashFullPathHash = 0;
        slashStateObserved = false;
        slashEnterGraceFrames = 0;
    }

    private void AcquireSlashFacingLock(Transform hitTarget)
    {
        var facing = ResolveSlashFacingDirection(hitTarget);
        facing.y = 0f;
        if (facing.sqrMagnitude <= 0f) return;

        slashFacingDirection = facing.normalized;
        if (moveController != null)
        {
            moveController.SetSlashFacingDirection(slashFacingDirection);
        }

        if (slashFacingLockApplied || moveController == null) return;

        slashFacingLockApplied = true;
        moveController.AddRotationLock();
    }

    private void UpdateSlashFacingLock()
    {
        if (!slashFacingLockApplied) return;

        if (IsStateActive(activeSlashShortHash, activeSlashFullPathHash))
        {
            slashStateObserved = true;
            return;
        }

        if (!slashStateObserved && slashEnterGraceFrames > 0)
        {
            slashEnterGraceFrames--;
            return;
        }

        if (chainCombat != null && chainCombat.IsSlowActive)
        {
            return;
        }

        ClearActiveSlashState();
        ReleaseSlashFacingLock();
    }

    private void ReleaseSlashFacingLock()
    {
        if (!slashFacingLockApplied) return;

        slashFacingLockApplied = false;
        if (moveController != null)
        {
            moveController.ClearSlashFacingDirection();
            moveController.RemoveRotationLock();
        }
    }

    private Vector3 ResolveSlashFacingDirection(Transform hitTarget)
    {
        if (hitTarget != null)
        {
            var towardTarget = hitTarget.position - transform.position;
            towardTarget.y = 0f;
            if (towardTarget.sqrMagnitude > 0f)
            {
                return towardTarget.normalized;
            }
        }

        if (dashController != null)
        {
            var dashFacing = dashController.DashDirection;
            dashFacing.y = 0f;
            if (dashFacing.sqrMagnitude > 0f)
            {
                return dashFacing.normalized;
            }
        }

        return transform.forward;
    }

    private void CacheSlashStateHashes()
    {
        if (slashStateNames == null || slashStateNames.Length == 0)
        {
            slashStateShortHashes = System.Array.Empty<int>();
            slashStateFullPathHashes = System.Array.Empty<int>();
            return;
        }

        slashStateShortHashes = new int[slashStateNames.Length];
        slashStateFullPathHashes = new int[slashStateNames.Length];

        for (int i = 0; i < slashStateNames.Length; i++)
        {
            var stateName = slashStateNames[i];
            if (string.IsNullOrWhiteSpace(stateName)) continue;

            slashStateShortHashes[i] = Animator.StringToHash(stateName);
            slashStateFullPathHashes[i] = Animator.StringToHash(BuildStatePath(stateName));
        }
    }

    private static string BuildStatePath(string stateName)
    {
        return $"{BaseLayerName}.{stateName}";
    }

    private void PlayHitVfx(Transform hitTarget)
    {
        if (hitVfxPrefab == null) return;

        var spawnPoint = hitVfxSpawnPoint != null ? hitVfxSpawnPoint : transform;
        var useTargetAnchor = hitTarget != null;
        var anchor = useTargetAnchor ? hitTarget : spawnPoint;
        var position = anchor.position + anchor.TransformDirection(hitVfxOffset);

        Quaternion rotation;
        if (useTargetAnchor)
        {
            var look = transform.position - anchor.position;
            look.y = 0f;
            rotation = look.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(look.normalized, Vector3.up)
                : anchor.rotation;
        }
        else
        {
            rotation = spawnPoint.rotation;
        }

        var instance = Instantiate(hitVfxPrefab, position, rotation);
        if (instance == null) return;

        ConfigureHitVfxTiming(instance);

        if (hitVfxFollowSpawnPoint)
        {
            instance.transform.SetParent(anchor, true);
        }

        if (!hitVfxFollowSpawnPoint && hitVfxAutoDestroyTime > 0f)
        {
            StartCoroutine(DestroyHitVfxAfterDelay(instance, hitVfxAutoDestroyTime));
        }
    }

    private void ConfigureHitVfxTiming(GameObject instance)
    {
        if (instance == null) return;
        if (chainCombat == null || !chainCombat.IsSlowActive) return;

        var particleSystems = instance.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < particleSystems.Length; i++)
        {
            var particleSystem = particleSystems[i];
            if (particleSystem == null) continue;

            var main = particleSystem.main;
            main.useUnscaledTime = true;
        }

        var animators = instance.GetComponentsInChildren<Animator>(true);
        for (int i = 0; i < animators.Length; i++)
        {
            var effectAnimator = animators[i];
            if (effectAnimator == null) continue;
            effectAnimator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }

    private System.Collections.IEnumerator DestroyHitVfxAfterDelay(GameObject instance, float delay)
    {
        if (instance == null) yield break;
        if (delay <= 0f)
        {
            Destroy(instance);
            yield break;
        }

        yield return new WaitForSecondsRealtime(delay);
        if (instance != null)
        {
            Destroy(instance);
        }
    }

    private bool IsStateActive(int shortNameHash, int fullPathHash)
    {
        if (animator == null) return false;
        if (shortNameHash == 0 && fullPathHash == 0) return false;

        var current = animator.GetCurrentAnimatorStateInfo(BaseLayerIndex);
        if (current.shortNameHash == shortNameHash || current.fullPathHash == fullPathHash)
        {
            return true;
        }

        if (!animator.IsInTransition(BaseLayerIndex)) return false;

        var next = animator.GetNextAnimatorStateInfo(BaseLayerIndex);
        return next.shortNameHash == shortNameHash || next.fullPathHash == fullPathHash;
    }

    private void SubscribeEvents()
    {
        if (!isAutoSlashSubscribed && autoSlash != null)
        {
            autoSlash.OnAttackReady += HandleAttackReady;
            isAutoSlashSubscribed = true;
        }

        if (!isImpactSubscribed && dashController != null)
        {
            dashController.OnDashImpactTarget += HandleDashImpact;
            isImpactSubscribed = true;
        }
    }

    private void UnsubscribeEvents()
    {
        if (isAutoSlashSubscribed && autoSlash != null)
        {
            autoSlash.OnAttackReady -= HandleAttackReady;
        }

        if (isImpactSubscribed && dashController != null)
        {
            dashController.OnDashImpactTarget -= HandleDashImpact;
        }

        isAutoSlashSubscribed = false;
        isImpactSubscribed = false;
    }
}
