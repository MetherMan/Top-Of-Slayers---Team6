using UnityEngine;

public class PlayerAttackAnimator : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private SlashDashController dashController;
    [SerializeField] private Animator animator;

    [Header("파라미터")]
    [SerializeField] private string attackTrigger = "Attack";

    private void Awake()
    {
        if (dashController == null) dashController = GetComponent<SlashDashController>();
        if (dashController == null) dashController = GetComponentInParent<SlashDashController>();
        if (animator == null) animator = GetComponent<Animator>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        if (dashController == null) return;
        dashController.OnDashStarted += HandleDashStarted;
    }

    private void OnDisable()
    {
        if (dashController == null) return;
        dashController.OnDashStarted -= HandleDashStarted;
    }

    private void HandleDashStarted()
    {
        if (animator == null) return;
        if (string.IsNullOrEmpty(attackTrigger)) return;
        animator.SetTrigger(attackTrigger);
    }
}
