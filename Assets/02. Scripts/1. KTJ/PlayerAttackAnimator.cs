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

    private void Awake()
    {
        if (dashController == null) dashController = GetComponent<SlashDashController>();
        if (dashController == null) dashController = GetComponentInParent<SlashDashController>();
        if (autoSlash == null) autoSlash = GetComponent<AutoSlashController>();
        if (autoSlash == null) autoSlash = GetComponentInParent<AutoSlashController>();
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (autoSlash != null)
        {
            autoSlash.OnAttackReady += HandleAttackReady;
        }
        if (dashController == null) return;
        dashController.OnDashImpact += HandleDashImpact;
    }

    private void OnDisable()
    {
        if (autoSlash != null)
        {
            autoSlash.OnAttackReady -= HandleAttackReady;
        }
        if (dashController == null) return;
        dashController.OnDashImpact -= HandleDashImpact;
    }

    private void HandleAttackReady()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(readyTrigger)) return;
        animator.SetTrigger(readyTrigger);
    }

    private void HandleDashImpact()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(slashTrigger)) return;
        animator.SetTrigger(slashTrigger);
    }
}
