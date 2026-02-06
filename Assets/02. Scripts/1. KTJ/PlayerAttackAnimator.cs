using UnityEngine;

public class PlayerAttackAnimator : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private SlashDashController dashController;
    [SerializeField] private AutoSlashController autoSlash;
    [SerializeField] private Animator animator;

    [Header("파라미터")]
    [SerializeField] private string readyTrigger = "Attack";
    [SerializeField] private string slashTrigger = "Slash";

    private bool isAutoSlashSubscribed;
    private bool isImpactSubscribed;
    private int pendingReadyRetry;
    private int pendingSlashRetry;
    private int attackReadyHash;
    private int attackHitHash;

    private void Awake()
    {
        attackReadyHash = Animator.StringToHash("Attack Ready");
        attackHitHash = Animator.StringToHash("Attack Hit");
        ResolveReferences();
    }

    private void OnEnable()
    {
        ResolveReferences();
        SubscribeEvents();
    }

    private void OnDisable()
    {
        UnsubscribeEvents();
        pendingReadyRetry = 0;
        pendingSlashRetry = 0;
    }

    private void HandleAttackReady()
    {
        if (animator == null) ResolveReferences();
        if (animator == null) return;
        if (string.IsNullOrEmpty(readyTrigger)) return;
        if (dashController != null && dashController.IsDashing) return;
        pendingSlashRetry = 0;
        pendingReadyRetry = 3;
        FireReadyTrigger();
    }

    private void HandleDashStarted()
    {
        if (animator == null) ResolveReferences();
        if (animator == null) return;
        if (string.IsNullOrEmpty(slashTrigger)) return;
        pendingReadyRetry = 0;
        pendingSlashRetry = 3;
        FireSlashTrigger();
    }

    private void Update()
    {
        if (animator == null) return;

        if (pendingReadyRetry > 0)
        {
            if (IsStateActive(attackReadyHash))
            {
                pendingReadyRetry = 0;
            }
            else
            {
                FireReadyTrigger();
                pendingReadyRetry--;
            }
        }

        if (pendingSlashRetry > 0)
        {
            if (IsStateActive(attackHitHash))
            {
                pendingSlashRetry = 0;
            }
            else
            {
                FireSlashTrigger();
                pendingSlashRetry--;
            }
        }
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
    }

    private void FireReadyTrigger()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(readyTrigger)) return;
        if (!string.IsNullOrEmpty(slashTrigger)) animator.ResetTrigger(slashTrigger);
        animator.SetTrigger(readyTrigger);
    }

    private void FireSlashTrigger()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(slashTrigger)) return;
        if (!string.IsNullOrEmpty(readyTrigger)) animator.ResetTrigger(readyTrigger);
        animator.SetTrigger(slashTrigger);
    }

    private bool IsStateActive(int shortNameHash)
    {
        if (shortNameHash == 0) return false;

        var current = animator.GetCurrentAnimatorStateInfo(0);
        if (current.shortNameHash == shortNameHash) return true;
        if (!animator.IsInTransition(0)) return false;

        var next = animator.GetNextAnimatorStateInfo(0);
        return next.shortNameHash == shortNameHash;
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
            dashController.OnDashImpact += HandleDashStarted;
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
            dashController.OnDashImpact -= HandleDashStarted;
        }

        isAutoSlashSubscribed = false;
        isImpactSubscribed = false;
    }
}
