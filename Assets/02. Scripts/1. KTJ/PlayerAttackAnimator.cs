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
    [SerializeField] private string readyStateName = "Attack Ready";
    [SerializeField] private string hitStateName = "Attack Hit";
    [SerializeField, Min(0f)] private float forceTransitionTime = 0.02f;
    [SerializeField] private bool useDirectStatePlay = true;

    private bool isAutoSlashSubscribed;
    private bool isImpactSubscribed;
    private int pendingReadyRetry;
    private int pendingSlashRetry;
    private int attackReadyHash;
    private int attackHitHash;
    private int attackReadyFullPathHash;
    private int attackHitFullPathHash;

    private void Awake()
    {
        attackReadyHash = Animator.StringToHash(readyStateName);
        attackHitHash = Animator.StringToHash(hitStateName);
        attackReadyFullPathHash = Animator.StringToHash($"Base Layer.{readyStateName}");
        attackHitFullPathHash = Animator.StringToHash($"Base Layer.{hitStateName}");
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
        if (dashController != null && dashController.IsDashing) return;
        pendingSlashRetry = 0;
        pendingReadyRetry = 3;
        PlayReadyState();
    }

    private void HandleDashStarted()
    {
        if (animator == null) ResolveReferences();
        if (animator == null) return;
        pendingReadyRetry = 0;
        pendingSlashRetry = 3;
        PlayHitState();
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
                PlayReadyState();
                pendingReadyRetry--;
            }

            if (pendingReadyRetry == 0 && !IsStateActive(attackReadyHash))
            {
                ForceState(readyStateName);
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
                PlayHitState();
                pendingSlashRetry--;
            }

            if (pendingSlashRetry == 0 && !IsStateActive(attackHitHash))
            {
                ForceState(hitStateName);
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

    private void ForceState(string stateName)
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(stateName)) return;
        animator.CrossFadeInFixedTime(stateName, forceTransitionTime, 0, 0f);
    }

    private void PlayReadyState()
    {
        if (useDirectStatePlay)
        {
            if (attackReadyFullPathHash != 0)
            {
                animator.CrossFadeInFixedTime(attackReadyFullPathHash, forceTransitionTime, 0, 0f);
                return;
            }
            ForceState(readyStateName);
            return;
        }

        if (!string.IsNullOrEmpty(readyTrigger))
        {
            FireReadyTrigger();
        }
    }

    private void PlayHitState()
    {
        if (useDirectStatePlay)
        {
            if (attackHitFullPathHash != 0)
            {
                animator.CrossFadeInFixedTime(attackHitFullPathHash, forceTransitionTime, 0, 0f);
                return;
            }
            ForceState(hitStateName);
            return;
        }

        if (!string.IsNullOrEmpty(slashTrigger))
        {
            FireSlashTrigger();
        }
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
